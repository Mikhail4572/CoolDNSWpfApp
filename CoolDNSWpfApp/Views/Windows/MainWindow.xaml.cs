using CoolDNSWpfApp.Models;
using CoolDNSWpfApp.ModelsUI;
using CoolDNSWpfApp.Services;
using CoolDNSWpfApp.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Windows;

namespace CoolDNSWpfApp.Views.Windows;


public partial class MainWindow : Window
{
    public MainWindow(ImageServices imageServices, CoolDNSDBContext context)
    {
        InitializeComponent();
        

        this.Loaded += async (s, e) =>
        {
            var products = await context.Products.OrderByDescending(x => x.Rating).Select(x => new ProductUI
            {
                Id = x.Id,
                Count = x.Count,
                Description = x.Description,
                Attachments = x.Attachments,
                Discount = x.Discount,
                FeedbackCount = x.FeedbackCount,
                Name = x.Name,
                Rating = x.Rating,
                Tags = x.Tags
            }).ToListAsync();

            MainViewModel mainViewModel = new(context, imageServices);

            this.DataContext = mainViewModel;

            await foreach (var item in imageServices.LoadImage(products))
                mainViewModel.Products.Add(item);
        };
    }

    //private async IAsyncEnumerable<ProductUI> LoadImage(List<ProductUI> products)
    //{
    //    var tasks = products.Select(async item =>
    //    {
    //        item.Images = [ ..await Task.WhenAll(item.Attachments.Where(x => x.ContentType.Contains("png", StringComparison.OrdinalIgnoreCase) ||
    //                            x.ContentType.Contains("jpg", StringComparison.OrdinalIgnoreCase)).Select(x => _imageServices.GetImage(x.ObjectKey)))];

    //        return item;
    //    }).ToList();

    //    foreach (var task in tasks)
    //    {
    //        var prUi = await task;
    //        prUi.Image = prUi.Images.FirstOrDefault();
    //        yield return prUi;
    //    }
    //}
}
