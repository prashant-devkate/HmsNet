using HmsNet.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace HmsNet.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Bill> Bills { get; set; }
        public DbSet<Transaction> Transactions { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

           
            // Seed: Users
            modelBuilder.Entity<User>().HasData( new User 
            {
                UserId = 1, 
                Username = "admin", 
                Firstname = "Admin", 
                Lastname = "Admin", 
                Email = "admin.helpdesk@gmail.com", 
                PasswordHash = "hashed-password", 
                Role = "Admin", 
                Status = "Active",
                CreatedAt = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc)
            }
            );
        }
    }
}