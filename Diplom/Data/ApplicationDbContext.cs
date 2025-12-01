using Microsoft.EntityFrameworkCore;
using TicketViewer.Models;

namespace TicketViewer.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Requests> Requests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Requests>(entity =>
            {
                // таблиця у MySQL
                entity.ToTable("requests_clean");

                // первинний ключ
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Start_date).HasColumnName("Start_date");
                entity.Property(e => e.Change_date).HasColumnName("Change_date");

                entity.Property(e => e.AgeMinutes).HasColumnName("AgeMinutes");

                entity.Property(e => e.Area_ID).HasColumnName("Area_ID");
                entity.Property(e => e.Priority).HasColumnName("Priority");
                entity.Property(e => e.Status).HasColumnName("Status");

                entity.Property(e => e.Responsible).HasColumnName("Responsible");
                entity.Property(e => e.Category).HasColumnName("Category");
                entity.Property(e => e.Header).HasColumnName("Header");
                entity.Property(e => e.Initiator).HasColumnName("Initiator");
            });
        }
    }
}
