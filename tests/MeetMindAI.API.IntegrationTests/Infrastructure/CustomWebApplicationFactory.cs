using Xunit;

using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Persistence.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace MeetMindAI.API.IntegrationTests.Infrastructure;

public sealed class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "MeetMindAI.Test",
                ["Jwt:Audience"] = "MeetMindAI.Test",
                ["Jwt:SecretKey"] = "ThisIsAVeryLongSecretKeyForIntegrationTests123456789!"
            });
        });


        builder.ConfigureServices(services =>
        {
            // Remove the production EF Core registration.
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<ApplicationDbContext>();
            services.RemoveAll<IApplicationDbContext>();

            // Keep one SQLite connection open for the lifetime
            // of this factory so the in-memory database survives
            // between DbContext instances.
            _connection =
                new SqliteConnection("DataSource=:memory:");

            _connection.Open();

            services.AddDbContext<ApplicationDbContext>(
                options =>
                    options.UseSqlite(_connection));

            // Replace the abstraction as well.
            services.AddScoped<IApplicationDbContext>(
                provider =>
                    provider.GetRequiredService<ApplicationDbContext>());

            var serviceProvider =
                services.BuildServiceProvider();

            using var scope =
                serviceProvider.CreateScope();

            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<ApplicationDbContext>();

            dbContext.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection?.Dispose();
        }
    }


}
