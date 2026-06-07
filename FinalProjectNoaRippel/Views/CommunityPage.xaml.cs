using FinalProjectNoaRippel.ViewModels;

namespace FinalProjectNoaRippel.Views
{
    public partial class CommunityPage : ContentPage
    {
        public CommunityPage()
        {
            InitializeComponent();
            BindingContext = new CommunityViewModel();
        }
    }
}