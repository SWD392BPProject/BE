using KidProjectServer.Controllers;
using KidProjectServer.Entities;
using KidProjectServer.Models;
using KidProjectServer.Repositories;
using KidProjectServer.Util;
using Microsoft.Extensions.Hosting;
using System.Drawing;

namespace KidProjectServer.Services
{
    public interface IPackageService
    {
        Task<bool> CheckIsBuyPackage(int hostId);
        Task<Package[]> GetPackagePaging(int offset, int size);
        Task<int> CountPackagePaging();
        Task<Package> GetPackageByID(int id);
        Task<PackageOrderDto[]> GetPackageOrderPaging(int offset, int size);
        Task<int> CountPackageOrderPaging();
        Task<PackageOrder> CreatePackageOrder(OrderPackageForm order);
        Task<PackageOrder> GetPackageOrderByID(int id);
        Task<PackageOrder?> UpdateStatusPackageOrder(int id, string status);
        Task<PackageOrder[]> GetOrdersByUserID(int userId, int offset, int size);
        Task<int> CountOrdersByUserID(int userId);
        Task<Package> CreatePackage(string fileName, PackageFormData formData);
        Task<Package> UpdatePackage(string fileName, Package oldPackage, PackageFormData formData);
        Task<Package?> DeletePackageByID(int id);
    }

    public class PackageService : IPackageService
    {
        private readonly IPackageRepository _packageRepository;

        public PackageService(IPackageRepository packageRepository)
        {
            _packageRepository = packageRepository;
        }

        public async Task<bool> CheckIsBuyPackage(int hostId)
        {
            return await _packageRepository.CheckIsBuyPackage(hostId);
        }

        public async Task<int> CountOrdersByUserID(int userId)
        {
            return await _packageRepository.CountOrdersByUserID(userId);
        }

        public async Task<int> CountPackageOrderPaging()
        {
            return await _packageRepository.CountPackageOrderPaging();
        }

        public async Task<int> CountPackagePaging()
        {
            return await _packageRepository.CountPackagePaging();
        }

        public async Task<Package> CreatePackage(string fileName, PackageFormData formData)
        {
            return await _packageRepository.CreatePackage(fileName, formData);    
        }

        public async Task<PackageOrder> CreatePackageOrder(OrderPackageForm order)
        {
            return await _packageRepository.CreatePackageOrder(order);
        }

        public async Task<Package?> DeletePackageByID(int id)
        {
            return await _packageRepository.DeletePackageByID(id);
        }

        public async Task<PackageOrder[]> GetOrdersByUserID(int userId, int offset, int size)
        {
            return await _packageRepository.GetOrdersByUserID(userId, offset, size);
        }

        public async Task<Package> GetPackageByID(int id)
        {
            return await _packageRepository.GetPackageByID(id);
        }

        public async Task<PackageOrder> GetPackageOrderByID(int id)
        {
            return await _packageRepository.GetPackageOrderByID(id);
        }

        public async Task<PackageOrderDto[]> GetPackageOrderPaging(int offset, int size)
        {
            return await GetPackageOrderPaging(offset, size);
        }

        public async Task<Package[]> GetPackagePaging(int offset, int size)
        {
            return await _packageRepository.GetPackagePaging(offset, size);
        }

        public async Task<Package> UpdatePackage(string fileName, Package oldPackage, PackageFormData formData)
        {
            return await _packageRepository.UpdatePackage(fileName, oldPackage, formData);
        }

        public async Task<PackageOrder?> UpdateStatusPackageOrder(int id, string status)
        {
            return await _packageRepository.UpdateStatusPackageOrder(id, status);
        }
    }
}
