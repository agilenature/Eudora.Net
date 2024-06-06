// PARAM0: element ID

{
    function GetElementInnerHTML() {
        const elem = document.body.getElement("@PARAM0");
        if (elem != null) {
            return elem.innerHTML;
        }
        else {
            console.error("Element of id {@PARAM0} not found");
        }
    }
    GetElementInnerHTML();
}
