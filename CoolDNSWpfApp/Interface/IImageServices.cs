using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CoolDNSWpfApp.Interface;

public interface IImageServices
{
    Task<ImageSource> GetImage(string objectKey);

    Task<string> UploadImage(BitmapImage imageSource);
}
