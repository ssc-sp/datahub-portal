using System;
using System.Globalization;

namespace NRCan.Datahub.Metadata
{
    public class FieldChoice
    {
        public string ChoiceId { get; set; }
        public string LabelEnglish { get; set; }
        public string LabelFrench { get; set; }
        
        public string Label => IsFrenchCulture() ? LabelFrench : LabelEnglish;

        static bool IsFrenchCulture() => CultureInfo.CurrentCulture.Name.StartsWith("fr", StringComparison.InvariantCulture);
    }
}
