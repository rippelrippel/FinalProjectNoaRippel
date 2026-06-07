using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FinalProjectNoaRippel.Service.DBService.AlertService;

namespace FinalProjectNoaRippel.Service.DBService
{
    // מספק דרך מרכזית להצגת הודעות אלרט למשתמש דרך ה
    // Shell

    public class AlertService : IAlertService
    {       
        // מציג הודעת התראה למשתמש דרך ה
        // Shell הנוכחי
        public Task ShowAlertAsync(string title, string message, string cancel)
        {
            return Shell.Current.DisplayAlert(title, message, cancel);
        }
    }
}
