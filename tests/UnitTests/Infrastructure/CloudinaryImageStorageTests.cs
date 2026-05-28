using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Domain.Notes;
using Infrastructure.Cloudinary;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace UnitTests.Infrastructure;

[Trait("Category", "Integration")]
public class CloudinaryImageStorageTests
{
    private static readonly IConfiguration Config = new ConfigurationBuilder()
        .AddUserSecrets<CloudinaryImageStorageTests>()
        .Build();

    private static Cloudinary BuildCloudinary() =>
        new(new Account(
            Config["Cloudinary:CloudName"],
            Config["Cloudinary:ApiKey"],
            Config["Cloudinary:ApiSecret"]
        )) { Api = { Secure = true } };

    private static IImageStorage BuildStorage(Cloudinary cloudinary) =>
        new CloudinaryImageStorage(cloudinary, NullLogger<CloudinaryImageStorage>.Instance);

    [Fact]
    public async Task DeleteAsync_ExistingImage_RemovesFromCloudinary()
    {
        var cloudinary = BuildCloudinary();

        // 上傳一張 1x1 透明 PNG 作為測試素材
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription("test.png",
                new MemoryStream(Convert.FromBase64String(
                    "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg=="))),
            PublicId = $"test/storage-test-{Guid.NewGuid():N}",
            Overwrite = true
        };

        var uploadResult = await cloudinary.UploadAsync(uploadParams);
        Assert.Equal("image", uploadResult.ResourceType);

        var publicUrl = uploadResult.SecureUrl.ToString();
        Console.WriteLine($"Uploaded: {publicUrl}");

        // 用 IImageStorage 刪除
        var storage = BuildStorage(cloudinary);
        await storage.DeleteAsync(publicUrl);

        // 確認已刪除（GetResourceAsync 應回傳 NotFound）
        var check = await cloudinary.GetResourceAsync(uploadResult.PublicId);
        Assert.Null(check?.PublicId);
    }

    [Fact]
    public async Task DeleteAsync_InvalidUrl_LogsWarningAndDoesNotThrow()
    {
        var storage = BuildStorage(BuildCloudinary());

        // 不應 throw，只 log warning
        await storage.DeleteAsync("https://example.com/not-a-cloudinary-url.png");
    }
}
