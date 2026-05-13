using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProjectNoaRippel.Service
{
    public interface IAppLogger
    {
        void LogDebug(string message);
        void LogError(string message);
    }
}
