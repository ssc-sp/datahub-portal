using System;
using System.Globalization;

namespace Datahub.Metadata.Model
{
    public class FieldChoice
    {
        public int FieldChoiceId { get; set; }
        public int FieldDefinitionId { get; set; }
        public virtual FieldDefinition FieldDefinition { get; set; }
        public string Value_TXT { get; set; }
        public string Label_English_TXT { get; set; }
        public string Label_French_TXT { get; set; }

        #region Entity extensions

        public string Label => (IsFrenchCulture() ? Label_French_TXT : Label_English_TXT) ?? Label_English_TXT ?? Value_TXT;
        static bool IsFrenchCulture() => CultureInfo.CurrentCulture.Name.StartsWith("fr", StringComparison.InvariantCulture);

        #endregion
    }
}
