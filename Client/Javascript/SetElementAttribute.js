// PARAM0: element ID
// PARAM1: element attribute name
// PARAM2: element attribute value

{
    function SetElementAttribute() {
        let elem = document.getElementById("@PARAM0");
        if (elem != null) {
            elem.setAttribute("@PARAM1", "@PARAM2");
        }
        else {
            console.error("Element of id {@PARAM0} not found");
        }
    }
    SetElementAttribute();
}

