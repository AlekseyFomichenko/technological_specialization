using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homeworks.Collections_Part1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            /*Есть лабиринт описанный в виде двумерного массива где 1 это стены, 0 - проход, 2 - искомая цель.
            Пример лабиринта:
            1 1 1 1 1 1 1
            1 0 0 0 0 0 1
            1 0 1 1 1 0 1
            0 0 0 0 1 0 2
            1 1 0 0 1 1 1
            1 1 1 1 1 1 1
            1 1 1 1 1 1 1
            Напишите алгоритм определяющий наличие выхода из лабиринта и выводящий на экран координаты точки выхода если таковые имеются.*/

            int[,] labirynth1 = new int[,]
            {
                {1, 1, 1, 1, 1, 1, 1 },
                {1, 0, 0, 0, 0, 0, 1 },
                {1, 0, 1, 1, 1, 0, 1 },
                {1, 0, 0, 0, 1, 0, 2 },
                {1, 1, 0, 0, 1, 1, 1 },
                {1, 1, 1, 1, 1, 1, 1 },
                {1, 1, 1, 1, 1, 1, 1 }
            };

            CustomMap customMap = new CustomMap(labirynth1);

            customMap.FindPath(3,3);
        }
    }
}
