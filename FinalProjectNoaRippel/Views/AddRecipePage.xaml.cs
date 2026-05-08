using FinalProjectNoaRippel.ViewModels;

namespace FinalProjectNoaRippel.Views;

public partial class AddRecipePage : ContentPage
{
    public AddRecipePage(AddRecipeViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}