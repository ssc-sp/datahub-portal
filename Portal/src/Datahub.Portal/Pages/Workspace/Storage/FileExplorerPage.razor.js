export function copyTextToClipboard(text) {
    navigator.clipboard.writeText(text)
        .catch(e => console.error(e));
}

export async function focusStorageElement(id) {
    const findElement = targetId => document.getElementById(targetId)
        ?? document.querySelector(`[button-id="${targetId}"], [select-id="${targetId}"]`);
    const hydrate = async element => {
        if (element?.localName.startsWith('gcds-')) {
            await customElements.whenDefined(element.localName);
            await element.componentOnReady?.();
        }
    };
    const disclosure = document.getElementById('storage-selector-details');
    await hydrate(disclosure);
    const nativeDetails = disclosure?.shadowRoot?.querySelector('details');
    let element = findElement(id);
    // A collapsed disclosure hides the original trigger; removal may also delete it.
    if (id.startsWith('storage-') && !id.endsWith('-heading')
        && (!element || (disclosure?.contains(element) && !nativeDetails?.open))) {
        element = disclosure;
    }
    element ??= disclosure;
    if (!element) return;
    await hydrate(element);
    // GCDS exposes its disclosure summary as a button, outside the native details panel.
    const target = element === disclosure
        ? element.shadowRoot?.querySelector('button[aria-expanded]') ?? element.shadowRoot?.querySelector('summary') ?? element
        : element.shadowRoot?.querySelector('button, select, h1, h2, h3') ?? element;
    if (!target.hasAttribute('tabindex') && !target.matches('summary, button, select')) {
        target.setAttribute('tabindex', '-1');
    }
    target.focus();
}
