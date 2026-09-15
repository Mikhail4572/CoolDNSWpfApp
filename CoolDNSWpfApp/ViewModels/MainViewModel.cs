using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CoolDNSWpfApp.Models;
using CoolDNSWpfApp.ModelsUI;
using CoolDNSWpfApp.Services;
using CoolDNSWpfApp.Views.Windows;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Media;
using System.Windows;


namespace CoolDNSWpfApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private List<ObservableObject> _casheViewModels = [];

    [ObservableProperty]
    private ObservableCollection<ProductUI> _filterProducts = [];

    [ObservableProperty]
    private ObservableCollection<ProductUI> _products = [];

    private readonly CoolDNSDBContext _context;
    private readonly ImageServices _imageServices;

    [ObservableProperty]
    private string searchText;

    private DateTimeOffset _lastRun = DateTimeOffset.MinValue;
    private TimeSpan _interval = TimeSpan.FromSeconds(2);

    public MainViewModel() { }

    public MainViewModel(CoolDNSDBContext context, ImageServices imageServices)
    {
        this._context = context;
        this._imageServices = imageServices;
        Products.CollectionChanged += (s, e) =>
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems is not null && e.NewItems.Count > 0 && e.NewItems[0] is ProductUI addPrUi)
                        _filterProducts.Add(addPrUi);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems is not null && e.OldItems.Count > 0 && e.OldItems[0] is ProductUI remPrUi)
                        _filterProducts.Remove(remPrUi);
                    break;

                default:
                    break;
            }
        };
    }

    [RelayCommand]
    public async Task LastImage(ProductUI product)
    {
        try
        {
            ProductUI productFromObs = Products.First(x => x.Id == product.Id);

            if (object.ReferenceEquals(product.Image, productFromObs.Images[0]))
                return;

            productFromObs.Image = productFromObs.Images[productFromObs.Images.IndexOf(product.Image) - 1];
        }
        catch { }
    }

    [RelayCommand]
    public async Task NextImage(ProductUI product)
    {
        try
        {
            ProductUI productFromObs = Products.First(x => x.Id == product.Id);

            if (object.ReferenceEquals(product.Image, productFromObs.Images[^1]))
                return;

            productFromObs.Image = productFromObs.Images[productFromObs.Images.IndexOf(product.Image) + 1];
        }
        catch { }
    }

    [RelayCommand]
    public async Task ToAddWindow()
    {
        var currentWindow = App.Current.Windows.OfType<MainWindow>().FirstOrDefault();

        AddProductWindow window = new()
        {
            Owner = currentWindow,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        if (_casheViewModels.OfType<AddProductViewModel>().FirstOrDefault() is not AddProductViewModel vm)
        {
            var tags = _context.Tags.ToList();
            vm = new AddProductViewModel(_context, tags, _imageServices);

            window.DataContext = vm;

            _casheViewModels.Add(vm);
        }

        else
            window.DataContext = vm;

        window.ShowDialog();
    }

    partial void OnSearchTextChanged(string value)
    {
        if (DateTimeOffset.UtcNow - _lastRun < _interval)
            return;

        _lastRun = DateTimeOffset.UtcNow;

        FilterProducts.Clear();

        if (string.IsNullOrEmpty(value))
        {
            foreach (var item in Products)
                FilterProducts.Add(item);

            return;
        }

        var searchResult = Products.Where(x => x.Name.Contains(value, StringComparison.OrdinalIgnoreCase));

        foreach (var item in searchResult)
            FilterProducts.Add(item);
    }
}


