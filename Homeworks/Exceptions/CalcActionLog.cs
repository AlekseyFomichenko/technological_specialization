using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homeworks.Exceptions
{
    internal class CalcActionLog
    {
        public CalcAction CalcAction { get; private set; }

        public int CalcArgument { get; private set; }

        public CalcActionLog(CalcAction calcAction, int caclArgument)
        {
            CalcAction = calcAction;
            CalcArgument = caclArgument;
        }

        public CalcActionLog(CalcAction calcAction, double caclArgument)
        {
            CalcAction = calcAction;
            CalcArgument = (int)caclArgument;
        }
    }
}
