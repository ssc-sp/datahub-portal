﻿using Datahub.Metadata.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Datahub.CKAN.Package;

class KeywordFieldAgent : FieldAgent
{
    const string KeywordPrefix = "keywords_";

    readonly Dictionary<string, string[]> _languages = new();

    public override bool Matches(FieldDefinition definition)
    {
        return definition.FieldNameTXT.StartsWith(KeywordPrefix, StringComparison.InvariantCulture);
    }

    public override (bool append, FieldAgent agent) Instantiate(string fieldName, string fieldValue)
    {
        var append = _languages.Count == 0;

        var language = fieldName.Substring(KeywordPrefix.Length);
        _languages[language] = fieldValue.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(k => k.Trim()).ToArray();

        return (append, this);
    }

    public override void RenderField(IDictionary<string, object> data)
    {
        data["keywords"] = _languages;
    }
}