using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homeworks.Collections_Part1
{
    internal class CustomMap
    {
        private Stack<Tuple<int, int>> _path = new Stack<Tuple<int, int>>();
        public int[,] map;

        public CustomMap(int[,] myArray)
        {
            map = myArray;
        }

        public int HasExit(int i, int j)
        {
            int resault = 0;
            Console.WriteLine($"Респаун: {map[i, j]}");
            if (map[i, j] == 0) _path.Push(new(i, j));

            while (_path.Count > 0)
            {

                var current = _path.Pop();

                Console.WriteLine($"Текущее местоположение: {current.Item1},{current.Item2}");

                if (IsExit(current))
                {
                    Console.WriteLine($"Путь найден {current.Item1},{current.Item2} ");
                    resault++;
                }

                map[current.Item1, current.Item2] = 1;

                if (IsMoveDown(current))
                    _path.Push(new(current.Item1 + 1, current.Item2));

                if (IsMoveLeft(current))
                    _path.Push(new(current.Item1, current.Item2 + 1));

                if (IsMoveUp(current))
                    _path.Push(new(current.Item1 - 1, current.Item2));

                if (IsMoveRight(current))
                    _path.Push(new(current.Item1, current.Item2 - 1));
            }
            return resault;
        }

        private bool IsExit(Tuple<int, int> current) => map[current.Item1, current.Item2] == 2;
        private bool IsMoveUp(Tuple<int, int> current)
        {
            return current.Item1 > 0 && map[current.Item1 - 1, current.Item2] != 1;
        }
        private bool IsMoveDown(Tuple<int, int> current)
        {
            return current.Item1 + 1 < map.GetLength(0) && map[current.Item1 + 1, current.Item2] != 1;
        }
        private bool IsMoveLeft(Tuple<int, int> current)
        {
            return current.Item2 + 1 < map.GetLength(1) && map[current.Item1, current.Item2 + 1] != 1;
        }
        private bool IsMoveRight(Tuple<int, int> current)
        {
            return current.Item2 > 0 && map[current.Item1, current.Item2 - 1] != 1;
        }
    }
}
