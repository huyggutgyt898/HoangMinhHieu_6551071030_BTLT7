using Microsoft.EntityFrameworkCore;
using SunriseHomestayApp.Models;

namespace SunriseHomestayApp;

internal static class DatabaseConfiguration
{
    private const string DefaultConnectionString =
        @"Server=.\SQL2025;Database=SunriseHomestayDb;Trusted_Connection=True;TrustServerCertificate=True;";

    public static SunriseHomestayDbContext CreateContext()
    {
        var connectionString = Environment.GetEnvironmentVariable("SUNRISEHOMESTAY_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = DefaultConnectionString;
        }

        var options = new DbContextOptionsBuilder<SunriseHomestayDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new SunriseHomestayDbContext(options);
    }
}
