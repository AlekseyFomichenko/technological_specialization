using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homeworks.Exceptions
{
    internal class CalcException : Exception
    {
        public CalcException(string message, Stack<CalcActionLog> actionLogs) : base(message) 
        {
            ActionLogs = actionLogs;
        }
        public CalcException(string message, Exception e) : base(message, e) { }
        public Stack<CalcActionLog> ActionLogs { get; private set; }
        public override string ToString()
        {
            return Message + ": " + string.Join("\n", ActionLogs.Select(x => $"{x.CalcAction}: {x.CalcArgument}"));
        }
    }
    internal class CalcDivideByZeroException : CalcException
    {
        public CalcDivideByZeroException(string message, Stack<CalcActionLog> actionLogs) : base(message, actionLogs) { }
        public CalcDivideByZeroException(string message, Exception e) : base(message, e) { }
    }

    internal class CalcOverFlowException : CalcException
    {
        public CalcOverFlowException(string message, Stack<CalcActionLog> actionLogs) : base(message, actionLogs) { }
        public CalcOverFlowException(string message, Exception e) : base(message, e) { }
    }
}
