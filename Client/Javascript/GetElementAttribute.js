// PARAM0: element ID
// PARAM1: element attribute name

{
    function GetElementAttribute() {
        const elem = document.getElementById("@PARAM0");
        if (elem != null) {
            return elem.getAttribute("@PARAM1");
        }
        else {
            console.error("Element of id {@PARAM0} not found");
        }
    }
    GetElementAttribute();
}
