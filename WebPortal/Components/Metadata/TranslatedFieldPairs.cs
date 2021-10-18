using Datahub.Metadata.Model;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;

namespace Datahub.Portal.Components.Metadata
{
    public record KeywordArgs(ObjectFieldValue Source, string Keyword);
    
    public class TranslatedFieldPair
    {
        public string RootName { get; init; }
        public ObjectFieldValue FieldEnglish {  get; set; }
        public ObjectFieldValue FieldFrench { get; set; }
        public EventHandler<KeywordArgs> OnKeywordPicked { get; set; }
        public EventHandler<KeywordArgs> OnKeywordDeleted { get; set; }

        public ObjectFieldValue GetPaired(ObjectFieldValue value)
        {
            return FieldEnglish == value ? FieldFrench : FieldEnglish;
        }
    }

    public class TranslatedFieldPairs
    {
        private readonly Dictionary<string, TranslatedFieldPair> _pairs;

        public TranslatedFieldPairs()
        {
            _pairs = new Dictionary<string, TranslatedFieldPair>();
        }

        public TranslatedFieldPair BindField(string fieldName, ObjectFieldValue value)
        {
            if (!IsTranslatedField(fieldName))
                return null;

            var rootName = GetFieldNameRoot(fieldName);
            
            if (!_pairs.TryGetValue(rootName, out var pair))
            {
                pair = new TranslatedFieldPair() { RootName = rootName };
                _pairs[rootName] = pair;
            }

            switch (GetFieldLanguage(fieldName)) 
            {
                case "en": 
                    pair.FieldEnglish = value;
                    break;
                case "fr":
                    pair.FieldFrench = value;
                    break;
            };

            return pair;
        }

        static bool IsTranslatedField(string fieldName) => fieldName?.EndsWith("_en") == true || fieldName?.EndsWith("_fr") == true;
        static string GetFieldNameRoot(string fieldName) => fieldName[0..^3];
        static string GetFieldLanguage(string fieldName) => fieldName[^2..];
    }
}
