using Finbuckle.MultiTenant.Abstractions;

namespace Infraestructure.Tenancy;
public class ABCSchoolTenantInfo : ITenantInfo
{
    public string Id { get; set; }
    public string Identifier { get; set; }
    public string Name { get; set; }
    public string ConnectionString { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime ValiUpTo { get; set; }
    public bool IsActive { get; set; }
}
