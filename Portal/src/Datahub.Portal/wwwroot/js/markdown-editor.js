const _instances = [];

export function initializeMarkdown(elementId, dotNetObjectRef) {
    console.log("initializeMarkdown", elementId, dotNetObjectRef);
    
    const easyMDE = new EasyMDE({
        element: document.getElementById(elementId),
        hideIcons: ["side-by-side", "fullscreen"]
    });

    easyMDE.codemirror.on("change", function () {
        dotNetObjectRef.invokeMethodAsync("UpdateInternalValue", easyMDE.value());
    });

    _instances[elementId] = {
        dotNetObjectRef: dotNetObjectRef,
        elementId: elementId,
        editor: easyMDE,
    };

    return {
        initialize: true,
        dispose: () => destroy_editor(elementId)
    };
}

export function destroyMarkdown(elementId) {
    console.log("destroyMarkdown", elementId);
    
    const instances = _instances || {};
    
    // remove the js created elements from the DOM
    if (instances[elementId]) {
        document.querySelector(`#${elementId}-wrapper .EasyMDEContainer`).remove();
    }
        
    // delete the reference
    delete instances[elementId];
}

export function setValue(elementId, value) {
    console.log("calling setValue");
    const instance = _instances[elementId];
    if (instance) {
        console.log("value changed");
        instance.editor.value(value);
    }
}

