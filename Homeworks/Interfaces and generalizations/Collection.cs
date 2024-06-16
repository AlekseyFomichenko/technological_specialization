using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homeworks.Interfaces_and_generalizations
{
    internal class Collection<T> : IIndexable<T>
    {

        private T[] _arr;

        public T this[int index]
        {
            get => _arr[index];
            set => _arr[index] = value;
        }

        public Collection(int size)
        {
            _arr = new T[size];
        }
    }

}
