using System;
using System.Globalization;

namespace NRCan.Datahub.Metadata
{
    public class FieldChoice
    {
        public int Id { get; set; }
        public int FieldDefinitionId { get; set; }
        public virtual FieldDefinition FieldDefinition { get; set; }
        public string Value { get; set; }
        public string LabelEnglish { get; set; }
        public string LabelFrench { get; set; }

        #region Entity extensions

        public string Label => IsFrenchCulture() ? LabelFrench : LabelEnglish;
        static bool IsFrenchCulture() => CultureInfo.CurrentCulture.Name.StartsWith("fr", StringComparison.InvariantCulture);

        #endregion
    }
}
