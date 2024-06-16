using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homeworks.Interfaces_and_generalizations
{
    internal interface IControllable
    {
        bool IsOn {  get; }
        void On();
        void Off();
    }
}
