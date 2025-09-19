using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace UploadImageGCP.Services
{
    public class GcpUploadService
    {
        private readonly StorageClient _storageClient;
        private readonly string _bucketName;

        public GcpUploadService(StorageClient storageClient, string bucketName)
        {
            _storageClient = storageClient;
            _bucketName = bucketName;
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");

            var objectName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            using (var stream = file.OpenReadStream())
            {
                await _storageClient.UploadObjectAsync(_bucketName, objectName, null, stream);
            }
            return objectName;
        }
    }
}
