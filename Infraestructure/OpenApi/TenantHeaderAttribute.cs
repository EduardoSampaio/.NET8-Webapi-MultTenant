using Infraestructure.Tenancy;

namespace Infraestructure.OpenApi
{
    public class TenantHeaderAttribute : SwaggerHeaderAttribute
    {
        public TenantHeaderAttribute()
            : base(TenancyConstants.TenantIdName, 
                  "Enter your tenant name to access this API.",
                  string.Empty,
                  isRequired: true)
        {
        }
    }
}
