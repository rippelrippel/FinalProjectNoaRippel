using FinalProjectNoaRippel.ViewModels;

namespace FinalProjectNoaRippel.Views;

public partial class BlogPage : ContentPage
{
    public BlogPage(BlogViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var vm = BindingContext as BlogViewModel;
        if (vm != null)
            await vm.LoadBlogRecipesAsync();
    }
}