using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace GymSystem.BLL.Interfaces
{
    public interface IImageService
    {
        Task DeleteImageAsync(string imageFileName);
        Task<Tuple<int, string>> UploadImageAsync(IFormFile imageFile);
        Task<byte[]> GetImageAsync(string imageName);
    }
}