export function retrieveUserAgent() {
    return window.navigator.userAgent;
}
export function retrieveResolution() {
    const width = window.screen.width;
    const height = window.screen.height;
    return `${width}x${height}`;
}
export function retrieveTimeZone() {
    let date = new Date();
    return date.toLocaleDateString(undefined, { day: '2-digit', timeZoneName: 'long' }).substring(4)
}