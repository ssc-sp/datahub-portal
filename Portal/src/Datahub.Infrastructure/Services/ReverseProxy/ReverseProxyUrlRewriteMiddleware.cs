using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Datahub.Application.Services.ReverseProxy;

namespace Datahub.Infrastructure.Services.ReverseProxy
{
    /// <summary>
    /// Middleware that rewrites root-absolute URLs in proxied responses to include the workspace web app prefix.
    /// Only applies to requests under the configured reverse proxy prefix (e.g. /w/{acronym}).
    /// Supports decompressing gzip/br/deflate responses for rewriting, then re-compressing with the same encoding.
    /// </summary>
    public class ReverseProxyUrlRewriteMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IReverseProxyConfigService _configService;
        private readonly ILogger<ReverseProxyUrlRewriteMiddleware> _logger;

        public ReverseProxyUrlRewriteMiddleware(RequestDelegate next,
            IReverseProxyConfigService configService,
            ILogger<ReverseProxyUrlRewriteMiddleware> logger)
        {
            _next = next;
            _configService = configService;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            // Only process requests under the reverse proxy prefix: /{prefix}/{acronym}/...
            var path = context.Request.Path.Value ?? string.Empty;
            var expectedPrefix = "/" + _configService.WebAppPrefix + "/";
            if (!path.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            // Extract the acronym from the path: /{prefix}/{acronym}/...
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length < 2)
            {
                await _next(context);
                return;
            }
            var acronym = segments[1];
            var workspaceBase = _configService.BuildWebAppURL(acronym, routeInfo: true); // e.g. "/w/{acr}"

            // Swap the response body to capture data written by YARP and rewrite it
            var originalBody = context.Response.Body;
            await using var buffer = new MemoryStream();
            context.Response.Body = buffer;

            try
            {
                await _next(context);

                // Only rewrite on success-ish codes and text-like content
                var statusCode = context.Response.StatusCode;
                var contentType = context.Response.ContentType ?? string.Empty;
                var contentEncoding = context.Response.Headers["Content-Encoding"].ToString();

                if (statusCode >= 200 && statusCode < 400 && IsTextContent(contentType))
                {
                    buffer.Position = 0;
                    var textEncoding = GetEncodingFromContentType(contentType) ?? Encoding.UTF8;

                    // Decompress if needed
                    byte[] bodyBytes;
                    if (!string.IsNullOrEmpty(contentEncoding))
                    {
                        var lower = contentEncoding.ToLowerInvariant();
                        if (lower.Contains("gzip"))
                        {
                            using var gzip = new GZipStream(buffer, CompressionMode.Decompress, leaveOpen: true);
                            using var ms = new MemoryStream();
                            await gzip.CopyToAsync(ms, context.RequestAborted);
                            bodyBytes = ms.ToArray();
                        }
                        else if (lower.Contains("br"))
                        {
                            using var br = new BrotliStream(buffer, CompressionMode.Decompress, leaveOpen: true);
                            using var ms = new MemoryStream();
                            await br.CopyToAsync(ms, context.RequestAborted);
                            bodyBytes = ms.ToArray();
                        }
                        else if (lower.Contains("deflate"))
                        {
                            using var deflate = new DeflateStream(buffer, CompressionMode.Decompress, leaveOpen: true);
                            using var ms = new MemoryStream();
                            await deflate.CopyToAsync(ms, context.RequestAborted);
                            bodyBytes = ms.ToArray();
                        }
                        else
                        {
                            // Unknown encoding, pass-through
                            buffer.Position = 0;
                            await buffer.CopyToAsync(originalBody, context.RequestAborted);
                            return;
                        }
                    }
                    else
                    {
                        using var ms = new MemoryStream();
                        await buffer.CopyToAsync(ms, context.RequestAborted);
                        bodyBytes = ms.ToArray();
                    }

                    var body = textEncoding.GetString(bodyBytes);
                    var rewritten = RewriteBody(body, workspaceBase);
                    var rewrittenBytes = textEncoding.GetBytes(rewritten);

                    // Re-compress if needed
                    byte[] outputBytes;
                    if (!string.IsNullOrEmpty(contentEncoding))
                    {
                        var lower = contentEncoding.ToLowerInvariant();
                        using var outMs = new MemoryStream();
                        if (lower.Contains("gzip"))
                        {
                            using (var gzipOut = new GZipStream(outMs, CompressionLevel.Fastest, leaveOpen: true))
                            {
                                await gzipOut.WriteAsync(rewrittenBytes, 0, rewrittenBytes.Length, context.RequestAborted);
                            }
                            outputBytes = outMs.ToArray();
                        }
                        else if (lower.Contains("br"))
                        {
                            using (var brOut = new BrotliStream(outMs, CompressionLevel.Fastest, leaveOpen: true))
                            {
                                await brOut.WriteAsync(rewrittenBytes, 0, rewrittenBytes.Length, context.RequestAborted);
                            }
                            outputBytes = outMs.ToArray();
                        }
                        else if (lower.Contains("deflate"))
                        {
                            using (var deflateOut = new DeflateStream(outMs, CompressionLevel.Fastest, leaveOpen: true))
                            {
                                await deflateOut.WriteAsync(rewrittenBytes, 0, rewrittenBytes.Length, context.RequestAborted);
                            }
                            outputBytes = outMs.ToArray();
                        }
                        else
                        {
                            // Unknown encoding, pass-through as original compressed
                            buffer.Position = 0;
                            await buffer.CopyToAsync(originalBody, context.RequestAborted);
                            return;
                        }
                        context.Response.Headers["Content-Encoding"] = lower;
                        outputBytes = outMs.ToArray();
                    }
                    else
                    {
                        // Ensure Content-Encoding header is cleared if not compressed
                        context.Response.Headers.Remove("Content-Encoding");
                        outputBytes = rewrittenBytes;
                    }

                    context.Response.ContentLength = outputBytes.Length;

                    // Write out
                    await originalBody.WriteAsync(outputBytes, 0, outputBytes.Length, context.RequestAborted);
                }
                else
                {
                    // Pass-through unmodified
                    buffer.Position = 0;
                    await buffer.CopyToAsync(originalBody, context.RequestAborted);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "ReverseProxy URL rewrite skipped due to exception");
                // Best-effort: flush original content
                buffer.Position = 0;
                await buffer.CopyToAsync(originalBody, context.RequestAborted);
            }
            finally
            {
                context.Response.Body = originalBody;
            }
        }

