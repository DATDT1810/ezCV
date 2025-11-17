using ezCV.Application.External;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezCV.Application.Features.CvProcessing
{
    public class ImageCvProcessingService
    {
        private readonly ICloudinaryService _cloudinaryService;

        public ImageCvProcessingService(ICloudinaryService cloudinaryService)
        {
            _cloudinaryService = cloudinaryService;
        }

        public async Task<string> AddImageCv(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            // VALIDATE FILE TYPE
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
            {
                throw new ArgumentException("Chỉ chấp nhận file ảnh (jpg, jpeg, png).");
            }

            // VALIDATE FILE SIZE (max 5MB) - SỬA LỖI CHÍNH TẢ: 1924 → 1024
            if (file.Length > 5 * 1024 * 1024) // SỬA: 1924 → 1024
            {
                throw new ArgumentException("Kích thước file không được vượt quá 5MB.");
            }

            try
            {
                var imageResult = await _cloudinaryService.UploadImageAsync(file);

                // KIỂM TRA KẾT QUẢ UPLOAD
                if (imageResult == null)
                {
                    throw new InvalidOperationException("Cloudinary upload returned null result");
                }

                if (imageResult.Error != null)
                {
                    throw new InvalidOperationException($"Cloudinary upload error: {imageResult.Error.Message}");
                }

                // KIỂM TRA SECURE URL
                if (imageResult.SecureUrl == null)
                {
                    // Thử sử dụng Url thường nếu SecureUrl null
                    if (imageResult.Url != null)
                    {
                        return imageResult.Url.ToString();
                    }
                    else
                    {
                        throw new InvalidOperationException("Cloudinary upload succeeded but no URL returned. PublicId: " + imageResult.PublicId);
                    }
                }

                return imageResult.SecureUrl.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi upload ảnh: {ex.Message}");
            }
        }

        public async Task<bool> DeleteImageCv(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return false;

            try
            {
                var uri = new Uri(imageUrl);
                var fileName = uri.Segments.Last();
                var publicId = System.IO.Path.GetFileNameWithoutExtension(fileName);

                var result = await _cloudinaryService.DeleteImageAsync(publicId);
                return result.Result == "ok";
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
