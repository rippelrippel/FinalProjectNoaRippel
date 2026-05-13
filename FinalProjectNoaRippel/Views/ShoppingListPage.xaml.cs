using FinalProjectNoaRippel.ViewModels;

namespace FinalProjectNoaRippel.Views;

public partial class ShoppingListPage : ContentPage
{
	public ShoppingListPage(ShoppingListViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;

    }
}