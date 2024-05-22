namespace Homeworks.Pogruzhenie_v_Csharp_hw2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int[,] a =
            { 
                { 7, 3, 2 }, 
                { 4, 9, 6 }, 
                { 1, 8, 5 } 
            };

            Print2dArray(a);
            //Sort(a);
            AnotherSort(a);
            Print2dArray(a);
        }
            static void Sort(int[,] array) //Работает наполовину
            {
                //for (int i = 0; i < 9; i++)
                //{
                //    for (int x = 0; x < array.GetLength(0); x++)
                //    {
                //        for (int y = 0; y < array.GetLength(1) - 1; y++)
                //        {

                //            if (array[x, y] > array[x, y + 1])
                //            {
                //                (array[x, y], array[x, y + 1]) = (array[x, y + 1], array[x, y]);
                //            }
                //            if (x < array.GetLength(0))
                //            {
                //                if (array[x, y + 1] > array[x + 1, y])
                //                {
                //                    (array[x, y + 1], array[x + 1, y]) = (array[x + 1, y], array[x, y + 1]);
                //                }
                //            }

                //        }
                //    }
                //}

                for (int k = 0; k < array.GetLength(0); k++)
                {
                    for (int l = 0; l < array.GetLength(1) - 1; l++)
                    {
                        for (int i = 0; i < array.GetLength(0) - 1; i++)
                        {
                            for (int j = 0; j < array.GetLength(1) - 1; j++)
                            {
                                if (array[i, j] > array[i, j + 1])
                                {
                                    int t = array[i, j];
                                    array[i, j] = array[i, j + 1];
                                    array[i, j + 1] = t;
                                }
                                if (j == 1)
                                {
                                    if (array[i, j + 1] > array[i + 1, 0])
                                    {
                                        int t = array[i, j + 1];
                                        array[i, j + 1] = array[i + 1, 0];
                                        array[i + 1, 0] = t;
                                    }
                                }
                            }

                            if (array[i, l] > array[i, l + 1])
                            {
                                int t = array[i, l];
                                array[i, l] = array[i, l + 1];
                                array[i, l + 1] = t;
                            }
                        }
                    }
                }

                //int n = array.GetLength(0);
                //int m = array.GetLength(1);

                //for (int i = 0;i < n; i++)
                //{
                //    for(int j = 0;j < m - 1; j++)
                //    {
                //        if (array[i,j] > array[i,j+1])
                //        {
                //            int t = array[i, j];
                //            array[i, j] = array[i, j + 1];
                //            array[i, j + 1] = t;
                //        }
                //    }
                //}
            }
            static void Print2dArray(int[,] array)
            {
                for (int i = 0; i < array.GetLength(0); i++)
                {
                    for (int j = 0; j < array.GetLength(1); j++)
                    {
                        Console.Write(array[i, j]);
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
            }
            static void AnotherSort(int[,] array)
            {
                int[] newArray = new int[array.Length];
                int t = 0;
                for (int i = 0; i < array.GetLength(0); i++)
                    for (int j = 0; j < array.GetLength(1); j++)
                        newArray[t++] = array[i, j];

                Array.Sort(newArray); //По сути здесь можно любой другой метод сортировки применить
                t = 0;

                for (int i = 0;i < array.GetLength(0); i++)
                    for (int j = 0; j < array.GetLength(1); j++)
                        array[i, j] = newArray[t++];
            }
            
        
    }
}
