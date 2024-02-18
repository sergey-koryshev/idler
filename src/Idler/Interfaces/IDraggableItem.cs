using Idler.Models;
namespace Idler.Interfaces
{
    public interface IDraggableItem
    {
        DragOverPlaceholderPosition DragOverPlaceholderPosition { get; set; }

        int SortOrder { get; set; }
    }
}
