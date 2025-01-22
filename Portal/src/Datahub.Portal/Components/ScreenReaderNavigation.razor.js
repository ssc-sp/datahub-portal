export function getNavigationIds(idList) {
    var navigationIds = [];
    for (var i = 0; i < idList.length; i++) {
        var element = document.getElementById(idList[i]);
        if (element) {
            navigationIds.push(idList[i]);
        }
    }
    return navigationIds;
}