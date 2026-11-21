using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Vetcare.Infrastructure.Persistence;

public sealed class VetCareDbContextFactory : IDesignTimeDbContextFactory<VetCareDbContext>
{
    public VetCareDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<VetCareDbContext>()
            .UseSqlite("Data Source=vetcare-design-time.db")
            .Options;
        return new VetCareDbContext(options);
    }
}