        private static bool IsTextContent(string contentType)
        {
            contentType = contentType.ToLowerInvariant();
            return contentType.StartsWith("text/")
                   || contentType.Contains("json")
                   || contentType.Contains("javascript")
                   || contentType.Contains("xml")
                   || contentType.Contains("html")
                   || contentType.Contains("css");
        }

        private static Encoding? GetEncodingFromContentType(string contentType)
        {
            // Very simple charset parser
            var idx = contentType.IndexOf("charset=", StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                var charset = contentType[(idx + 8)..].Trim().Trim('"', '\'', ';');
                try
                {
                    return Encoding.GetEncoding(charset);
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

        private static string RewriteBody(string body, string workspaceBase)
        {
            // workspaceBase comes like "/w/{acr}" (no trailing slash)
            // Build regexes that replace root-absolute references not already starting with the workspaceBase
            var escapedBase = Regex.Escape(workspaceBase);

            // 1) HTML attributes href|src|action|content="/..." (and single quotes)
            body = Regex.Replace(body,
                pattern: $@"\b(href|src|action|content)\s*=\s*""(?!(?:{escapedBase}))/",
                replacement: $"$1=\"{workspaceBase}/",
                options: RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            body = Regex.Replace(body,
                pattern: $@"\b(href|src|action|content)\s*=\s*'(?!(?:{escapedBase}))/",
                replacement: $"$1='{workspaceBase}/",
                options: RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            // 2) CSS url(/...)
            body = Regex.Replace(body,
                pattern: $@"url\(\s*(?!(?:{escapedBase}))/",
                replacement: $"url({workspaceBase}/",
                options: RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            // 3) JavaScript string patterns commonly used in SPAs fetch('/...'), location.href='/...'
            body = Regex.Replace(body,
                pattern: $@"(fetch\s*\(|location\.href\s*=\s*)'(?!{escapedBase})/",
                replacement: $"$1'{workspaceBase}/",
                options: RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            body = Regex.Replace(body,
                pattern: $@"(fetch\s*\(|location\.href\s*=\s*)""(?!{escapedBase})/",
                replacement: $"$1\"{workspaceBase}/",
                options: RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            return body;
        }
    }
}
