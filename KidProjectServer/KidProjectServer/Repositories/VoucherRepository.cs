using KidProjectServer.Config;
using KidProjectServer.Controllers;
using KidProjectServer.Entities;
using KidProjectServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;

namespace KidProjectServer.Repositories
{
    public interface IVoucherRepository
    {
        Task DisableVoucherStatus(int voucherID);
        Task Create3DefaultVouchers(int userId);
        Task<VoucherDto[]> GetVoucherByUserID(int id);
    }

    public class VoucherRepository : IVoucherRepository
    {
        private readonly DBConnection _context;

        public VoucherRepository(DBConnection context)
        {
            _context = context;
        }

        public async Task Create3DefaultVouchers(int userId)
        {
            List<Voucher> listVoucherAdd = new List<Voucher>();
            DateTime expiryDate = DateTime.UtcNow.AddDays(30); // ADD 30days to expriryDate
            Voucher voucher1 = new Voucher
            {
                VoucherCode = "VOUCHER100K",
                DiscountAmount = 100000,
                DiscountPercent = 0,
                ExpiryDate = expiryDate,
                DiscountMax = 100000,
                Status = Constants.STATUS_ACTIVE,
                UserID = userId
            };
            Voucher voucher2 = new Voucher
            {
                VoucherCode = "VOUCHER10%",
                DiscountAmount = 0,
                DiscountPercent = 10,
                ExpiryDate = expiryDate,
                DiscountMax = 200000,
                Status = Constants.STATUS_ACTIVE,
                UserID = userId
            };
            Voucher voucher3 = new Voucher
            {
                VoucherCode = "VOUCHER20%",
                DiscountAmount = 0,
                DiscountPercent = 20,
                ExpiryDate = expiryDate,
                DiscountMax = 200000,
                Status = Constants.STATUS_ACTIVE,
                UserID = userId
            };
            listVoucherAdd.Add(voucher1);
            listVoucherAdd.Add(voucher2);
            listVoucherAdd.Add(voucher3);
            _context.Vouchers.AddRange(listVoucherAdd);
            await _context.SaveChangesAsync();
        }

        public async Task DisableVoucherStatus(int voucherID)
        {
            Voucher voucher = await _context.Vouchers.FindAsync(voucherID);
            if(voucher != null)
            {
                voucher.Status = Constants.STATUS_INACTIVE;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<VoucherDto[]> GetVoucherByUserID(int id)
        {
            return await (from vouchers in _context.Vouchers
                    where vouchers.UserID == id && vouchers.Status == Constants.STATUS_ACTIVE
                    select new VoucherDto
                    {
                        VoucherID = vouchers.VoucherID,
                        VoucherCode = vouchers.VoucherCode,
                        //PackageName = packages.PackageName,
                        DiscountAmount = vouchers.DiscountAmount,
                        DiscountPercent = vouchers.DiscountPercent,
                        ExpiryDate = vouchers.ExpiryDate,
                        DiscountMax = vouchers.DiscountMax
                    }).ToArrayAsync();
        }
    }
}
