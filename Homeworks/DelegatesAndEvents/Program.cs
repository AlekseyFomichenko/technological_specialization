namespace Homeworks.DelegatesAndEvents
{
    internal class Program
    {
        static void CalculatorGetResult(object sendler, EventArgs args)
        {
            Console.WriteLine($"Ответ: {((Calculator)sendler).Result}");
        }
        static void Main(string[] args)
        {
            Calculator calc = new Calculator();
            calc.GetResult += CalculatorGetResult;

            bool isEscape() => Console.ReadKey().Key.Equals(ConsoleKey.Escape);

            void ParseInput(double value)
            {
                Console.Write("\r");
                if (double.TryParse(Console.ReadLine(), out var number)) value = number;
                else Environment.Exit(0);
            }

            bool flag = true;
            while (flag)
            {
                double input1 = 0;
                double input2 = 0;

                Console.WriteLine("введите первое число: ");
                if (isEscape()) Environment.Exit(0);
                ParseInput(input1);
                
                Console.WriteLine("введите второе число: ");
                if (isEscape()) Environment.Exit(0);
                ParseInput(input2);

                Console.Write("введите арифметическую операцию: ");
                string operation = Console.ReadLine();

                switch (operation)
                {
                    case "+":
                        calc.Sum(input1, input2);
                        break;
                    case "-":
                        calc.Sub(input1, input2);
                        break;
                    case "/":
                        calc.Divide(input1, input2);
                        break;
                    case "*":
                        calc.Multy(input1, input2);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
