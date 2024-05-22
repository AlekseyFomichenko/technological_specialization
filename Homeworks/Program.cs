namespace Homeworks
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Написать программу-калькулятор, вычисляющую выражения вида a +b, a - b, a / b, a* b,
            //введенные из командной строки, и выводящую результат выполнения на экран.

            string[] arithmeticSymbols = { "+", "-", "*", "/" };
            if (args.Length == 0) { Console.WriteLine("Ошибка. Нужно ввести арифметическое выражение. Пример: a + b"); }
            else if (double.TryParse(args[0], out double num1)
                && arithmeticSymbols.Contains(args[1])
                && double.TryParse(args[2], out double num2))
            {
                string message = $"{num1} {args[1]} {num2} = ";

                switch (args[1])
                {
                    case "+":
                        double res = num1 + num2;
                        Console.WriteLine(message + res);
                        break;
                    case "-":
                        double res2 = num1 - num2;
                        Console.WriteLine(message + res2);
                        break;
                    case "*":
                        double res3 = num1 * num2;
                        Console.WriteLine(message + res3);
                        break;
                    case "/":
                        if (num2 == 0)
                        {
                            throw new ArgumentException("Ошибка. Делить на ноль нельзя.");
                        }
                        else
                        {
                            double res4 = Math.Round(num1 / num2, 2);
                            Console.WriteLine(message + res4);
                            break;
                        }
                    default:
                        break;
                }
            }
            else
            {
                Console.WriteLine("Ошибка. Неверный формат выражения.");
            }
        }
    }
}
