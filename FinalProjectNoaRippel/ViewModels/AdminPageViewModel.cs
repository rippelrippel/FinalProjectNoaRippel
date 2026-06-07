using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

// מנהל את דף האדמין — נגיש רק למשתמשים עם הרשאת מנהל.
// מכיל כפתור אחד לניווט לרשימת כל המשתמשים.
namespace FinalProjectNoaRippel.ViewModels
{
    public class AdminPageViewModel : ViewModelBase
    {
        public ICommand ViewUsersCommand { get; }// מנווט לדף רשימת כל המשתמשים במערכת


        public AdminPageViewModel()
        {
            ViewUsersCommand = new Command(async () =>
                await Shell.Current.GoToAsync("//UsersListPage"));
        }
    }
}
