// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
// cloned from https://github.com/DuendeArchive/IdentityModel/blob/6f9e050167846724828138ba6ee8b626eb31c669/src/Client/BasicAuthenticationOAuthHeaderValue.cs

using System.Net.Http.Headers;
using System.Text;

namespace Datahub.Portal.Services.Auth
{

    public static class BasicAuthenticationHelper
    {
        /// <summary>
        /// Sets a basic authentication header for RFC6749 client authentication.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        public static void SetBasicAuthenticationOAuth(this HttpClient client, string userName, string password)
        {
            client.DefaultRequestHeaders.Authorization = new BasicAuthenticationOAuthHeaderValue(userName, password);
        }
    }


    /// <summary>
    /// HTTP Basic Authentication authorization header for RFC6749 client authentication
    /// </summary>
    /// <seealso cref="System.Net.Http.Headers.AuthenticationHeaderValue" />
    public class BasicAuthenticationOAuthHeaderValue : AuthenticationHeaderValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BasicAuthenticationOAuthHeaderValue"/> class.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        public BasicAuthenticationOAuthHeaderValue(string userName, string password)
            : base("Basic", EncodeCredential(userName, password))
        { }

        /// <summary>
        /// Encodes the credential.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">userName</exception>
        public static string EncodeCredential(string userName, string password)
        {
            if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentNullException(nameof(userName));
            if (password == null) password = "";

            Encoding encoding = Encoding.UTF8;
            string credential = $"{UrlEncode(userName)}:{UrlEncode(password)}";

            return Convert.ToBase64String(encoding.GetBytes(credential));
        }

        private static string UrlEncode(string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return String.Empty;
            }

            return Uri.EscapeDataString(value).Replace("%20", "+");
        }
    }
}
