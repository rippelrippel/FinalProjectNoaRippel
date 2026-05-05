using FinalProjectNoaRippel.ViewModels;

namespace FinalProjectNoaRippel.Views;

public partial class UserDetailsPage : ContentPage
{
    public UserDetailsPage(UserDetailsPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}