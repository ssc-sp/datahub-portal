using Datahub.Core.Model.Datahub;
using Microsoft.Extensions.Localization;

namespace Datahub.Portal.Pages.Workspace.Publishing
{
    public static class PublicationDisplayUtil
    {
        public static string SelectableDatasetTitle(this OpenDataSubmission s, IStringLocalizer Localizer) => 
            string.IsNullOrWhiteSpace(s.DatasetTitle) ?
                Localizer["(untitled dataset {0})", s.Id] :
                s.DatasetTitle;
    }
}
