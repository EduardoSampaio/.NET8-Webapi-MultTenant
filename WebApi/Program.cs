
using Infraestructure;
using Application;
using System.Threading.Tasks;

namespace WebApi;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();

        builder.Services.AddInfrastructure(builder.Configuration);

        var jwtSettings = builder.Services.GetJwtSettings(builder.Configuration);

        builder.Services.AddJwtAuthentication(jwtSettings);

        builder.Services.AddApplicationService();

        var app = builder.Build();

        await app.Services.AddDatabaseInitializeAsync();

        app.UseHttpsRedirection();

        app.UseInfrastructure();

        app.MapControllers();

        app.Run();
    }
}
