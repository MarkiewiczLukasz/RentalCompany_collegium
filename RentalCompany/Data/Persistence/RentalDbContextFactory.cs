using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Data.Persistence;

internal sealed class RentalDbContextFactory : IDesignTimeDbContextFactory<RentalDbContext>
{
    public RentalDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Port=54329;Database=rental_company;Username=rental;Password=RentalDb123!";
        var options = new DbContextOptionsBuilder<RentalDbContext>().UseNpgsql(connectionString).Options;
        return new RentalDbContext(options);
    }
}
