

const DatahubChannel = {
    Start: "azcopy-start",
    Progress: "azcopy-progress",
    Success: "azcopy-success",
    Error: "azcopy-error",
    Cancel: "azcopy-cancel",
    Prompt: "azcopy-prompt",
    ConflictResponse: "azcopy-conflict-response",
}

export function promptForFileUpload(){
    document.querySelector('.file-list .dropzone-container input').click();
}

export function promptForNewFolderName() {
    let message = "Please enter a name for the new folder";
    return window.prompt(message)
}


let count = 0;

export function azSyncDown(sasToken, dotNetHelper) {
    
    let ipcRenderer = window.ipcRenderer;
    if(isElectron() && ipcRenderer) {
        console.log("IPC Renderer found, sending message to main process");
        setupIpcMessageHandling(ipcRenderer, sasToken, dotNetHelper)
    }
    else {
        console.log("IPC Renderer not found, cannot send message to main process");
        
        dotNetHelper?.invokeMethodAsync("AzCopyProgress", count++ > 2 ? "Completed" : "InProgress", (Math.random() * 100) + "");
    }
}

function setupIpcMessageHandling(ipcRenderer, sasToken, dotNetHelper){
    let message = {
        sasToken: sasToken
    };
    
    ipcRenderer.on(DatahubChannel.Progress, (event, arg) => {
        console.log("*****************************");
        console.log("progress");
        
        let jobStatus = ((arg || {}).latestStatus || {}).JobStatus || "Preparing";
        let percentComplete = ((arg || {}).latestStatus || {}).PercentComplete || "0";
        console.log(`Status [${jobStatus}] ${percentComplete}%`);
        
        if(jobStatus === 'InProgress' && percentComplete + "" === "100") {
            percentComplete = 1;
        }

        dotNetHelper?.invokeMethodAsync("AzCopyProgress", jobStatus + "", percentComplete + "");
    });

    ipcRenderer.on(DatahubChannel.Prompt, (event, arg) => {
        console.log("*****************************");
        console.log("prompt");
        console.log(arg);
        let overwrite = window.confirm("Overwrite file?");

        let response = {
            arg: arg,
            overwrite: overwrite
        }

        ipcRenderer.send(DatahubChannel.ConflictResponse, response);
    });

    ipcRenderer.send(DatahubChannel.Start, message);
}


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