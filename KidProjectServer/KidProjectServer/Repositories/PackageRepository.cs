using KidProjectServer.Config;
using KidProjectServer.Controllers;
using KidProjectServer.Entities;
using KidProjectServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Server;

namespace KidProjectServer.Repositories
{
    public interface IPackageRepository
    {
        Task<bool> CheckIsBuyPackage(int hostId);
        Task<int> CountPackagePaging();
        Task<Package[]> GetPackagePaging(int offset, int size);
        Task<Package> GetPackageByID(int id);
        Task<PackageOrderDto[]> GetPackageOrderPaging(int offset, int size);
        Task<int> CountPackageOrderPaging();
        Task<PackageOrder> CreatePackageOrder(OrderPackageForm order);
        Task<PackageOrder> GetPackageOrderByID(int id);
        Task<PackageOrder?> UpdateStatusPackageOrder(int id, string status);
        Task<PackageOrder[]> GetOrdersByUserID(int userId, int offset, int size);
        Task<int> CountOrdersByUserID(int userId);
        Task<Package> CreatePackage(string fileName, PackageFormData formData);
        Task<Package> UpdatePackage(string fileName, Package oldPackage,PackageFormData formData);
        Task<Package?> DeletePackageByID(int id);
        
    }

    public class PackageRepository : IPackageRepository
    {
        private readonly DBConnection _context;

        public PackageRepository(IConfiguration configuration, DBConnection context)
        {
            _context = context;
        }

        public async Task<bool> CheckIsBuyPackage(int hostId)
        {
            PackageOrder[] packageOrders = await _context.PackageOrders.Where(p => p.UserID == hostId && p.Status == Constants.BOOKING_STATUS_PAID && p.CreateDate > DateTime.UtcNow.AddDays(-(double)p.ActiveDays)).ToArrayAsync();
            if (packageOrders != null && packageOrders.Length > 0)
            {
                return true;
            }
            return false;
        }

        public async Task<int> CountOrdersByUserID(int userId)
        {
            return await _context.PackageOrders.Where(p => p.UserID == userId).CountAsync();
        }

        public async Task<int> CountPackageOrderPaging()
        {
            var query = from packageOrders in _context.PackageOrders
                        join users in _context.Users on packageOrders.UserID equals users.UserID
                        select new PackageOrderDto
                        {
                            PackageOrderID = packageOrders.PackageOrderID,
                            FullName = users.FullName,
                            PaymentAmount = packageOrders.PaymentAmount,
                            PackageName = packageOrders.PackageName,
                            PackagePrice = packageOrders.PackagePrice,
                            VoucherPrice = packageOrders.VoucherPrice,
                            CreateDate = packageOrders.CreateDate,
                        };
            return await _context.Packages.Where(p => p.Status == Constants.STATUS_ACTIVE).CountAsync();
        }

        public async Task<int> CountPackagePaging()
        {
            return await _context.Packages.Where(p => p.Status == Constants.STATUS_ACTIVE).CountAsync();
        }

        public async Task<Package> CreatePackage(string fileName,PackageFormData formData)
        {
            Package package = new Package
            {
                AdminUserID = formData.AdminUserID,
                PackageName = formData.PackageName,
                Description = formData.Description,
                ActiveDays = formData.ActiveDays,
                Image = fileName, // Save the image path to the database
                Price = formData.Price,
                CreateDate = DateTime.UtcNow,
                LastUpdateDate = DateTime.UtcNow,
                Status = Constants.STATUS_ACTIVE
            };

            await _context.Packages.AddAsync(package);
            await _context.SaveChangesAsync();
            return package;
        }

        public async Task<PackageOrder> CreatePackageOrder(OrderPackageForm order)
        {
            Package package = await _context.Packages.Where(p => p.PackageID == order.PackageID && p.Status == Constants.STATUS_ACTIVE).FirstOrDefaultAsync();
            User user = await _context.Users.Where(p => p.UserID == order.UserID && p.Status == Constants.STATUS_ACTIVE).FirstOrDefaultAsync();
            PackageOrder packageOrders = new PackageOrder
            {
                PackageID = package.PackageID,
                UserID = user.UserID,
                PackageName = package.PackageName,
                PackageDescription = package.Description,
                PackagePrice = package.Price,
                ActiveDays = package.ActiveDays,
                PaymentAmount = package.Price,
                CreateDate = DateTime.UtcNow,
                LastUpdateDate = DateTime.UtcNow,
                Status = Constants.BOOKING_STATUS_CREATE,
            };

            await _context.PackageOrders.AddAsync(packageOrders);
            await _context.SaveChangesAsync();
            return packageOrders;
        }

        public async Task<Package?> DeletePackageByID(int id)
        {
            var packageOld = await GetPackageByID(id);
            if (packageOld == null)
            {
                return null;
            }

            packageOld.Status = Constants.STATUS_INACTIVE;
            packageOld.LastUpdateDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return packageOld;
        }

        public async Task<PackageOrder[]> GetOrdersByUserID(int userId, int offset, int size)
        {
            return await _context.PackageOrders.Where(p => p.UserID == userId).Skip(offset).Take(size).OrderByDescending(p => p.CreateDate).ToArrayAsync();
        }

        public async Task<Package> GetPackageByID(int id)
        {
            return await _context.Packages.FindAsync(id);
        }

        public async Task<PackageOrder> GetPackageOrderByID(int id)
        {
            return await _context.PackageOrders.Where(p => p.PackageOrderID == id).FirstOrDefaultAsync();
        }

        public async Task<PackageOrderDto[]> GetPackageOrderPaging(int offset, int size)
        {
            var query = from packageOrders in _context.PackageOrders
                        join users in _context.Users on packageOrders.UserID equals users.UserID
                        select new PackageOrderDto
                        {
                            PackageOrderID = packageOrders.PackageOrderID,
                            FullName = users.FullName,
                            PaymentAmount = packageOrders.PaymentAmount,
                            PackageName = packageOrders.PackageName,
                            PackagePrice = packageOrders.PackagePrice,
                            VoucherPrice = packageOrders.VoucherPrice,
                            CreateDate = packageOrders.CreateDate,
                        };
            return await query.Skip(offset).Take(size).OrderByDescending(p => p.CreateDate).ToArrayAsync();
        }

        public async Task<Package[]> GetPackagePaging(int offset, int size)
        {
            return await _context.Packages.Where(p => p.Status == Constants.STATUS_ACTIVE).Skip(offset).Take(size).OrderByDescending(p => p.CreateDate).ToArrayAsync();
        }

        public async Task<Package> UpdatePackage(string fileName, Package packageOld, PackageFormData formData)
        {
            packageOld.AdminUserID = formData.AdminUserID;
            packageOld.PackageName = formData.PackageName;
            packageOld.Description = formData.Description;
            packageOld.ActiveDays = formData.ActiveDays;
            packageOld.Image = fileName;
            packageOld.Price = formData.Price;
            packageOld.LastUpdateDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return packageOld;
        }

        public async Task<PackageOrder?> UpdateStatusPackageOrder(int id, string status)
        {
            PackageOrder packageOrder = await GetPackageOrderByID(id);
            if (packageOrder == null)
            {
                return null;
            }
            packageOrder.Status = status;
            await _context.SaveChangesAsync();
            return packageOrder;
        }
    }
}
