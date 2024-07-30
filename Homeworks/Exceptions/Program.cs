namespace Homeworks.Exceptions
{
    internal class Program
    {
        static void CalculatorGetResult(object sendler, EventArgs args)
        {
            Console.WriteLine($"Ответ: {((Calculator)sendler).Result}");
        }
        static void Execute (Action<double> action, double value = 10)
        {
            try
            {
                action.Invoke (value);
            }
            catch (CalcDivideByZeroException ex)
            {
                Console.WriteLine(ex);
            }
            catch (CalcOverFlowException ex)
            {
                Console.WriteLine(ex);
            }
        }
        static void Main(string[] args)
        {
            Calculator calc = new Calculator();
            calc.GetResult += CalculatorGetResult;

            Execute(calc.Sub, int.MaxValue);
            Execute(calc.Sub, 2.1);
            Execute(calc.Divide, 0);
            Execute(calc.Sub, 1.89);
            Execute(calc.Multy);
        }
    }
}
