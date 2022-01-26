﻿using Datahub.Metadata.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Datahub.Metadata.DTO
{
    /// <summary>
    /// DTO to transfer object metadata field values
    /// </summary>
    public class FieldValueContainer : List<ObjectFieldValue>
    {
        public FieldValueContainer(string objectId, FieldDefinitions definitions, IEnumerable<ObjectFieldValue> values) : base()
        {
            ObjectId = objectId;
            Definitions = definitions;
            AddRange(values);
        }

        public string ObjectId { get; init; }
        public FieldDefinitions Definitions { get; init; }
        public ObjectFieldValue this[string fieldName] => GetFieldValueByName(fieldName);
        public string GetValue(string fieldName, string defaultValue = "") => this[fieldName]?.Value_TXT ?? defaultValue;

        public char ChoiceSeparator => '|';

        public IEnumerable<FieldChoice> GetSelectedChoices(string fieldName)
        {
            var fieldValue = this[fieldName];
            var choiceValues = (fieldValue?.Value_TXT ?? "").Split(ChoiceSeparator, StringSplitOptions.RemoveEmptyEntries);
            foreach (var choiceValue in choiceValues)
            {
                var choice = fieldValue.FieldDefinition.Choices.FirstOrDefault(c => choiceValue == c.Value_TXT);
                if (choice is not null)
                    yield return choice;
            }
        }

        public bool ValidateRequired(Func<FieldDefinition, bool> isRequired)
        {
            var map = this.ToDictionary(fv => fv.FieldDefinitionId);

            var findValidValue = (int id) => map.TryGetValue(id, out ObjectFieldValue value) && !string.IsNullOrEmpty(value.Value_TXT);
            var passRequired = (FieldDefinition f) => !isRequired.Invoke(f) || findValidValue(f.FieldDefinitionId);

            return Definitions.Fields.All(passRequired);
        }

        public ObjectFieldValue SetValue(string fieldName, string fieldValue)
        {
            if (Definitions is null)
                throw new ArgumentNullException(nameof(Definitions));

            var definition = Definitions.Get(fieldName);
            if (definition is null)
                throw new ArgumentNullException(nameof(fieldName));

            var fieldValueObj = this.FirstOrDefault(v => v.FieldDefinitionId == definition.FieldDefinitionId);
            if (fieldValueObj is not null)
            {
                fieldValueObj.Value_TXT = fieldValue;
            }
            else
            {
                fieldValueObj = new() { FieldDefinitionId = definition.FieldDefinitionId, Value_TXT = fieldValue };
                Add(fieldValueObj);    
            }

            return fieldValueObj;
        }

        public FieldValueContainer GetReadonlyCopy()
        {
            return new FieldValueContainer(ObjectId, Definitions, CloneFieldsReadonly());
        }

        public string GetIndexableValues(string separator, bool english, params string[] fieldNames)
        {
            List<string> values = new();
            foreach (var fieldName in fieldNames)
            {
                var fieldValue = GetFieldValueByName(fieldName);
                if (fieldValue is not null)
                {
                    if (fieldValue.FieldDefinition.HasChoices)
                    {
                        values.Add(fieldValue.FieldDefinition.GetChoiceTextValue(fieldValue.Value_TXT, english));
                    }
                    else
                    {
                        values.Add(fieldValue.Value_TXT);
                    }
                }
            }
            return string.Join(separator, values);
        }

        public Dictionary<string, string> GetValues(bool english, params string[] fieldNames)
        {
            Dictionary<string, string> values = new();
            foreach (var fieldName in fieldNames)
            {
                var fieldValue = GetFieldValueByName(fieldName);
                if (fieldValue is not null)
                {
                    if (fieldValue.FieldDefinition.HasChoices)
                    {
                        values[fieldName] = fieldValue.FieldDefinition.GetChoiceTextValue(fieldValue.Value_TXT, english);
                    }
                    else
                    {
                        values[fieldName] = fieldValue.Value_TXT;
                    }
                }
            }
            return values;
        }

        private IEnumerable<ObjectFieldValue> CloneFieldsReadonly()
        {
            foreach (var fieldValue in this)
            {
                var cloned = fieldValue.Clone();
                cloned.FieldDefinition ??= Definitions.Get(fieldValue.FieldDefinitionId);
                yield return cloned;
            }
        }

        private ObjectFieldValue GetFieldValueByName(string fieldName)
        {
            var definitionId = Definitions?.Get(fieldName)?.FieldDefinitionId;
            return definitionId.HasValue ? this.FirstOrDefault(v => v.FieldDefinitionId == definitionId.Value) : null;
        }
    }
}
