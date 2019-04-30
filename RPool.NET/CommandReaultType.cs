using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPool.NET
{
    public enum CommandReaultType
    {

        Success=1,
        Error=0,
        ProcessNotReady=-1,
        ProcessMayClosed=-2
    }
}
