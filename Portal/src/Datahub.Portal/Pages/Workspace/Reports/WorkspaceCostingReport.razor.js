export function retrieveTimeZone() {
    let date = new Date();
    return date.toLocaleDateString(undefined, { day: '2-digit', timeZoneName: 'long' }).substring(4)
}