export function focusElement(elementId) {
    var element = document.getElementById(elementId);
    if (element) {
        element.focus();
    }
}