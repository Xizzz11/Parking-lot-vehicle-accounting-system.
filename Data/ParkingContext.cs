using Microsoft.EntityFrameworkCore;
using ParkingSystem.Models;

namespace ParkingSystem.Data
{
    public class ParkingContext : DbContext
    {
        public DbSet<VehicleOwner> VehicleOwners => Set<VehicleOwner>();
        public DbSet<Vehicle> Vehicles => Set<Vehicle>();
        public DbSet<ParkingSpace> ParkingSpaces => Set<ParkingSpace>();
        public DbSet<ParkingTicket> ParkingTickets => Set<ParkingTicket>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=192.168.9.203\\SQLEXPRESS;Database=ParkingLotDB;User Id=student1;Password=123456;TrustServerCertificate=True;");
                optionsBuilder.UseSqlServer("Server=192.168.9.203\\SQLEXPRESS;Database=ParkingLotDB;User Id=student1;Password=123456;TrustServerCertificate=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VehicleOwner>(e =>
            {
                e.ToTable("VehicleOwner");
                e.HasKey(x => x.OwnerID);
                e.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
                e.Property(x => x.LastName).IsRequired().HasMaxLength(100);
                e.Property(x => x.PhoneNumber).IsRequired().HasMaxLength(20);
                e.Property(x => x.Email).HasMaxLength(200);
            });

            modelBuilder.Entity<Vehicle>(e =>
            {
                e.ToTable("Vehicle");
                e.HasKey(x => x.VehicleID);
                e.Property(x => x.LicensePlate).IsRequired().HasMaxLength(15);
                e.Property(x => x.Make).IsRequired().HasMaxLength(100);
                e.Property(x => x.Model).IsRequired().HasMaxLength(100);
                e.Property(x => x.Color).IsRequired().HasMaxLength(50);

                e.HasOne(x => x.Owner)
                 .WithMany(o => o.Vehicles)
                 .HasForeignKey(x => x.OwnerID)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ParkingSpace>(e =>
            {
                e.ToTable("ParkingSpace");
                e.HasKey(x => x.ParkingSpaceID);
                e.Property(x => x.SpaceNumber).IsRequired().HasMaxLength(10);
                e.Property(x => x.IsOccupied).IsRequired();
            });

            modelBuilder.Entity<ParkingTicket>(e =>
            {
                e.ToTable("ParkingTicket");
                e.HasKey(x => x.TicketID);
                e.Property(x => x.EntryTime).IsRequired();
                e.Property(x => x.Fee).HasColumnType("decimal(18,2)");

                e.HasOne(x => x.Vehicle)
                 .WithMany()
                 .HasForeignKey(x => x.VehicleID)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.ParkingSpace)
                 .WithMany()
                 .HasForeignKey(x => x.ParkingSpaceID)
                 .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
