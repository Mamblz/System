using Microsoft.EntityFrameworkCore;
using CrmSystem.Models;

namespace CrmSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Client> Clients { get; set; }
        public virtual DbSet<Interaction> Interactions { get; set; }
        public virtual DbSet<Ticket> Tickets { get; set; }
        public virtual DbSet<Deal> Deals { get; set; }

        public virtual DbSet<TasksItems> TaskItems { get; set; }

        public ApplicationDbContext() { }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=crm_database.db");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Client>()
                .Property(c => c.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Interaction>()
                .HasOne(i => i.Client)
                .WithMany(c => c.Interactions)
                .HasForeignKey(i => i.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Ticket>()
                .Property(t => t.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Ticket>()
                .HasIndex(t => t.ClientId);

            modelBuilder.Entity<Deal>()
                .HasOne(d => d.Client)
                .WithMany()
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "admin", Email = "admin@example.com" }
            );
            modelBuilder.Entity<TasksItems>()
                .Property(t => t.Id)
                .ValueGeneratedOnAdd();
        }
    }
}
