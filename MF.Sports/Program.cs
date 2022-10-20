using Keycloak.AuthServices.Authentication;
using Keycloak.AuthServices.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var host = builder.Host;
var configuration = builder.Configuration;
var services = builder.Services;

host.ConfigureKeycloakConfigurationSource();
// conventional registration from keycloak.json
services.AddKeycloakAuthentication(configuration);

services.AddAuthorization(options =>
{
    options.AddPolicy("RequireWorkspaces", builder =>
    {
        builder.RequireProtectedResource("workspaces", "workspaces:read")
            .RequireRealmRoles("User")
            .RequireResourceRoles("Admin");
    });
})
    .AddKeycloakAuthorization(configuration);

var app = builder.Build();

app.UseAuthentication()
    .UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateTime.Now.AddDays(index),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.RequireAuthorization();
//.RequireAuthorization("RequireWorkspaces");

app.Run();

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}