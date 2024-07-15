using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Homeworks.DelegatesAndEvents
{
    internal class Calculator : ICalc
    {
        public double Result { get; set; } = 0;

        public event EventHandler<EventArgs> GetResult;
        

        Stack<double> Results = new Stack<double>();

        public void ClearResult()
        {
            Results.Clear();
            Result = 0;
        }
        public void Task3 (List<int> ints, Predicate<int> isEven, Func<int, int, int> op, Action<int> action)
        {
            int sum = 0;
            foreach (var item in ints)
            {
                if (isEven(item))
                {
                    sum = op(item, sum);
                    action(sum);
                }
            }
            Result = sum;
            RaisEvent();
        }
        public void CancelLast()
        {
            if(Results.Count == 0) return;
            Result = Results.Pop();
            RaisEvent();
        }

        public void Divide(double value1, double value2)
        {
            Results.Push(Result);
            Result = value1 / value2;
            RaisEvent();
        }

        public void Multy(double value1, double value2)
        {
            Results.Push(Result);
            Result = value1 * value2;
            RaisEvent();
        }

        public void Sub(double value1, double value2)
        {
            Results.Push(Result);
            Result = value1 - value2;
            RaisEvent();
        }

        public void Sum(double value1, double value2)
        {
            Results.Push(Result);
            Result = value1 + value2;
            RaisEvent();
        }
        void RaisEvent()
        {
            GetResult?.Invoke(this, EventArgs.Empty);
        }
    }
}
