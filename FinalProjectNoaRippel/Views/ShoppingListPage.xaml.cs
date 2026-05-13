using FinalProjectNoaRippel.ViewModels;

namespace FinalProjectNoaRippel.Views;

public partial class ShoppingListPage : ContentPage
{
	public ShoppingListPage(ShoppingListViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;

    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // ???? ???? ?? ?????? ?-Firebase
        if (BindingContext is ShoppingListViewModel vm)
            await vm.ReloadItemsAsync();
    }
}