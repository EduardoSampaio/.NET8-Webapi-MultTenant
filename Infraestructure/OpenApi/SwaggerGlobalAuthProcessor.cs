using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Namotion.Reflection;
using NSwag.Generation.AspNetCore;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infraestructure.OpenApi
{
    public class SwaggerGlobalAuthProcessor(string scheme) : IOperationProcessor
    {
        private readonly string _scheme = scheme;

        public SwaggerGlobalAuthProcessor(): this(JwtBearerDefaults.AuthenticationScheme)
        {
            
        }

        public bool Process(OperationProcessorContext context)
        {
            var list = ((AspNetCoreOperationProcessorContext)context)
                .ApiDescription.ActionDescriptor.TryGetPropertyValue<IList<object>>("EndpointMetadata") as List<object>;

            if (list is not null)
            {
                if(list.OfType<AllowAnonymousAttribute>().Any())
                {
                    return true;
                }

                if(context.OperationDescription.Operation.Security.Count == 0)
                {
                    (context.OperationDescription.Operation.Security ??= [])
                        .Add(new NSwag.OpenApiSecurityRequirement
                        {
                            { 
                                _scheme,
                                Array.Empty<string>()
                            }
                        });
                }
            }

            return true;
        }
    }

    public static class ObjectExtensions
    {
        public static T TryGetPropertyValue<T>(this object obj, string propertyName, T defaultValue = default)
        {
            var property = obj.GetType().GetProperty(propertyName);
            if (property is not null)
            {
                return (T)property.GetValue(obj);
            }
            return defaultValue;
        }
    }
}
