using DemoProject.Entities;
using Microsoft.EntityFrameworkCore;

namespace DemoProject.DataAccess;

public class DemoContext : DbContext
{
    public DbSet<Person> Persons { get; set; }

    public DemoContext(DbContextOptions<DemoContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //modelBuilder.Entity<Person>().Property(x => x.Age).HasColumnType();
        modelBuilder.Entity<Person>().Property(x => x.Name).HasMaxLength(100);
        modelBuilder.Entity<Person>().Property(x => x.PhotoUrl).HasMaxLength(1000);

        //modelBuilder.ApplyConfiguration(new PersonConfiguration());
        //modelBuilder.ApplyConfigurationsFromAssembly();
    }
}
