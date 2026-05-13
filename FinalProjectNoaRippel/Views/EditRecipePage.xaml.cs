using FinalProjectNoaRippel.ViewModels;

namespace FinalProjectNoaRippel.Views;

public partial class EditRecipePage : ContentPage
{
	public EditRecipePage(EditRecipeViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }

}