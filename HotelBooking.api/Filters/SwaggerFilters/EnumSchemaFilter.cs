using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        // Only process if type is Enum
        if (context.Type.IsEnum)
        {
            var array = new OpenApiArray();
            var names = Enum.GetNames(context.Type);
            var values = Enum.GetValues(context.Type);

            // Create description list: "1 = Service", "2 = Policy"
            var fullDescription = new List<string>();

            for (int i = 0; i < names.Length; i++)
            {
                // Get integer value
                var intValue = (int)values.GetValue(i);
                var name = names[i];

                fullDescription.Add($"{intValue} = {name}");
            }

            // Override Swagger description (Use <br/> for line breaks in Swagger UI)
            schema.Description += "<p>Values:</p><ul><li>" +
                                  string.Join("</li><li>", fullDescription) +
                                  "</li></ul>";
        }
    }
}