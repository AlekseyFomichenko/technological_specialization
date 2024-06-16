using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homeworks.Interfaces_and_generalizations
{
    internal interface IMatrix<T>
    {
        T this[int row, int column] { get; set; }

        void PrintMatrix();
    }
}
