namespace Infraestructure.Tenancy;

public interface ITenantDbSeeder
{
    Task InitializeDatabaseAsync(CancellationToken cancellationToken);
}
