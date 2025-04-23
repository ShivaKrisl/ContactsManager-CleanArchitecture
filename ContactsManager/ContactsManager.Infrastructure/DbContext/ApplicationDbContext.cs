using ContactsManager.Core.Domain.IdentityEntities;
using Entities_Core;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;


namespace Db
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public virtual DbSet<Person> Persons { get; set; }

        public virtual DbSet<Country> Countries { get; set; }

        /// <summary>
        /// Constructor for the ApplicationDbContext class.
        /// </summary>
        /// <param name="options"></param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // for configuring the identity tables -- default implementation
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Person>().ToTable("Persons");
            modelBuilder.Entity<Country>().ToTable("Countries");

            // Configure the primary key for the Person entity
            modelBuilder.Entity<Person>()
                .HasKey(p => p.PersonId);

            // Configure the primary key for the Country entity

            modelBuilder.Entity<Country>()
                .HasKey(c => c.CountryId);

            // Configure the relationship between Person and Country
            modelBuilder.Entity<Person>()
                .HasOne(p => p.Country)
                .WithMany(c => c.Persons)
                .HasForeignKey(p => p.CountryId)
                .OnDelete(DeleteBehavior.Cascade);

            // seed data 
            string countriesJson = File.ReadAllText("CountriesData.json");
            List<Country>? countries = JsonSerializer.Deserialize<List<Country>>(countriesJson);
            foreach (Country country in countries)
            {
                modelBuilder.Entity<Country>().HasData(country);
            }

        }
    }
}
