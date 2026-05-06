using FinalProjectNoaRippel.ViewModels;

namespace FinalProjectNoaRippel.Views;

public partial class FoodListPage : ContentPage
{
    public FoodListPage(FoodListViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}