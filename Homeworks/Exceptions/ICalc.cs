using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homeworks.Exceptions
{
    internal interface ICalc
    {
        event EventHandler<EventArgs> GetResult;
        void Divide(int value1);
        void Multy(int value1);
        void Sum(int value1);
        void Sub(int value1);
        void Divide(double value1);
        void Multy(double value1);
        void Sum(double value1);
        void Sub(double value1);
        void CancelLast();
    }
}
