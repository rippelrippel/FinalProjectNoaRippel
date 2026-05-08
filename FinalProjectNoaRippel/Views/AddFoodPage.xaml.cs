using FinalProjectNoaRippel.ViewModels;

namespace FinalProjectNoaRippel.Views;

public partial class AddFoodPage : ContentPage
{
    public AddFoodPage(AddFoodViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}