
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
    const searchPopup = document.getElementById("search-popup");
    if(searchPopup == null){
        return;
    }

    const outsideClick = !searchPopup.contains(e.target);
    if(outsideClick && e.target.id != "search-input"){
        _dotNetHelper.invokeMethodAsync("CloseSearchPopup");
    }

}