
let _dotNetHelper;

export function initialize(dotNetHelper){
    
    _dotNetHelper = dotNetHelper;
    document.addEventListener("click", detectClick);

    return {
        dispose: () => {
            document.removeEventListener("click", detectClick);
        }
    }
}

function detectClick(e){
    const helpPopup = document.getElementById("help-popup");
    if(helpPopup == null){
        return;
    }
    
    const outsideClick = !helpPopup.contains(e.target);
    if(outsideClick){
        _dotNetHelper.invokeMethodAsync("CloseHelpPopup");
    }
        
}