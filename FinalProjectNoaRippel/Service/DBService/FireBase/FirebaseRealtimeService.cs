using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProjectNoaRippel.Service.DBService.FireBase
{

    public class FirebaseRealtimeService : IDBInstance
    {
        protected FirebaseClient? _firebaseClient;

        public FirebaseRealtimeService()
        {
            //יוצר את החיבור
            _firebaseClient = new FirebaseClient("https://finalprojectnoarippel-default-rtdb.europe-west1.firebasedatabase.app/");
        }
        public string Info()
        {
            //תיאור
            return "Type: Google Firebase RealTime Database client";
        }
    }
}
