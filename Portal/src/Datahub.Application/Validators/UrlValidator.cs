using FluentValidation;

namespace Datahub.Application.Validators
{
    public class UrlValidator : AbstractValidator<string>
    {
       public UrlValidator()
       {
           RuleFor(url => url)
               .NotEmpty().WithMessage("Url cannot be empty")
               .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _)).WithMessage("Url is not valid");
       }
    }

    public class GitRepoValidator : AbstractValidator<string>
    {
        public GitRepoValidator()
        {
            RuleFor(url => url)
                .NotEmpty().WithMessage("Url cannot be empty")
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _)).WithMessage("Url is not valid")
                .Must(url => url.EndsWith(".git")).WithMessage("Url must end with .git");
        }
    }
}