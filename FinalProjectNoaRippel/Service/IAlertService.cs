using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProjectNoaRippel.Service.DBService
{ 
    // ממשק המגדיר את הפעולות להצגת הודעות למשתמש.

    public interface IAlertService
    {       
        // מציג הודעת התראה עם כותרת הודעה וכפתור סגירה

        Task ShowAlertAsync(string title, string message, string cancel);
    }
}
