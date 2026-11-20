function blazorGetTimezoneOffset() {
    return new Date().getTimezoneOffset();
}

function focusElementById(elementId) {
    document.getElementById(elementId)?.focus();
}
