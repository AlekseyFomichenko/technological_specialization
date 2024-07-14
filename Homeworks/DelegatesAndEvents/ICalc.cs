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
        void Divide(int value);
        void Multy(int value);
        void Sum(int value);
        void Sub(int value);
        void CancelLast();
    }
}
