using FoodDeliveryApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FoodDeliveryApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public ApplicationDbContext() { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=DESKTOP-DMO4MSP\\Ndabe;Database=SnackDashDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True");
            }
        }

        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<OrderItem>()
               .Property(oi => oi.UnitPrice)
               .HasColumnType("decimal(18, 2)");

            modelBuilder.Entity<MenuItem>()
               .Property(mi => mi.Price)
               .HasColumnType("decimal(18, 2)");
    
            modelBuilder.Entity<Order>()
               .Property(o => o.TotalAmount)
               .HasColumnType("decimal(18, 2)");
              
            // This code uses '.Restaurant' and is correct.
            modelBuilder.Entity<MenuItem>()
                .HasOne(mi => mi.Restaurant) 
                .WithMany(r => r.MenuItems)
                .HasForeignKey(mi => mi.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

            // Other relationships...
            modelBuilder.Entity<OrderItem>().HasOne(oi => oi.Order).WithMany(o => o.OrderItems).HasForeignKey(oi => oi.OrderId);
            modelBuilder.Entity<OrderItem>().HasOne(oi => oi.MenuItem).WithMany(mi => mi.OrderItems).HasForeignKey(oi => oi.MenuItemId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Order>().HasOne(o => o.Restaurant).WithMany(r => r.Orders).HasForeignKey(o => o.RestaurantId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Review>().HasOne(r => r.MenuItem).WithMany(mi => mi.Reviews).HasForeignKey(r => r.MenuItemId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Review>().HasOne(r => r.User).WithMany(u => u.Reviews).HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}