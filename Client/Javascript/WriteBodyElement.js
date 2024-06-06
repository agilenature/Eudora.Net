// PARAM0: element tag/type
// PARAM1: element id
// PARAM2: element innerHTML

{
    function WriteBodyElement() {
        let el = document.body.getElementById("$PARAM1");
        if (el == null) {
            el = document.createElement("$PARAM0");
            document.body.appendChild(el);
            el.setAttribute("id", "$PARAM1");
        }
        el.innerHTML = "$PARAM2";
    }
    WriteBodyElement();
}