window.overrideHistory = (url) => {
    window.location.replace(url);
};

window.overrideUrl = (url) => {
    window.history.replaceState({}, "", url)
};