// PARAM0: The element tag (img, div, etc) to be created
// PARAM1: The (outer/full) HTML to be inserted

{
    function InsertElement() {
        var elm = document.createElement("@PARAM0");
        elm.outerHTML = "@PARAM1";
        document.body.appendChild(elm);
    }
    InsertElement();    
}
