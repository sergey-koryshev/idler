using Idler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.HumanInterfaceDevice;

namespace Idler.Interfaces
{
    public interface IDraggableItem
    {
        DragOverPlaceholderPosition DragOverPlaceholderPosition { get; set; }

        int SortOrder { get; set; }
    }
}
