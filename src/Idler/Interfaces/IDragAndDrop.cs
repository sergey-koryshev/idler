namespace Idler.Interfaces
{
    internal interface IDragAndDrop
    {
        void OnElementDropped(IDraggableItem droppped, IDraggableItem target);
    }
}
