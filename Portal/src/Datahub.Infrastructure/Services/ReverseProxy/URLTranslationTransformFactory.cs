using Datahub.Application.Services.ReverseProxy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace Datahub.Infrastructure.Services.ReverseProxy;

public static class URLTranslationTransformHelper
{
    public static RouteConfig WithTransformRewriteURLsInBody(this RouteConfig routeConfig, string prefix)
    {
        // Ensure prefix ends with a slash
        routeConfig.WithTransform(transform => {
            transform[IReverseProxyConfigService.WorkspacePrefix] = prefix;
        });
        return routeConfig;
    }
}

public class URLTranslationTransformFactory : ITransformFactory
{

    private readonly ILogger logger;

    public URLTranslationTransformFactory(ILogger<URLTranslationTransformFactory> logger)
    {
        this.logger = logger;
    }

    public bool Build(TransformBuilderContext context, IReadOnlyDictionary<string, string> transformValues)
    {
        if (transformValues.TryGetValue(IReverseProxyConfigService.WorkspacePrefix, out var workspacePrefix))
        {
            // Normalize prefix once: ensure it starts with a single '/'
            var normalizedPrefix = "/" + workspacePrefix.Trim('/');

            // Remove the workspace prefix from the request path
            context.AddPathRemovePrefix(new PathString(normalizedPrefix));

            context.AddResponseTransform(async responseContext =>
            {
                if (responseContext.ProxyResponse?.Content is null)
                {
                    return;
                }

                // Only attempt to rewrite for text-like content
                var mediaType = responseContext.ProxyResponse.Content.Headers.ContentType?.MediaType ?? string.Empty;
                var isTextLike =
                    mediaType.Contains("text/html", StringComparison.OrdinalIgnoreCase) ||
                    mediaType.Contains("text/css", StringComparison.OrdinalIgnoreCase) ||
                    mediaType.Contains("application/javascript", StringComparison.OrdinalIgnoreCase) ||
                    mediaType.Contains("text/javascript", StringComparison.OrdinalIgnoreCase) ||
                    mediaType.Contains("application/json", StringComparison.OrdinalIgnoreCase);

                if (!isTextLike)
                {
                    return;
                }

                // Determine encoding if provided, default to UTF8
                Encoding encoding = Encoding.UTF8;
                var charset = responseContext.ProxyResponse.Content.Headers.ContentType?.CharSet;
                if (!string.IsNullOrWhiteSpace(charset))
                {
                    try { encoding = Encoding.GetEncoding(charset); } catch { /* fallback to UTF8 */ }
                }

                var stream = await responseContext.ProxyResponse.Content.ReadAsStreamAsync();
                using var reader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: true, leaveOpen: false);

                // TODO: size limits, timeouts
                var body = await reader.ReadToEndAsync();

                if (!string.IsNullOrEmpty(body))
                {
                    responseContext.SuppressResponseBody = true;

                    // If JSON/JS config contains: requests_pathname_prefix: "/" -> set to empty string ""
                    body = Regex.Replace(
                        body,
                        "(?<key>\\\"requests_pathname_prefix\\\"|requests_pathname_prefix)\\s*:\\s*(?<q>[\"'])\\s*/\\s*\\k<q>",
                        m =>
                        {
                            var key = m.Groups["key"].Value;
                            var q = m.Groups["q"].Value;
                            return $"{key}: {q}{normalizedPrefix}/{q}"; // empty string value
                        },
                        RegexOptions.Compiled);

                    // HTML attribute URL rewriter: href/src/action/data-*/poster/etc.
                    // Instead of prefixing with the workspace prefix, strip a single leading slash
                    body = Regex.Replace(
                        body,
                        "(?<attr>\\b(?:href|src|action|poster|formaction|data-[a-z0-9_-]+)\\s*=\\s*)([\"'])(?<url>[^\"']+)([\"'])",
                        m =>
                        {
                            var attr = m.Groups["attr"].Value;
                            var quote = m.Groups[2].Value;
                            var url = m.Groups["url"].Value;

                            // Leave protocol-relative (//) and absolute (http/https) as-is
                            if (url.StartsWith("//") || url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                                return attr + quote + url + quote;

                            // Strip a single leading slash for root-absolute paths
                            if (url.StartsWith("/", StringComparison.Ordinal) && (url.Length == 1 || url[1] != '/'))
                            {
                                var newUrl = url.TrimStart('/');
                                return attr + quote + newUrl + quote;
                            }

                            // Leave relative as-is
                            return attr + quote + url + quote;
                        },
                        RegexOptions.IgnoreCase | RegexOptions.Compiled);

                    // CSS url(...) rewriter: strip a single leading slash from root-absolute paths, leave protocol-relative
                    body = Regex.Replace(
                        body,
                        "url\\(\\s*([\"']?)(?<url>[^\"')]+)\\1\\s*\\)",
                        m =>
                        {
                            var url = m.Groups["url"].Value;

                            if (url.StartsWith("//") || url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                                return $"url({url})";

                            if (url.StartsWith("/", StringComparison.Ordinal) && (url.Length == 1 || url[1] != '/'))
                            {
                                var newUrl = url.TrimStart('/');
                                return $"url({newUrl})";
                            }

                            return $"url({url})";
                        },
                        RegexOptions.IgnoreCase | RegexOptions.Compiled);

                    // Ensure base href ends with a trailing slash for correct relative resolution
                    var baseHref = normalizedPrefix.EndsWith('/') ? normalizedPrefix : normalizedPrefix + "/";

                    // If HTML, inject a <base href="..."> into <head> if not already present
                    if (mediaType.Contains("text/html", StringComparison.OrdinalIgnoreCase))
                    {
                        var hasBase = Regex.IsMatch(body, "<base\\s+href=", RegexOptions.IgnoreCase);
                        if (!hasBase)
                        {
                            // Insert right after the opening <head> tag if present, else before first <meta> or at start of <html>
                            var headMatch = Regex.Match(body, "<head[^>]*>", RegexOptions.IgnoreCase);
                            if (headMatch.Success)
                            {
                                body = body.Insert(headMatch.Index + headMatch.Length, $"<base href=\"{baseHref}\">\n");
                            }
                            else
                            {
                                // Fallback: insert as the first element in the document
                                var htmlMatch = Regex.Match(body, "<html[^>]*>", RegexOptions.IgnoreCase);
                                if (htmlMatch.Success)
                                {
                                    // If <head> is missing, create one after <html>
                                    var insertPos = htmlMatch.Index + htmlMatch.Length;
                                    body = body.Insert(insertPos, $"\n<head><base href=\"{baseHref}\"></head>\n");
                                }
                                else
                                {
                                    // As a last resort, prepend
                                    body = $"<base href=\"{baseHref}\">\n" + body;
                                }
                            }
                        }
                    }

                    var bytes = encoding.GetBytes(body);
                    responseContext.HttpContext.Response.ContentLength = bytes.Length;
                    await responseContext.HttpContext.Response.Body.WriteAsync(bytes);
                }

            });
        }
        return true;
    }

    public bool Validate(TransformRouteValidationContext context, IReadOnlyDictionary<string, string> transformValues)
    {
        return true;
    }
}

