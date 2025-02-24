
using Infraestructure;
using System.Threading.Tasks;

namespace WebApi;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddInfrastructure(builder.Configuration);
        var jwtSettings = builder.Services.GetJwtSettings(builder.Configuration);
        builder.Services.AddJwtAuthentication(jwtSettings);

        var app = builder.Build();

        await app.Services.AddDatabaseInitializeAsync();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.UseInfrastructure();

        app.MapControllers();

        app.Run();
    }
}
