using Datahub.Core.Services.Docs;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.Utils
{
    public static class CertValidationUtils
    {
        /// <summary>
        /// Add a HttpClient with a trusted certificate.
        /// See more details on https://github.com/dotnet/runtime/issues/39835
        /// and https://stackoverflow.com/questions/64905704/using-custom-root-ca-with-httpclient
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IHttpClientBuilder AddHttpClientWithTrustedCertificate<T>(this IServiceCollection services) where T : class
        {
            var builder = services
                .AddHttpClient<T>()
                .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = ServerCertificateCustomValidation;
                return handler;
            });
            return builder;
        }


        /// <summary>
        /// Add a HttpClient with a trusted certificate.
        /// See more details on https://github.com/dotnet/runtime/issues/39835
        /// and https://stackoverflow.com/questions/64905704/using-custom-root-ca-with-httpclient
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IHttpClientBuilder AddHttpClientWithTrustedCertificate(this IServiceCollection services, string name)
        {
            var builder = services
                .AddHttpClient(name)
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    var handler = new HttpClientHandler();
                    handler.ServerCertificateCustomValidationCallback = ServerCertificateCustomValidation;
                    return handler;
                });
            return builder;
        }

        private static bool ServerCertificateCustomValidation(HttpRequestMessage requestMessage, X509Certificate2 certificate,
            X509Chain chain, SslPolicyErrors sslErrors)
        {
            if (certificate == null) return false;
            if (chain == null) return false;
            //chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
            //chain.ChainPolicy.CustomTrustStore.Add(GetTrustedRootCert());            
            //return chain.Build(certificate);

            // Missing cert or the destination hostname wasn't valid for the cert.
            if ((sslErrors & ~SslPolicyErrors.RemoteCertificateChainErrors) != 0)
            {
                return false;
            }

            chain.ChainPolicy.CustomTrustStore.Clear();
            chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
            chain.ChainPolicy.CustomTrustStore.Add(GetTrustedRootCert());
            return chain.Build(certificate);
        }

        private static X509Certificate2 GetTrustedRootCert()
        {
            // load the certificate from embedded resource
            var assembly = typeof(CertValidationUtils).Assembly;
            var resourceName = "Datahub.Core.Utils.Certs.gc1.cer";
            using var stream = assembly.GetManifestResourceStream(resourceName);
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            var cert = new X509Certificate2(bytes);
            return cert;

        }
    }
}
