namespace Datahub.Core.Model
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "using lowercase to match response from Azure management API")]
    public class AccessTokenResponse
    {
#nullable enable
        public string? token_type { get; set; }
        public string? expires_in { get; set; }
        public string? ext_expires_in { get; set; }
        public string? expires_on { get; set; }
        public string? not_before { get; set; }
        public string? resource { get; set; }
        public string? access_token { get; set; }
#nullable disable
    }
}
