using DAL.DTO;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BL.Interface
{
    public interface IPictureService
    {
        public Task<bool> CreatePicture(string inputPath, string message, byte[] key, byte[] iv, DateTime dateCreate,int CreateUserId);
        public Task<bool> CreatePicture(string inputPath,  string message, DateTime dateCreate,int CreateUserId);
        public Task<bool>  DeletePicture(int deletePictureId);
        public Task<PictureDto> GetPicture(int pictureId);
        public Task<List<PictureDto>> GetAllPicture(int userId);
        public Task<List<PictureDto>> GetAllPicture(string key, string iv);
        public byte[] EncryptStringToBytes_Aes(string plainText, byte[] key, byte[] iv);
        public string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] key, byte[] iv);
        public byte[] EncryptMessageInImage(string inputPath, string message, byte[] key, byte[] iv);
        public Task<string> DecryptMessageFromImage(int id, byte[] key, byte[] iv);

    }
}
