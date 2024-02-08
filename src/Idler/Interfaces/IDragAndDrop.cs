using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Idler.Interfaces
{
    internal interface IDragAndDrop<T>
    {
        void OnElementDropped(T droppped, T target);
    }
}
