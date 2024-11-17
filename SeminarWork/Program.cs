namespace SeminarWork
{
    internal class Program
    {
        static async Task<int> Task1(int[] array) => await Task.Run(() => array.Sum());
        static async Task Main(string[] args)
        {
            int[] myArray = { 1, 2, 578, 38, 38, 38, 93234, 93, 273, 89, 4 };
            int[] myArray2 = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };

            var t1 = Task1(myArray);
            var t2 = Task1(myArray2);

            int res = await t1 + await t2;
            Console.WriteLine($"{t1.Result} + {t2.Result} = {res}");
        }


    }
}
