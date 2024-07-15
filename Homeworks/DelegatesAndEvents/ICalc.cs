using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homeworks.DelegatesAndEvents
{
    internal interface ICalc
    {
        event EventHandler<EventArgs> GetResult;
        void Divide(double value1, double value2);
        void Multy(double value1, double value2);
        void Sum(double value1, double value2);
        void Sub(double value1, double value2);
        void CancelLast();
    }
}
