const _instances = [];

function initializeMarkdown(elementId, dotNetObjectRef) {

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
};

function destroyMarkdown(elementId) {
    const instances = _instances || {};
    delete instances[elementId];
}
