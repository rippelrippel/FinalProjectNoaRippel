using FinalProjectNoaRippel.Models;
using FinalProjectNoaRippel.Views;

namespace FinalProjectNoaRippel
{

    public partial class App : Application
    {

        public User? CurrentUser { get; set; } = null;

        public App(SignInPage signInPage)
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var signInPage = IPlatformApplication.Current!.Services.GetService<SignInPage>()!;
            return new Window(signInPage);
        }
    }
}