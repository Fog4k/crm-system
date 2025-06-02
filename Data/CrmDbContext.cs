using Microsoft.EntityFrameworkCore;
using CrmSystem.Api.Models;

namespace CrmSystem.Api.Data;

public class CrmDbContext : DbContext
{
    public CrmDbContext(DbContextOptions<CrmDbContext> options) : base(options) { }

    public DbSet<Client> Clients => Set<Client>();
}