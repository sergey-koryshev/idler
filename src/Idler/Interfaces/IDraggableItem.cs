namespace Idler.Interfaces
{
    using Idler.Models;

    public interface IDraggableItem
    {
        DragOverPlaceholderPosition DragOverPlaceholderPosition { get; set; }

        int SortOrder { get; set; }
    }
}
