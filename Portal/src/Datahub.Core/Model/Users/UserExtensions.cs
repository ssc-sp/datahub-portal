using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Datahub.Core.Extensions;

namespace Datahub.Core.Model.Users
{
    public static class UserExtensions
    {
        public static string? UserUID(this PortalUser user)
        {
            if (user.EntraUser?.GraphGuid != null)
            {
                return user.EntraUser.GraphGuid;
            }
            else if (user.ExternalUser?.ExternalSubject != null)
            {
                return user.ExternalUser.ExternalSubject?.ToString();
            }
            return null;
        }

        public const string ENTRA = "A";
        public const string EXTERNAL = "E";

        public static string UserProfileLink(this PortalUser user)
        {
            if (user.EntraUser != null)
            {
                return $"{ENTRA}-{user.EntraUser.GraphGuid}".Base64Encode();
            }
            else if (user.ExternalUser != null)
            {
                return $"{EXTERNAL}-{user.ExternalUser.ExternalSubject}".Base64Encode();
            }
            return string.Empty;
        }

        /// <summary>
        /// Decodes a Base64-encoded profile link (produced by <see cref="UserProfileLink"/>)
        /// and returns a tuple where the first item is the Entra user id (if present)
        /// and the second item is the External user id (if present).
        /// </summary>
        /// <param name="encoded">The Base64-encoded profile link.</param>
        /// <returns>Tuple of (entraId, externalId) where one will be null depending on the type.</returns>
        public static (string? Entra, string? External) DecodeUserProfileLink(this string encoded)
        {
            if (string.IsNullOrWhiteSpace(encoded)) return (null, null);

            try
            {
                var trimmed = encoded.Trim();
                var decoded = trimmed.Base64Decode();
                if (decoded.StartsWith($"{ENTRA}-", StringComparison.Ordinal))
                {
                    var value = decoded.Substring(ENTRA.Length + 1);
                    return (string.IsNullOrEmpty(value) ? null : value, null);
                }
                else if (decoded.StartsWith($"{EXTERNAL}-", StringComparison.Ordinal))
                {
                    var value = decoded.Substring(EXTERNAL.Length + 1);
                    return (null, string.IsNullOrEmpty(value) ? null : value);
                }
            }
            catch (FormatException)
            {
                // invalid base64
            }
            catch (ArgumentException)
            {
                // invalid input for Base64 conversion
            }

            return (null, null);
        }
    }
}
