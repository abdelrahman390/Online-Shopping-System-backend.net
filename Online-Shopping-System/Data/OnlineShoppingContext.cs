using Microsoft.EntityFrameworkCore;

using Online_Shopping_System.Models.Carts;
using Online_Shopping_System.Models.Orders;
using Online_Shopping_System.Models.Payment;
using Online_Shopping_System.Models.Products;
using Online_Shopping_System.Models.Shipping;
using Online_Shopping_System.Models.Users;

namespace Online_Shopping_System.Data
{
    public class OnlineShoppingContext : DbContext
    {
        public OnlineShoppingContext(DbContextOptions<OnlineShoppingContext> options)
            : base(options)
        {
        }

        // =========================
        // Carts
        // =========================

        public DbSet<Cart> Carts { get; set; }

        public DbSet<CartItem> CartItems { get; set; }


        // =========================
        // Orders
        // =========================

        public DbSet<Order> Orders { get; set; }


        // =========================
        // Payments
        // =========================

        public DbSet<Payment> Payments { get; set; }

        public DbSet<CashPayment> CashPayments { get; set; }

        public DbSet<CreditCardPayment> CreditCardPayments { get; set; }

        public DbSet<WalletPayment> WalletPayments { get; set; }


        // =========================
        // Payment Types
        // =========================

        //public DbSet<PaymentType> PaymentTypes { get; set; }


        // =========================
        // Products
        // =========================

        public DbSet<Product> Products { get; set; }

        public DbSet<Books> Books { get; set; }

        public DbSet<Clothes> Clothes { get; set; }

        public DbSet<Electronics> Electronics { get; set; }


        // =========================
        // Shipping
        // =========================

        public DbSet<ShippingRecords> ShippingRecords { get; set; }

        public DbSet<ShippingType> ShippingTypes { get; set; }


        // =========================
        // Users
        // =========================

        public DbSet<User> Users { get; set; }

        public DbSet<UserType> UserTypes { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // ==========================================
            // CART
            // ==========================================

            modelBuilder.Entity<Cart>()
                .HasKey(c => c.CartId);

            // User 1 ---- * Cart
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithMany(u => u.Carts)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Find a user's cart quickly
            modelBuilder.Entity<Cart>()
                .HasIndex(c => c.UserId);

            // Cart 1 ---- * CartItems
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            // Find all items belonging to a cart quickly
            modelBuilder.Entity<CartItem>()
                .HasIndex(ci => ci.CartId);

            // Product 1 ---- * CartItems
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Find cart items for a product quickly
            modelBuilder.Entity<CartItem>()
                .HasIndex(ci => ci.ProductId);

            // A product should normally appear only once in a cart
            modelBuilder.Entity<CartItem>()
                .HasIndex(ci => new { ci.CartId, ci.ProductId })
                .IsUnique();


            // ==========================================
            // ORDER
            // ==========================================

            modelBuilder.Entity<Order>()
                .HasKey(o => o.OrderId);

            // Cart 1 ---- 1 Order
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Cart)
                .WithOne(c => c.Order)
                .HasForeignKey<Order>(o => o.CartId)
                .OnDelete(DeleteBehavior.Restrict);

            // User 1 ---- * Orders
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Get all orders for a user
            modelBuilder.Entity<Order>()
                .HasIndex(o => o.UserId);

            // ==========================================
            // USER
            // ==========================================

            modelBuilder.Entity<User>()
                .HasKey(u => u.UserId);

            // UserType 1 ---- * Users
            modelBuilder.Entity<User>()
                .HasOne(u => u.UserType)
                .WithMany(ut => ut.Users)
                .HasForeignKey(u => u.UserTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.UserTypeId);

            // If Email is used for login:
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();


            // ==========================================
            // PRODUCT INHERITANCE
            // ==========================================

            // TPT:
            // Product
            //   ├── Books
            //   ├── Clothes
            //   └── Electronics

            modelBuilder.Entity<Product>()
                .UseTptMappingStrategy();

            modelBuilder.Entity<Product>()
                .ToTable("Products");

            modelBuilder.Entity<Books>()
                .ToTable("Books");

            modelBuilder.Entity<Clothes>()
                .ToTable("Clothes");

            modelBuilder.Entity<Electronics>()
                .ToTable("Electronics");


            // ==========================================
            // PAYMENT INHERITANCE
            // ==========================================

            // TPT:
            // Payment
            //   ├── CashPayment
            //   ├── CreditCardPayment
            //   └── WalletPayment

            modelBuilder.Entity<Payment>()
                .UseTptMappingStrategy();

            modelBuilder.Entity<Payment>()
                .ToTable("Payments");

            modelBuilder.Entity<CashPayment>()
                .ToTable("CashPayments");

            modelBuilder.Entity<CreditCardPayment>()
                .ToTable("CreditCardPayments");

            modelBuilder.Entity<WalletPayment>()
                .ToTable("WalletPayments");

            // Get payments belonging to an order
            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.OrderId);

            // If TransactionId is used to find a payment
            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.TransactionId)
                .IsUnique();


            // ==========================================
            // PAYMENT TYPE
            // ==========================================

            //modelBuilder.Entity<PaymentType>()
            //    .ToTable("PaymentTypeS");


            // ==========================================
            // SHIPPING
            // ==========================================

            modelBuilder.Entity<ShippingRecords>()
                .ToTable("ShippingRecords");

            modelBuilder.Entity<ShippingType>()
                .ToTable("ShippingTypes");

            modelBuilder.Entity<ShippingRecords>()
                .HasOne(sr => sr.ShippingType)
                .WithMany(st => st.ShippingRecords)
                .HasForeignKey(sr => sr.ShippingTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ShippingRecords>()
                .HasIndex(sr => sr.ShippingTypeId);


            // ==========================================
            // USER TYPE
            // ==========================================

            modelBuilder.Entity<UserType>()
                .ToTable("UserTypes");
        }
    }
}