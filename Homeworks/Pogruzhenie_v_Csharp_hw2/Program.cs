

namespace Homeworks.Pogruzhenie_v_Csharp_hw2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int[,] a = new int[3, 3] { { 7, 3, 2 }, { 4, 9, 6 }, { 1, 8, 5 } };

            int[] b = { 4, 2, 9, 3, 5, 0, 3 };

            Sort(a);

            for (int i = 0; i < a.GetLength(0); i++)
            {
                for (int j = 0; j < a.GetLength(1); j++)
                {
                    Console.Write(a[i, j]);
                }
                Console.WriteLine();
            }

            void Sort(int[,] array)
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

                //for (int i = 0;i < 3; i++)
                //{
                //    for(int j = 0;j < 3 - 1; j++)
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
        }
    }
}
