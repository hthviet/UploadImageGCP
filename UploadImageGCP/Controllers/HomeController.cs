using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UploadImageGCP.Models;
using Google.Cloud.Storage.V1;
using System.IO;
using System.Threading.Tasks;
using UploadImageGCP.Services;

namespace UploadImageGCP.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _config;
        private readonly GcpUploadService _gcpUploadService;

        public HomeController(ILogger<HomeController> logger, IConfiguration config, GcpUploadService gcpUploadService)
        {
            _logger = logger;
            _config = config;
            _gcpUploadService = gcpUploadService;
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
            if (imageFiles == null || imageFiles.Length == 0 || imageFiles.All(f => f == null || f.Length == 0))
            {
                TempData["UploadMessage"] = "Please select at least one image file.";
                return RedirectToAction("Index");
            }

            var uploadedUrls = new List<string>();
            foreach (var file in imageFiles)
            {
                if (file != null && file.Length > 0)
                {
                    try
                    {
                        var objectName = await _gcpUploadService.UploadFileAsync(file);
                        var bucketName = _config["GcpStorage:BucketName"];
                        var url = $"https://storage.googleapis.com/{bucketName}/{objectName}";
                        uploadedUrls.Add(url);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "GCP upload failed");
                    }
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
