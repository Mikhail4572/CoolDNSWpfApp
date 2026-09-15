using Amazon.S3;
using Amazon.S3.Model;
using CoolDNSWpfApp.Interface;
using CoolDNSWpfApp.ModelsUI;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CoolDNSWpfApp.Services;


/*
Имя контейнера: CoolDnsContainer
keyID: 005f29d16621a350000000001
applicationKey: K005qmPg5IfADbZy+Ht7ojANEGV1sZI
keyName: CoolDnsKey



"BackblazeB2": {
    "AccessKey": "005f29d16621a350000000001",
    "SecretKey": "K005qmPg5IfADbZy+Ht7ojANEGV1sZI",
    "Bucket": "CoolDnsContainer",
    "ServiceUrl": "https://s3.us-east-005.backblazeb2.com"
  }
*/

public class ImageServices : IImageServices
{
    private readonly IAmazonS3 _amazonS3;

    public ImageServices(IAmazonS3 amazonS3) =>
        _amazonS3 = amazonS3;

    public async Task<ImageSource> GetImage(string objectKey)
    {
        try
        {
            var response = await _amazonS3.GetObjectAsync("CoolDnsContainer", objectKey);

            MemoryStream memory = new();

            await response.ResponseStream.CopyToAsync(memory);

            memory.Position = 0;

            BitmapImage bitmap = new();

            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = memory;
            bitmap.EndInit();

            //bitmap.Freeze();

            return bitmap;
        }
        catch
        {
            return null;
        }
    }

    public async Task<string> UploadImage(BitmapImage bitmap)
    {
        await using MemoryStream stream = new();

        var contentType = $"image/{Path.GetExtension(bitmap.UriSource.LocalPath).TrimStart('.')}";

        BitmapEncoder encoder = contentType switch
        {
            "image/jpeg" => new JpegBitmapEncoder(),
            _ => new PngBitmapEncoder()
        };

        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        encoder.Save(stream);

        stream.Position = 0;

        var extension = Path.GetExtension(Path.GetFileName(bitmap.UriSource.LocalPath));

        var objectKey = Guid.NewGuid() + extension;

        PutObjectRequest request = new()
        {
            BucketName = "CoolDnsContainer",
            Key = objectKey,
            InputStream = stream,
            ContentType = contentType
        };

        await _amazonS3.PutObjectAsync(request);

        return objectKey;
    }

    public async IAsyncEnumerable<ProductUI> LoadImage(List<ProductUI> products)
    {
        var tasks = products.Select(async item =>
        {
            item.Images = [ ..await Task.WhenAll(item.Attachments.Where(x => x.ContentType.Contains("png", StringComparison.OrdinalIgnoreCase) ||
                                x.ContentType.Contains("jpg", StringComparison.OrdinalIgnoreCase)).Select(x => GetImage(x.ObjectKey)))];

            return item;
        }).ToList();

        foreach (var task in tasks)
        {
            var prUi = await task;
            prUi.Image = prUi.Images.FirstOrDefault();
            yield return prUi;
        }
    }
}
