using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FinalProjectNoaRippel.ViewModels
{
    public class AdminPageViewModel : ViewModelBase
    {
        public ICommand ViewUsersCommand { get; }

        public AdminPageViewModel()
        {
            ViewUsersCommand = new Command(async () =>
                await Shell.Current.GoToAsync("//UsersListPage"));
        }
    }
}
