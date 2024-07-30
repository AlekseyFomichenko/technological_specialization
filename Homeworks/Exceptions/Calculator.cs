using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Homeworks.Exceptions
{
    internal class Calculator : ICalc
    {
        public double Result { get; set; } = 0;

        public event EventHandler<EventArgs> GetResult;
        
        Stack<double> Results = new Stack<double>();

        Stack<CalcActionLog> actions = new Stack<CalcActionLog>();
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

        public void Divide(int value1)
        {
            if (value1 == 0)
            {
                actions.Push(new CalcActionLog(CalcAction.Divide, value1));
                throw new CalcDivideByZeroException("Нельзя делить на ноль", actions);
            }
            Results.Push(Result);
            Result /= value1;
            RaisEvent();
        }

        public void Multy(int value1)
        {
            long temp = (long)(value1 * Result);
            if (temp > int.MaxValue)
            {
                actions.Push(new CalcActionLog(CalcAction.Multy, value1));
                throw new CalcOverFlowException("Переполнение", actions);
            }
            Results.Push(Result);
            Result *= value1;
            RaisEvent();
        }

        public void Sub(int value1)
        {
            int temp = (int)Result - value1;
            if (temp < int.MinValue || (Result == int.MinValue && value1 == int.MaxValue))
            {
                actions.Push(new CalcActionLog(CalcAction.Sub, value1));
                throw new CalcOverFlowException("Переполнение", actions);
            }
            Results.Push(Result);
            Result -= value1;
            RaisEvent();
        }

        public void Sum(int value1)
        {
            ulong temp = (ulong)(Result + value1);
            if (temp > int.MaxValue)
            {
                actions.Push(new CalcActionLog(CalcAction.Sum, value1)); 
                throw new CalcOverFlowException("Переполнение", actions);
            }
            Results.Push(Result);
            Result += value1;
            RaisEvent();
        }
        void RaisEvent()
        {
            GetResult?.Invoke(this, EventArgs.Empty);
        }

        public void Divide(double value1)
        {
            if (value1 == 0)
            {
                actions.Push(new CalcActionLog(CalcAction.Divide, value1));
                throw new CalcDivideByZeroException("Нельзя делить на ноль", actions);
            }
            Results.Push(Result);
            Result /= value1;
            RaisEvent();
        }

        public void Multy(double value1)
        {
            long temp = (long)(value1 * Result);
            if (temp > int.MaxValue)
            {
                actions.Push(new CalcActionLog(CalcAction.Multy, value1));
                throw new CalcOverFlowException("Переполнение", actions);
            }
            Results.Push(Result);
            Result *= value1;
            RaisEvent();
        }

        public void Sum(double value1)
        {
            ulong temp = (ulong)(Result + value1);
            if (temp > int.MaxValue)
            {
                actions.Push(new CalcActionLog(CalcAction.Sum, value1));
                throw new CalcOverFlowException("Переполнение", actions);
            }
            Results.Push(Result);
            Result += value1;
            RaisEvent();
        }

        public void Sub(double value1)
        {
            int temp = (int)(Result - value1);
            if (temp < int.MinValue || (Result == int.MinValue && value1 == int.MaxValue))
            {
                actions.Push(new CalcActionLog(CalcAction.Sub, value1));
                throw new CalcOverFlowException("Переполнение", actions);
            }
            Results.Push(Result);
            Result -= value1;
            RaisEvent();
        }
    }
}
