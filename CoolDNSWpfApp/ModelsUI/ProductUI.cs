using System.Collections.ObjectModel;
using System.Windows.Media;
using CoolDNSWpfApp.Models;
using System.Collections.Specialized;
using System.ComponentModel;

namespace CoolDNSWpfApp.ModelsUI;

public class ProductUI : Product, INotifyPropertyChanged
{
    public List<ImageSource> Images { get; set; } 
    public ImageSource Image
    {
        get => field;
        set
        {
            field = value;
            OnPropertyChanged(nameof(Image));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName = null) =>    
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
