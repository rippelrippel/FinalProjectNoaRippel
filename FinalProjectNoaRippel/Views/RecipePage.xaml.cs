using FinalProjectNoaRippel.ViewModels;

namespace FinalProjectNoaRippel.Views;

public partial class RecipePage : ContentPage
{
	public RecipePage(RecipePageViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}