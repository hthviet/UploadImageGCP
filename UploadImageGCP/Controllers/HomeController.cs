using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UploadImageGCP.Models;
using Google.Cloud.Storage.V1;
using System.IO;
using System.Threading.Tasks;

namespace UploadImageGCP.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly IConfiguration _config;

        public HomeController(ILogger<HomeController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile[] imageFiles)
        {
            // Diagnostic: log all config keys/values
            foreach (var kvp in _config.AsEnumerable())
            {
                _logger.LogInformation($"CONFIG: {kvp.Key} = {kvp.Value}");
            }
            if (imageFiles == null || imageFiles.Length == 0 || imageFiles.All(f => f == null || f.Length == 0))
            {
                TempData["UploadMessage"] = "Please select at least one image file.";
                return RedirectToAction("Index");
            }

            var bucketName = _config["GcpStorage:BucketName"];
            var credentialsPath = _config["GcpStorage:CredentialsPath"];
            _logger.LogInformation($"BUCKET: {bucketName}, CREDS: {credentialsPath}");
            if (string.IsNullOrEmpty(bucketName))
            {
                TempData["UploadMessage"] = "GCP bucket name is not configured.";
                return RedirectToAction("Index");
            }

            var uploadedUrls = new List<string>();
            foreach (var file in imageFiles)
            {
                if (file != null && file.Length > 0)
                {
                    var url = await UploadToGcpBucket(file, bucketName, credentialsPath ?? string.Empty);
                    if (!string.IsNullOrEmpty(url))
                        uploadedUrls.Add(url);
                }
            }

            if (uploadedUrls.Count > 0)
            {
                TempData["UploadMessage"] = $"Uploaded {uploadedUrls.Count} image(s) successfully to GCP bucket.";
                TempData["ImageUrls"] = System.Text.Json.JsonSerializer.Serialize(uploadedUrls);
            }
            else
            {
                TempData["UploadMessage"] = "Failed to upload image(s) to GCP bucket.";
            }
            return RedirectToAction("Index");
        }

    private async Task<string?> UploadToGcpBucket(IFormFile file, string bucketName, string credentialsPath)
    {
        try
        {
            Google.Apis.Auth.OAuth2.GoogleCredential? credential = null;
            if (!string.IsNullOrEmpty(credentialsPath) && System.IO.File.Exists(credentialsPath))
            {
                credential = await Google.Apis.Auth.OAuth2.GoogleCredential.FromFileAsync(credentialsPath, System.Threading.CancellationToken.None);
            }
            var storage = credential != null
                ? await StorageClient.CreateAsync(credential)
                : await StorageClient.CreateAsync();
            string objectName = file.FileName;
            using (var stream = file.OpenReadStream())
            {
                await storage.UploadObjectAsync(bucketName, objectName, file.ContentType, stream);
            }
            // Assuming the bucket is public, construct the public URL
            string imageUrl = $"https://storage.googleapis.com/{bucketName}/{objectName}";
            return imageUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GCP upload failed");
            return null;
        }
    }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
