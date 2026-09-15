using System.Security.Cryptography;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Data;

namespace CoolDNSWpfApp.Converters;

public class TagColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (!int.TryParse(value.ToString(), out var tagId))
            return "#00000000";

        var randomColor = new
        {
            r = System.Convert.ToByte(RandomNumberGenerator.GetInt32(0, 100)),
            g = System.Convert.ToByte(RandomNumberGenerator.GetInt32(0, 255)),
            b = System.Convert.ToByte(RandomNumberGenerator.GetInt32(0, 255))
        };


        return tagId switch
        {
            1 => "#f1edff",
            2 => "#f2e5ff",
            3 => "#FFE5E5",
            4 => "#c179fd",
            5 => "#d75cab",
            6 => "#0f6cbd",
            _ => Color.FromRgb(randomColor.r, randomColor.g, randomColor.b).ToString()
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
}
