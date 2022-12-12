export function copyTextToClipboard(text){
    navigator.clipboard.writeText(text)
        .catch(e => console.error(e));
}