using KidProjectServer.Controllers;
using KidProjectServer.Entities;
using KidProjectServer.Repositories;
using KidProjectServer.Util;
using System.Drawing;

namespace KidProjectServer.Services
{
    public interface IImageService
    {
        Task<string?> UpdateImageFile(string? oldFileName, IFormFile? Image);

    }

    public class ImageService : IImageService
    {
        private readonly IImageRepository _imageRepository;

        public ImageService(IImageRepository imageRepository)
        {
            _imageRepository = imageRepository;
        }

        public Task<string?> UpdateImageFile(string? oldFileName, IFormFile? Image)
        {
            return _imageRepository.UpdateImageFile(oldFileName, Image);
        }
    }
}
