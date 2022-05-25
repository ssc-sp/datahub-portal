function isElectron() {
    // Renderer process
    if (typeof window !== 'undefined' && typeof window.process === 'object' && window.process.type === 'renderer') {
        return true;
    }

    // Main process
    if (typeof process !== 'undefined' && typeof process.versions === 'object' && !!process.versions.electron) {
        return true;
    }

    // Detect the user agent when the `nodeIntegration` option is set to false
    if (typeof navigator === 'object' && typeof navigator.userAgent === 'string' && navigator.userAgent.indexOf('Electron') >= 0) {
        return true;
    }

    return false;
}

export function promptForNewFolderName() {
    let message = "Please enter a name for the new folder";
    return window.prompt(message)
}

export function azSyncDown(sasToken) {
    let message = {
        sasToken: sasToken
    };

    let ipcRenderer = window.ipcRenderer;
    if(isElectron() && ipcRenderer) {
        console.log("IPC Renderer found, sending message to main process");
        ipcRenderer.on('azsync-reply', (event, arg) => {
            console.log(arg);
        });
        ipcRenderer.send('azsync-message', message);
    }
    else {
        console.log("IPC Renderer not found, cannot send message to main process");
        console.log(message);
    }
}