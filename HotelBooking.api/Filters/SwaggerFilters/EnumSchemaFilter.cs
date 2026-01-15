using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        // Chỉ xử lý nếu type là Enum
        if (context.Type.IsEnum)
        {
            var array = new OpenApiArray();
            var names = Enum.GetNames(context.Type);
            var values = Enum.GetValues(context.Type);

            // Tạo danh sách mô tả: "1 = Service", "2 = Policy"
            var fullDescription = new List<string>();

            for (int i = 0; i < names.Length; i++)
            {
                // Lấy giá trị số (int)
                var intValue = (int)values.GetValue(i);
                var name = names[i];

                fullDescription.Add($"{intValue} = {name}");
            }

            // Ghi đè vào mô tả của Swagger (Dùng <br/> để xuống dòng trong Swagger UI)
            schema.Description += "<p>Values:</p><ul><li>" +
                                  string.Join("</li><li>", fullDescription) +
                                  "</li></ul>";
        }
    }
}