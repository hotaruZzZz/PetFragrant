
function displaySubMenu(li) {

    var subMenu = li.getElementsByTagName("div")[0]
    subMenu.style.display = "flex";
}

function hideSubMenu(li) {

    var subMenu = li.getElementsByTagName("div")[0];

    subMenu.style.display = "none";

}
