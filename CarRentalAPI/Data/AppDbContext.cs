using CarRentalAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CarRentalAPI.Data;

// Replacement for existing AppDbContext.cs. Retains the original 5 DbSets and mappings.
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Car> Cars => Set<Car>();
    public DbSet<CarCategory> CarCategories => Set<CarCategory>();
    public DbSet<CarBrand> CarBrands => Set<CarBrand>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<CarImage> CarImages => Set<CarImage>();
    public DbSet<CarFeature> CarFeatures => Set<CarFeature>();
    public DbSet<CarFeatureMapping> CarFeatureMappings => Set<CarFeatureMapping>();
    public DbSet<BookingDetail> BookingDetails => Set<BookingDetail>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<RentalExtension> RentalExtensions => Set<RentalExtension>();
    public DbSet<CarHandoverRecord> CarHandoverRecords => Set<CarHandoverRecord>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<TestDriveBooking> TestDriveBookings => Set<TestDriveBooking>();
    public DbSet<SupportRequest> SupportRequests => Set<SupportRequest>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Refund> Refunds => Set<Refund>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<User>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<Car>().HasIndex(x => x.LicensePlate).IsUnique();
        modelBuilder.Entity<Car>().Property(x => x.PricePerDay).HasPrecision(18, 2);
        modelBuilder.Entity<Booking>().Property(x => x.TotalAmount).HasPrecision(18, 2);
        modelBuilder.Entity<Booking>().HasOne(x => x.Customer).WithMany(x => x.Bookings)
            .HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Booking>().HasOne(x => x.Car).WithMany(x => x.Bookings)
            .HasForeignKey(x => x.CarId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CarImage>().HasOne<Car>().WithMany()
            .HasForeignKey(x => x.CarId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<CarFeatureMapping>().HasOne<Car>().WithMany()
            .HasForeignKey(x => x.CarId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<CarFeatureMapping>().HasOne<CarFeature>().WithMany()
            .HasForeignKey(x => x.CarFeatureId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<BookingDetail>().HasOne<Booking>().WithMany()
            .HasForeignKey(x => x.BookingId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<BookingDetail>().HasOne<Car>().WithMany()
            .HasForeignKey(x => x.CarId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Contract>().HasOne<Booking>().WithMany()
            .HasForeignKey(x => x.BookingId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Payment>().HasOne<Contract>().WithMany()
            .HasForeignKey(x => x.ContractId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<RentalExtension>().HasOne<Booking>().WithMany()
            .HasForeignKey(x => x.BookingId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<CarHandoverRecord>().HasOne<Booking>().WithMany()
            .HasForeignKey(x => x.BookingId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<CarHandoverRecord>().HasOne<User>().WithMany()
            .HasForeignKey(x => x.StaffId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Review>().HasOne<User>().WithMany()
            .HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Review>().HasOne<Car>().WithMany()
            .HasForeignKey(x => x.CarId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<TestDriveBooking>().HasOne<User>().WithMany()
            .HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<TestDriveBooking>().HasOne<Car>().WithMany()
            .HasForeignKey(x => x.CarId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<SupportRequest>().HasOne<User>().WithMany()
            .HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<SupportRequest>().HasOne<User>().WithMany()
            .HasForeignKey(x => x.StaffId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Notification>().HasOne<User>().WithMany()
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Report>().HasOne<User>().WithMany()
            .HasForeignKey(x => x.CreatedById).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<RefreshToken>().HasOne<User>().WithMany()
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Refund>().HasOne<Payment>().WithMany()
            .HasForeignKey(x => x.PaymentId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<AuditLog>().HasOne<User>().WithMany()
            .HasForeignKey(x => x.ActorId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Contract>().HasIndex(x => x.BookingId).IsUnique();
        modelBuilder.Entity<CarFeatureMapping>().HasIndex(x => new { x.CarId, x.CarFeatureId }).IsUnique();
        modelBuilder.Entity<Payment>().HasIndex(x => x.TransactionCode).IsUnique();
        modelBuilder.Entity<RefreshToken>().HasIndex(x => x.TokenHash).IsUnique();
        modelBuilder.Entity<Review>().ToTable(t => t.HasCheckConstraint("CK_Review_Rating", "\"Rating\" BETWEEN 1 AND 5"));
        modelBuilder.Entity<Payment>().Property(x => x.Amount).HasPrecision(18, 2);
        modelBuilder.Entity<Refund>().Property(x => x.Amount).HasPrecision(18, 2);
        modelBuilder.Entity<Contract>().Property(x => x.DepositAmount).HasPrecision(18, 2);
        modelBuilder.Entity<RentalExtension>().Property(x => x.ExtraAmount).HasPrecision(18, 2);
        modelBuilder.Entity<BookingDetail>().Property(x => x.UnitPrice).HasPrecision(18, 2);
        modelBuilder.Entity<BookingDetail>().Property(x => x.Amount).HasPrecision(18, 2);
    }
}
