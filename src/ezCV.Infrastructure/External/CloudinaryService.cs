using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using ezCV.Application.External;
using ezCV.Application.External.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezCV.Infrastructure.External
{ 
    public class CloudinaryService : ICloudinaryService   
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IOptions<CloudinarySetting> config)
        {
            var acc = new Account
                   (
                   config.Value.CloudName,
                   config.Value.ApiKey,
                   config.Value.ApiSecret
                   );
            _cloudinary = new Cloudinary(acc);
        }

        public async Task<ImageUploadResult> UploadImageAsync(IFormFile file)
        {
            var uploadResult = new ImageUploadResult();

            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is null or empty", nameof(file));
            }

            if (file.Length > 0)
            {
                await using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream)
                };
                try
                {
                    uploadResult = await _cloudinary.UploadAsync(uploadParams);
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it as needed
                    throw new InvalidOperationException("Error uploading image to Cloudinary", ex);
                }
            }
            return uploadResult;
        }

        public Task<DeletionResult> DeleteImageAsync(string publicId)
        {
            var deleteResult = new DeletionParams(publicId);
            var result = _cloudinary.DestroyAsync(deleteResult);
            return result;
        }
    }
}
