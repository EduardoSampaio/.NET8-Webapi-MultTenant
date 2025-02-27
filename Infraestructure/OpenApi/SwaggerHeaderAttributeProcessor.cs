using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using System.Reflection;
using NJsonSchema;

namespace Infraestructure.OpenApi
{
    public class SwaggerHeaderAttributeProcessor : IOperationProcessor
    {
        public bool Process(OperationProcessorContext context)
        {
            if (context.MethodInfo.GetCustomAttribute(typeof(SwaggerHeaderAttribute)) is SwaggerHeaderAttribute swaggerHeader)
            {
                var parameters = context.OperationDescription.Operation.Parameters;

                var existingParameter = parameters.FirstOrDefault(p => p.Name == swaggerHeader.HeaderName && p.Kind == NSwag.OpenApiParameterKind.Header);

                if (existingParameter is not null)
                {
                    parameters.Remove(existingParameter);
                }

                parameters.Add(new NSwag.OpenApiParameter
                {
                    Name = swaggerHeader.HeaderName,
                    Kind = NSwag.OpenApiParameterKind.Header,
                    Description = swaggerHeader.Description,
                    Default = swaggerHeader.DefaultValue,
                    IsRequired = true,
                    Schema = new JsonSchema
                    {
                        Type = JsonObjectType.String,
                        Default = swaggerHeader.DefaultValue,
                    },

                });
            }
            return true;
        }
    }
}
