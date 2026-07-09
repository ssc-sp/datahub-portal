using System.Text.RegularExpressions;

namespace Datahub.Functions.Validators
{
	public static partial class EmailValidator
    {
        public static bool IsValidEmail(string email)
        {
            return !string.IsNullOrWhiteSpace(email) && EmailRegex().IsMatch(email);
        }

        [GeneratedRegex(@"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture)]
        private static partial Regex EmailRegex();
    }
}
