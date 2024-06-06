{
    var divElement = document.querySelector("body > div");
    if (divElement) {
        // Set focus on the div element
        divElement.focus();

        // Set the cursor position to the end of the div content
        var range = document.createRange();
        var sel = window.getSelection();
        range.setStart(divElement, divElement.childNodes.length);
        range.collapse(true);
        sel.removeAllRanges();
        sel.addRange(range);
    }
}