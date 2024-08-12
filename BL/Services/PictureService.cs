using DAL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SixLabors.ImageSharp.Formats.Png;
using System.Collections;
using  DAL;
using System.Security.Cryptography;
using DAL.Interfaces;
using BL.Interface;
using System.ComponentModel;
using Aoxe.Extensions;
namespace BL.Services
{
    public class PictureService:IPictureService
    {
        private readonly IPictureData _pictureData;
        public PictureService(IPictureData pictureData)
        {
            _pictureData = pictureData; 
        }
        public async Task<bool> CreatePicture(string inputPath, string message, byte[] key, byte[] iv,DateTime dateCreate,int CreateUserId)
        {
            byte[]image= EncryptMessageInImage(inputPath, message, key, iv);
            var pictureDto = new PictureDto(image, dateCreate, CreateUserId, key.ToBase64String(), iv.ToBase64String());
            return  await _pictureData.CreatePicture(pictureDto);
        }
        public async Task<bool> CreatePicture(string inputPath, string message, DateTime dateCreate,int CreateUserId)
        {
            KeyManager.GenerateAndSaveKeys();
            byte[] key = KeyManager.GetKey();
            byte[] iv = KeyManager.GetIV();
            return await CreatePicture(inputPath, message, key, iv, dateCreate, CreateUserId);
        }

        public async Task<bool> DeletePicture(int deletePictureId)
        {
            return await _pictureData.DeletePicture(deletePictureId);
        }
        public async Task<PictureDto> GetPicture(int pictureId)
        {
            return await _pictureData.GetPicture(pictureId); 
        }
        public async Task<List<PictureDto>> GetAllPicture(int userId)
        {
            return await _pictureData.GetAllPicture(userId);
        }
        public async Task<List<PictureDto>> GetAllPicture(string key, string iv)
        {
            return await _pictureData.GetAllPicture(key, iv);  
        }
        public  byte[] EncryptStringToBytes_Aes(string plainText, byte[] key, byte[] iv)
        {
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException(nameof(plainText));
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException(nameof(key));
            if (iv == null || iv.Length <= 0)
                throw new ArgumentNullException(nameof(iv));
            byte[] encrypted;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            return encrypted;
        }

        public string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] key, byte[] iv)
        {
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException(nameof(cipherText));
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException(nameof(key));
            if (iv == null || iv.Length <= 0)
                throw new ArgumentNullException(nameof(iv));
            string plaintext = null;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }
        public byte[] EncryptMessageInImage(string inputPath, string message, byte[] key, byte[] iv)
        {
            using (Image<Rgba32> image = Image.Load<Rgba32>(inputPath))
            {
                byte[] encryptedMessage =  EncryptStringToBytes_Aes(message, key, iv);
                byte[] lengthBytes = BitConverter.GetBytes(encryptedMessage.Length);
                byte[] fullMessageBytes = lengthBytes.Concat(encryptedMessage).ToArray();

                if (fullMessageBytes.Length * 8 > image.Width * image.Height)
                {
                    throw new InvalidOperationException("Message is too long to be hidden in this image.");
                }

                int byteIndex = 0;
                int bitIndex = 0;

                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        if (byteIndex >= fullMessageBytes.Length)
                            break;

                        Rgba32 pixel = image[x, y];

                        pixel.R = (byte)((pixel.R & ~1) | ((fullMessageBytes[byteIndex] >> bitIndex) & 1));
                        bitIndex++;

                        if (bitIndex == 8)
                        {
                            bitIndex = 0;
                            byteIndex++;
                        }

                        image[x, y] = pixel;
                    }

                    if (byteIndex >= fullMessageBytes.Length)
                        break;
                }

                using (var memoryStream = new MemoryStream())
                {
                    // שמירת התמונה לזרם הזיכרון בפורמט PNG (אפשר להחליף לפורמטים אחרים)
                    image.Save(memoryStream, new PngEncoder());

                    // החזרת המערך של byte[] מהזרם
                    return memoryStream.ToArray();
                }
            }
        }


            public async Task<string> DecryptMessageFromImage(int id, byte[] key, byte[] iv)
            {
                var picture = await GetPicture(id);
            if (picture != null)
            {
                using (Image<Rgba32> image = Image.Load<Rgba32>(picture.EncryptionPicture))
                {
                    int byteIndex = 0;
                    int bitIndex = 0;
                    byte[] lengthBytes = new byte[4];

                    for (int y = 0; y < image.Height; y++)
                    {
                        for (int x = 0; x < image.Width; x++)
                        {
                            Rgba32 pixel = image[x, y];
                            lengthBytes[byteIndex] = (byte)((lengthBytes[byteIndex] & ~(1 << bitIndex)) | ((pixel.R & 1) << bitIndex));
                            bitIndex++;

                            if (bitIndex == 8)
                            {
                                bitIndex = 0;
                                byteIndex++;
                            }

                            if (byteIndex == 4)
                                break;
                        }

                        if (byteIndex == 4)
                            break;
                    }

                    int messageLength = BitConverter.ToInt32(lengthBytes, 0);
                    byte[] messageBytes = new byte[messageLength];
                    byteIndex = 0;
                    bitIndex = 0;

                    for (int y = 0; y < image.Height; y++)
                    {
                        for (int x = 0; x < image.Width; x++)
                        {
                            if (byteIndex >= messageBytes.Length)
                                break;

                            Rgba32 pixel = image[x, y];

                            if (y * image.Width + x >= 32)
                            {
                                messageBytes[byteIndex] = (byte)((messageBytes[byteIndex] & ~(1 << bitIndex)) | ((pixel.R & 1) << bitIndex));
                                bitIndex++;

                                if (bitIndex == 8)
                                {
                                    bitIndex = 0;
                                    byteIndex++;
                                }
                            }
                        }

                        if (byteIndex >= messageBytes.Length)
                            break;
                    }

                    return DecryptStringFromBytes_Aes(messageBytes, key, iv);
                }
            }
            return null;
            }
        }
    }

       
