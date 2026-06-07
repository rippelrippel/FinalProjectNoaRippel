using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProjectNoaRippel.Service.DBService
{
    // ממשק בסיסי לחיבור למסד נתונים.
    // מספק מידע על סוג החיבור הקיים.
    public interface IDBInstance
    {
        string Info();
    }
}
