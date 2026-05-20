using FinalProjectNoaRippel.ViewModels;

namespace FinalProjectNoaRippel.Views;

public partial class BlogRecipePage : ContentPage
{
    public BlogRecipePage(BlogRecipeViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}