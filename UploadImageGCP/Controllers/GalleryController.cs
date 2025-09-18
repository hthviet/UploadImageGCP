using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace UploadImageGCP.Controllers
{
    public class GalleryController : Controller
    {
        private readonly IConfiguration _config;
        public GalleryController(IConfiguration config)
        {
            _config = config;
        }

        public async Task<IActionResult> Index()
        {
            var bucketName = _config["GcpStorage:BucketName"];
            var credentialsPath = _config["GcpStorage:CredentialsPath"];
            if (string.IsNullOrEmpty(bucketName))
            {
                TempData["UploadMessage"] = "GCP bucket name is not configured.";
                return RedirectToAction("Index", "Home");
            }
            Google.Apis.Auth.OAuth2.GoogleCredential? credential = null;
            if (!string.IsNullOrEmpty(credentialsPath) && System.IO.File.Exists(credentialsPath))
            {
                credential = await Google.Apis.Auth.OAuth2.GoogleCredential.FromFileAsync(credentialsPath, System.Threading.CancellationToken.None);
            }
            var storage = credential != null
                ? await StorageClient.CreateAsync(credential)
                : await StorageClient.CreateAsync();
            var images = new List<string>();
            await foreach (var obj in storage.ListObjectsAsync(bucketName, null))
            {
                if (!string.IsNullOrEmpty(obj.Name))
                {
                    images.Add($"https://storage.googleapis.com/{bucketName}/{obj.Name}");
                }
            }
            return View(images);
        }
    }
}
