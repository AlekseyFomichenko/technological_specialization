namespace Homeworks.DelegatesAndEvents
{
    internal class Program
    {
        static void CalculatorGetResult(object sendler, EventArgs args)
        {
            Console.WriteLine($"{((Calculator)sendler).Result}");
        }
        static void Main(string[] args)
        {
            Calculator calc = new Calculator();
            calc.GetResult += CalculatorGetResult;

            calc.Sum(2);
            calc.Sub(4);
            calc.Multy(3);
            calc.CancelLast();
            calc.CancelLast();
            calc.CancelLast();
            calc.CancelLast();
            calc.CancelLast();
            calc.CancelLast();
            calc.CancelLast();

            List<int> ints = [2, 42, 5, 2, 5, 1, 63, 7, 16];

            calc.Task3(ints, x => x % 2 == 0, (x, y) => x + y, Console.WriteLine);
        }
    }
}
