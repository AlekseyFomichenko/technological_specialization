using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homeworks.DelegatesAndEvents
{
    internal class Calculator : ICalc
    {
        public int Result { get; set; } = 0;

        public event EventHandler<EventArgs> GetResult;

        Stack<int> Results = new Stack<int>();

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

        public void Divide(int value)
        {
            Results.Push(Result);
            Result /= value;
            RaisEvent();
        }

        public void Multy(int value)
        {
            Results.Push(Result);
            Result *= value;
            RaisEvent();
        }

        public void Sub(int value)
        {
            Results.Push(Result);
            Result -= value;
            RaisEvent();
        }

        public void Sum(int value)
        {
            Results.Push(Result);
            Result += value;
            RaisEvent();
        }
        void RaisEvent()
        {
            GetResult?.Invoke(this, EventArgs.Empty);
        }
    }
}
