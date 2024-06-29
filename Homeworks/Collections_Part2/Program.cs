namespace Homeworks.Collections_Part2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int[] myArray = RandomArray(100, 200);
            int term = 34;
            int sum = 333;
            Print(myArray);
            Find(myArray, term, sum);
        }

        static int[] RandomArray(int length, int range)
        {
            Random random = new Random();
            return Enumerable.Range(0, length).Select(x => random.Next(range)).ToArray();
        }

        static void Find(int[] array, int term, int sum)
        {
            HashSet<int> set = new HashSet<int>();
            int temp = sum - term;
            foreach (int item in array)
            {
                if (set.Contains(temp - item))
                {
                    Console.WriteLine($"Искомые числа: {temp - item} и {item}");
                    return;
                }
                else
                    set.Add(item);
            }
            Console.WriteLine("Числа не найдены");
        }

        static void Print(int[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if(i % 10 == 9) Console.WriteLine();
                Console.Write(array[i] + "\t");
            }
            Console.WriteLine("\n");
        }
    }
}
