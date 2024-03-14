using KidProjectServer.Controllers;
using KidProjectServer.Entities;
using KidProjectServer.Models;
using KidProjectServer.Repositories;
using KidProjectServer.Util;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;

namespace KidProjectServer.Services
{
    public interface IVoucherService
    {
        Task DisableVoucherStatus(int voucherID);
        Task Create3DefaultVouchers(int userId);
        Task<VoucherDto[]> GetVoucherByUserID(int id);
    }

    public class VoucherService : IVoucherService
    {
        private readonly IVoucherRepository _voucherRepository;

        public VoucherService(IVoucherRepository voucherRepository)
        {
            _voucherRepository = voucherRepository;
        }

        public async Task Create3DefaultVouchers(int userId)
        {
            await _voucherRepository.Create3DefaultVouchers(userId);
        }

        public async Task DisableVoucherStatus(int voucherID)
        {
            await _voucherRepository.DisableVoucherStatus(voucherID);
        }

        public async Task<VoucherDto[]> GetVoucherByUserID(int id)
        {
            return await _voucherRepository.GetVoucherByUserID(id);
        }
    }
}
