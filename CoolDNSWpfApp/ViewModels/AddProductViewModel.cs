using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CoolDNSWpfApp.Models;
using CoolDNSWpfApp.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Tag = CoolDNSWpfApp.Models.Tag;


namespace CoolDNSWpfApp.ViewModels;

public partial class AddProductViewModel : ObservableObject
{

    [ObservableProperty]
    private ObservableCollection<Tag> selected_tags;

    [ObservableProperty]
    private Tag selectedTag;

    [ObservableProperty]
    private ObservableCollection<Tag> tags;

    [ObservableProperty]
    private Product product;

    [ObservableProperty]
    private ObservableCollection<ImageSource> images;

    private readonly CoolDNSDBContext _context;
    private readonly ImageServices _imageServices;

    public AddProductViewModel() { }
    public AddProductViewModel(CoolDNSDBContext context, IEnumerable<Tag> tags, ImageServices imageServices)
    {
        Selected_tags = [];
        _context = context;
        this.tags = new(tags);
        Product = new();
        Images = [];
        _imageServices = imageServices;
    }

    [RelayCommand]
    public async Task AddProduct()
    {
        try
        {
            ArgumentException.ThrowIfNullOrEmpty(Product.Name);
            ArgumentException.ThrowIfNullOrEmpty(Product.Description);


            if (Product.Count <= 0 || Product.FeedbackCount <= 0)
            {
                MessageBox.Show("заполните все поля");
                return;
            }

            if (Images.Count < 2)
            {
                MessageBox.Show("добавьте хотя бы 2 картинки");
                return;
            }

            if (Selected_tags.Count < 2)
            {
                MessageBox.Show("добавьте хотя бы 2 тега");
                return;
            }

            List<Attachment> attachments = [.. await Task.WhenAll(Images.Select(x => (BitmapImage)x).Select(async x => new Attachment
            {
                ContentType = $"image/{Path.GetExtension(x.UriSource.LocalPath).TrimStart('.')}",
                FileName = Path.GetFileName(x.UriSource.LocalPath),
                ObjectKey = await _imageServices.UploadImage(x)
            }))];

            foreach (var item in attachments)
                Product.Attachments.Add(item);

            foreach (var item in Selected_tags)
                Product.Tags.Add(item);

            _context.Products.Add(Product);

            await _context.SaveChangesAsync();

            MessageBox.Show("Успешно)");
            Product = new();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }


    [RelayCommand]
    public void AddImage()
    {
        OpenFileDialog fileDialog = new()
        {
            Title = "Выберите изображение",
            Filter = "Изображения (*.png;*.jpg)|*.png;*.jpg|Все файлы (*.*)|*.*",
            Multiselect = true
        };

        if (fileDialog.ShowDialog() == true)
        {
            var images = fileDialog.FileNames.Select(x => GetBitmapImageFromPath(x)).ToList();

            foreach (var item in images)
                Images.Add(item);
        }
    }

    private BitmapImage GetBitmapImageFromPath(string path)
    {
        try
        {
            BitmapImage bitmap = new();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path, UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            return bitmap;
        }
        catch
        {
            return null;
        }
    }

    partial void OnSelectedTagChanged(Tag value)
    {
        if (value != null && !Selected_tags.Any(x => x.Id == value.Id))
            Selected_tags.Add(value);
    }
}
