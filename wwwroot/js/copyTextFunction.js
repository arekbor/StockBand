function copyText(elementToCopy, elementInfo) {
    console.log(elementToCopy);
    elementToCopy.select();
    elementToCopy.setSelectionRange(0, 99999);
    navigator.clipboard.writeText(elementToCopy.value);
    elementInfo.style.display = "block";
    setTimeout(function () {
        elementInfo.style = "display:none";
    }, 3500);
}
