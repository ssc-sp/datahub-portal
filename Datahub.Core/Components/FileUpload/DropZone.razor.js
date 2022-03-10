// https://www.meziantou.net/upload-files-with-drag-drop-or-paste-from-clipboard-in-blazor.htm




export function initializeFileDropZone(dropZoneElement, inputFile, dotNetHelper) {
    // Add a class when the user drags a file over the drop zone
    function onDragHover(e) {
        e.preventDefault();
        dropZoneElement.classList.add("hover");
    }

    function onDragLeave(e) {
        e.preventDefault();
        dropZoneElement.classList.remove("hover");
    }

    // Handle the paste and drop events
    async function onDrop(e) {
        e.preventDefault();
        dropZoneElement.classList.remove("hover");
        console.log("here");
        
        // // Set the files property of the input element and raise the change event
        inputFile.files = e.dataTransfer.files;
        const event = new Event('change', { bubbles: true });
        inputFile.dispatchEvent(event);
    }

    function onPaste(e) {
        // Set the files property of the input element and raise the change event
        // inputFile.files = e.clipboardData.files;
        const event = new Event('change', { bubbles: true });
        // inputFile.dispatchEvent(event);
    }

    // Register all events
    dropZoneElement.addEventListener("dragenter", onDragHover);
    dropZoneElement.addEventListener("dragover", onDragHover);
    dropZoneElement.addEventListener("dragleave", onDragLeave);
    dropZoneElement.addEventListener("drop", onDrop);
    dropZoneElement.addEventListener('paste', onPaste);

    // The returned object allows to unregister the events when the Blazor component is destroyed
    return {
        dispose: () => {
            dropZoneElement.removeEventListener('dragenter', onDragHover);
            dropZoneElement.removeEventListener('dragover', onDragHover);
            dropZoneElement.removeEventListener('dragleave', onDragLeave);
            dropZoneElement.removeEventListener("drop", onDrop);
            dropZoneElement.removeEventListener('paste', handler);
        }
    }
}


// https://stackoverflow.com/a/53058574

// Drop handler function to get all files
async function getAllFileEntries(dataTransferItemList) {
    let fileEntries = [];
    // Use BFS to traverse entire directory/file structure
    let queue = [];
    // Unfortunately dataTransferItemList is not iterable i.e. no forEach
    for (let i = 0; i < dataTransferItemList.length; i++) {
        queue.push(dataTransferItemList[i].webkitGetAsEntry());
    }
    while (queue.length > 0) {
        let entry = queue.shift();
        if (entry.isFile) {
            fileEntries.push(entry);
        } else if (entry.isDirectory) {
            queue.push(...await readAllDirectoryEntries(entry.createReader()));
        }
    }
    return fileEntries;
}

// Get all the entries (files or sub-directories) in a directory 
// by calling readEntries until it returns empty array
async function readAllDirectoryEntries(directoryReader) {
    let entries = [];
    let readEntries = await readEntriesPromise(directoryReader);
    while (readEntries.length > 0) {
        entries.push(...readEntries);
        readEntries = await readEntriesPromise(directoryReader);
    }
    return entries;
}

// Wrap readEntries in a promise to make working with readEntries easier
// readEntries will return only some of the entries in a directory
// e.g. Chrome returns at most 100 entries at a time
async function readEntriesPromise(directoryReader) {
    try {
        return await new Promise((resolve, reject) => {
            directoryReader.readEntries(resolve, reject);
        });
    } catch (err) {
        console.log(err);
    }
}