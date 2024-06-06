// PARAM0: alt attribute value
// PARAM1: src attribute value

{
    function InsertInlineImage() {
        let img = document.createElement("img");
        img.setAttribute("alt", "@PARAM0");
        img.setAttribute("src", "@PARAM1");
        document.body.appendChild(img);
    }
    InsertInlineImage();
}