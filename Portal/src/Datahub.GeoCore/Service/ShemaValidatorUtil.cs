using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Reflection;

namespace Datahub.GeoCore.Service;

public static class ShemaValidatorUtil
{
	public static ShemaValidatorResult Validate(string jsonData)
	{
		try
		{
			var schemaData = GetSchemaStream();

			var schema = JSchema.Parse(schemaData);
			var json = JToken.Parse(jsonData);

			// validate json
			bool valid = json.IsValid(schema, out IList<ValidationError> errors);

			var messages = string.Empty;
			if (!valid)
			{
				messages = string.Join("\n", errors.Select(e => e.Message));
			}

			return new(valid, messages);
		}
		catch (Exception ex)
		{
			return new(false, ex.Message);
		}
	}

	static string GetSchemaStream()
	{
		var assembly = Assembly.GetExecutingAssembly();
		var stream = assembly?.GetManifestResourceStream("Datahub.GeoCore.Schema.DatasetShema.json");
		if (stream is not null)
		{
			using var reader = new StreamReader(stream);
			return reader.ReadToEnd();
		}
		return string.Empty;
	}
}