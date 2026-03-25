using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.CommonLib
{
    public enum Notify
    {
        DO_NOT_NOTIFY=0,
        ONLY_RESPONSIBLE=1,
        ONLY_CO_RESPONSIBLE=2,
        NOTIFY_ALL=3,
    }
}
