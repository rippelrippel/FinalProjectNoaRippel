using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FinalProjectNoaRippel.ViewModels
{
    // מחלקת בסיס שממנה יורשים כל הוויומודל באפליקציה.
    public class ViewModelBase : INotifyPropertyChanged
    {
        private bool _isBusy;// שולט על הצגת ספינר כשהאפליקציה מבצעת פעולה ברקע
        public bool IsBusy
        {
            get { return _isBusy; }
            set { if (_isBusy != value) { _isBusy = value; OnPropertyChanged(); } }
        }

        // אירוע שמופעל בכל שינוי בשדה היוזר אינטרפיס מאזין לאירוע הזה ומתעדכן
        public event PropertyChangedEventHandler? PropertyChanged;

        // מפעיל את אירוע PropertyChanged
        //CallerMemberNam שם הפונקציה או השדה שקרא לפונקציה 

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
