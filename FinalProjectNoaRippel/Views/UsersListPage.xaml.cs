using FinalProjectNoaRippel.ViewModels;

namespace FinalProjectNoaRippel.Views;

public partial class UsersListPage : ContentPage
{
    public UsersListPage(UsersListViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is UsersListViewModel vm)
            vm.GetAllUsersCommand?.Execute(null); // ← loads users every time page appears
    }
}