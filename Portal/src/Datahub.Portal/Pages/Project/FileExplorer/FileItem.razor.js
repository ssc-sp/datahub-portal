export function initializeDraggable(fileItemElement) {
    
    if(fileItemElement == null) {
        console.log("FileItem.initializeDraggable: fileItemElement is null");
        return;
    }
    
    function onDragStart(e) {
        console.log('onDragStart');
        e.dataTransfer.setData('text', e.target.dataset.name);
    }
    
    // Register all events
    fileItemElement.addEventListener("dragstart", onDragStart);

    // The returned object allows to unregister the events when the Blazor component is destroyed
    return {
        dispose: () => {
            fileItemElement.removeEventListener("dragstart", onDragStart);
        }
    }
}