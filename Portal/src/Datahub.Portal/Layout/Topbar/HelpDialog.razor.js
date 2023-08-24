export function retrieveUserAgent() {
    return window.navigator.userAgent;
}
export function retrieveResolution() {
    const width = window.screen.width;
    const height = window.screen.height;
    return `${width}x${height}`;
}