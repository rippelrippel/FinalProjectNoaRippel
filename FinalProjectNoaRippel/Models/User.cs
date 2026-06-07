using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProjectNoaRippel.Models
{
    public class User
    {
        public string? Id { get; set; }// מזהה ייחודי שנוצר בפיירבייס אוטיטיקשין

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? UserEmail { get; set; }
        public string? UserPassword { get; set; }
        public DateTime UBDate { get; set; }
        public DateTime RegDate { get; set; }
        public bool IsAdmin { get; set; } = false;
        public string? UserMobile { get; set; }
    }
}
