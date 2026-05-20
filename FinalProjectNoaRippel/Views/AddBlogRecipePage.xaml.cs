using FinalProjectNoaRippel.ViewModels;

namespace FinalProjectNoaRippel.Views;

public partial class AddBlogRecipePage : ContentPage
{
    public AddBlogRecipePage(AddBlogRecipeViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
