using System.Threading.Tasks;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace DatingApp.API.Interfaces
{
    public interface IPhotoService
    {
        public Task<ImageUploadResult> UploadImageAsync(IFormFile file);
        public Task<DeletionResult> DeleteImageAsync(string publicId);
    }
}