namespace Datahub.Core.Data.External
{
    public class BilingualText
    {
        public string En { get; set; }
        public string Fr { get; set; }

        public string GetString(bool isFrench)
        {
            return isFrench? Fr: En;
        }
    }
}