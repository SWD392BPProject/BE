using KidProjectServer.Config;
using KidProjectServer.Controllers;
using KidProjectServer.Entities;
using KidProjectServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Server;

namespace KidProjectServer.Repositories
{
    public interface IImageRepository
    {
        Task<string?> UpdateImageFile(string? oldFileName, IFormFile? Image);
        Task<string?> CreateImageFile(IFormFile? Image);
    }

    public class ImageRepository : IImageRepository
    {
        private readonly DBConnection _context;
        private readonly IConfiguration _configuration;

        public ImageRepository(IConfiguration configuration, DBConnection context)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<string?> CreateImageFile(IFormFile? Image)
        {
            if(Image == null)
            {
                return null;
            }
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(Image.FileName);
            var imagePath = Path.Combine(_configuration["ImagePath"], fileName);
            using (var stream = new FileStream(imagePath, FileMode.Create))
            {
                await Image.CopyToAsync(stream);
            }
            return fileName;
        }

        public async Task<string?> UpdateImageFile(string? oldFileName, IFormFile? Image)
        {
            string? fileName = oldFileName;
            if (Image != null && Image.Length > 0)
            {
                // Save the uploaded image to a specific location (or any other processing)
                fileName = Guid.NewGuid().ToString() + Path.GetExtension(Image.FileName);
                var imagePath = Path.Combine(_configuration["ImagePath"], fileName);
                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await Image.CopyToAsync(stream);
                }
                // Delete old image if it exists
                if (oldFileName != null)
                {
                    var oldImagePath = Path.Combine(_configuration["ImagePath"], oldFileName);
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
            }
            return fileName;
        }
    }
}
