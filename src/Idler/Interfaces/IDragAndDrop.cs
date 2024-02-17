using Idler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Idler.Interfaces
{
    internal interface IDragAndDrop
    {
        void OnElementDropped(IDraggableListItem droppped, IDraggableListItem target, DragOverPlaceholderPosition placeholderPosition);
    }
}
