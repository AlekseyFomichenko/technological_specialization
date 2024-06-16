using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homeworks.Interfaces_and_generalizations
{
    internal class Device : IControllable
    {
        public bool IsOn { get; private set; } = false;

        public void On()
        {
            IsOn = true;
        }

        public void Off()
        {
            IsOn = false;
        }

    }
}
