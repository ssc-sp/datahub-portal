
var BlazorClipboadInterop = BlazorClipboadInterop || {};

BlazorClipboadInterop.ListeningForPasteEvents = function (element, dotNetObject) {
    element.addEventListener('paste', function (e) { BlazorClipboadInterop.pasteEvent(e, dotNetObject) });
};

BlazorClipboadInterop.toBase64 = file => new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.readAsDataURL(file);
    reader.onload = () => resolve(reader.result);
    reader.onerror = error => reject(error);
});

BlazorClipboadInterop.pasteEvent =
    async function (e, dotNetObject) {

        var data = await navigator.clipboard.read();
        var items = []; //is passed to C#

        for (let i = 0; i < data.length; i++) {
            var item = {};
            items.push(item);
            for (let j = 0; j < data[i].types.length; j++) {

                const type = data[i].types[j];

                const blob = await data[i].getType(type);
                if (blob) {

                    if (type.startsWith("text") == true) {
                        const content = await blob.text();
                        item[type] = content;
                    }
                    else {
                        item[type] = await BlazorClipboadInterop.toBase64(blob);
                    }
                }
            }
        }

        dotNetObject.invokeMethodAsync('Pasted', items);
        e.preventDefault();
    }
