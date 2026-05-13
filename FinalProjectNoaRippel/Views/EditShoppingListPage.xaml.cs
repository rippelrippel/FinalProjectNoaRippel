using FinalProjectNoaRippel.ViewModels;

namespace FinalProjectNoaRippel.Views;

public partial class EditShoppingListPage : ContentPage
{
	public EditShoppingListPage(EditShoppingListViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;

    }
}