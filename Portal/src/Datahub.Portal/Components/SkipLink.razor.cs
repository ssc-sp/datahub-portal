namespace Datahub.Portal.Components
{
    public enum SkipLinkType
    {
        MainContent,
        Navigation
    }

    public partial class SkipLink
    {
        public const string MainContentId = "maincontent";
        public const string NavigationId = "nav";
        
        public static string GetSkipLinkId(SkipLinkType type)
        {
            return type switch
            {
                SkipLinkType.MainContent => MainContentId,
                SkipLinkType.Navigation => NavigationId,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public string GetSkipLinkText(SkipLinkType type)
        {
            return type switch
            {
                SkipLinkType.MainContent => Localizer["Skip to main content"],
                SkipLinkType.Navigation => Localizer["Skip to navigation"],
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static SkipLinkType GetSkipLinkType(string id)
        {
            return id switch
            {
                MainContentId => SkipLinkType.MainContent,
                NavigationId => SkipLinkType.Navigation,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        public static List<string> GetAvailableSkipLinks()
        {
            return Enum.GetValues<SkipLinkType>().Select(GetSkipLinkId).ToList();
        }
    }
}