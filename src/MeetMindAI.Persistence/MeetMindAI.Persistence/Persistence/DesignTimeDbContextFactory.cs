using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MeetMindAI.Persistence.Persistence;

/// <summary>
/// Design-time factory for EF Core migrations.
/// </summary>
public sealed class DesignTimeDbContextFactory
    : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        const string connectionString =
            "Host=ep-mute-sound-at6e2rfn-pooler.c-9.us-east-1.aws.neon.tech;" +
            "Database=neondb;" +
            "Username=neondb_owner;" +
            "Password=npg_HmAUKqSu5zV2;" +
            "SSL Mode=Require;" +
            "Trust Server Certificate=true;";

        var optionsBuilder =
            new DbContextOptionsBuilder<ApplicationDbContext>();

        optionsBuilder.UseNpgsql(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
