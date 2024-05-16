export function blazorGetTimezone() {
    return Intl.DateTimeFormat().resolvedOptions().timeZone;
}
export function blazorGetScreenDimentions() {
    return window.screen.width.toString() + 'x' + window.screen.height.toString();
}