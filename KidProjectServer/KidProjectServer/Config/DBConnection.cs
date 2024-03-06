using KidProjectServer.Entities;
using Microsoft.EntityFrameworkCore;

namespace KidProjectServer.Config
{
    public class DBConnection : DbContext
    {
        public DBConnection(DbContextOptions<DBConnection> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Party> Parties { get; set; }
        public DbSet<Slot> Slots { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<MenuParty> MenuParty { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<PackageOrder> PackageOrders { get; set; }
    }
}
