// PARAM0: element ID

{
    function GetElementOuterHTML() {
        const elem = document.body.getElement("@PARAM0");
        if (elem != null) {
            return elem.outerHTML;
        }
        else {
            console.error("Element of id {@PARAM0} not found");
        }
    }
    GetElementOuterHTML();
}
