
using UploadImageGCP.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register GcpUploadService and StorageClient
builder.Services.AddSingleton(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var bucketName = config["GcpStorage:BucketName"];
    if (string.IsNullOrEmpty(bucketName))
    {
        throw new InvalidOperationException("GcpStorage:BucketName configuration value is missing or empty.");
    }
    var credentialsPath = config["GcpStorage:CredentialsPath"];
    Google.Cloud.Storage.V1.StorageClient storageClient;
    if (!string.IsNullOrEmpty(credentialsPath) && System.IO.File.Exists(credentialsPath))
    {
        var credential = Google.Apis.Auth.OAuth2.GoogleCredential.FromFile(credentialsPath);
        storageClient = Google.Cloud.Storage.V1.StorageClient.Create(credential);
    }
    else
    {
        storageClient = Google.Cloud.Storage.V1.StorageClient.Create();
    }
    return new UploadImageGCP.Services.GcpUploadService(storageClient, bucketName);
});

// Configure GCP Pub/Sub settings (replace with your actual values or use configuration)
string gcpProjectId = "your-gcp-project-id";
string pubsubTopicId = "your-topic-id";
string pubsubSubscriptionId = "your-subscription-id";

// Register Pub/Sub publisher as singleton
builder.Services.AddSingleton(new PubSubPublisherService(gcpProjectId, pubsubTopicId));

// Register Pub/Sub subscriber as hosted background service
builder.Services.AddHostedService(provider => new PubSubSubscriberService(gcpProjectId, pubsubSubscriptionId));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
