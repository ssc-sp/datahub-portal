export function copyTextToClipboard(text) {
    navigator.clipboard.writeText(text)
        .catch(e => console.error(e));
}

export async function focusStorageElement(id) {
    const findElement = targetId => document.getElementById(targetId)
        ?? document.querySelector(`[button-id="${targetId}"], [select-id="${targetId}"]`);
    let element = findElement(id);
    // Removal can make its original trigger disappear.
    element ??= findElement('storage-add-button')
        ?? findElement('storage-account-select')
        ?? document.getElementById('storage-current-container');
    if (!element) return;
    if (element.localName.startsWith('gcds-')) {
        await customElements.whenDefined(element.localName);
        await element.componentOnReady?.();
    }
    const target = element.shadowRoot?.querySelector('button, select, h1, h2, h3') ?? element;
    if (!target.hasAttribute('tabindex') && !target.matches('button, select')) {
        target.setAttribute('tabindex', '-1');
    }
    target.focus();
}
