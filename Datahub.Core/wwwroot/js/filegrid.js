window.copyToClipboard = (elementId) => {
    var elToCopy = $(`#${elementId}`)[0];

    /* Select the text field */
    elToCopy.select();
    
    navigator.clipboard.writeText(elToCopy.value);
}

window.enableOnScroll = (elToScrollId, elToEnableId) => {
    var elToScroll = $(`#${elToScrollId}`)[0];
    if ((elToScroll.scrollTop + elToScroll.offsetHeight) >= elToScroll.scrollHeight){
        $(`#${elToEnableId}`)[0].disabled = false;
    }
};

window.SetFocusToElement = (elementid) => {
    $(`#${elementid}`).focus();
};

window.toggleMenu = (elementid, toggleclass='show', clientX, clientY) => {
    let menu = $(`#${elementid}`);
    
    if (clientX && clientY) {
        menu[0].style.top = `${clientY}px`;
        menu[0].style.left = `${clientX}px`;
    }
    menu.toggleClass(toggleclass);
};

window.toggleMenuWithBool = (elementid, isShow) => {
    let menu = $(`#${elementid}`);
    if (isShow == 'true' && !menu[0].classList.contains('show')) {
        menu.toggleClass('show');
    }
    else if (isShow == 'false') {
        menu.removeClass('show');
    }
};

window.onclick = (e) => {    
    $('.dropdown-content').removeClass('show');
   // $("div[id$='Div']").forEach(uncheckList);

    //$("div[id$='Div']").each(function (index, item) {
    //    if (item.classList.contains('selected-item')) {
    //        item.removeClass('selected-item');
    //        DotNet.invokeMethodAsync('Datahub.Portal', 'UnselectAll');
    //    }
    //});
}

window.openNav = () => {
    document.getElementById("myNav").style.width = "20%";
}

window.closeNav = () => {
    document.getElementById("myNav").style.width = "0%";
}

window.saveAsFile = (filename, bytesBase64) => {
    if (navigator.msSaveBlob) {
        //Download document in Edge
        var _blazorDownloadFileData = atob(bytesBase64);
        var _blazorDownloadFileBytes = new Uint8Array(_blazorDownloadFileData.length);
        for (var i = 0; i < _blazorDownloadFileData.length; i++) {
            {
                _blazorDownloadFileBytes[i] = _blazorDownloadFileData.charCodeAt(i);
            }
        }
        _blazorDownloadFileData = null;
        var _blazorDownloadFileBlob = new Blob([_blazorDownloadFileBytes.buffer], { type: "application/octet-stream" });
        _blazorDownloadFileBytes = null;
        navigator.msSaveBlob(_blazorDownloadFileBlob, filename);
        _blazorDownloadFileBlob = null;
    }
    else {
        //Download document in other browsers
        var link = document.createElement('a');
        link.download = filename;
        link.href = "data:application/octet-stream;base64," + bytesBase64;
        document.body.appendChild(link); // Needed for Firefox
        link.click();
        document.body.removeChild(link);
    }

}

function uncheckList(item, index) {
    if (item.classList.contains('selected-item')) {
        item.removeClass('selected-item');
        DotNet.invokeMethodAsync('Datahub.Portal', 'UnselectAll');
    }
}