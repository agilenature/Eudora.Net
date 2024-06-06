// PARAM0: element ID
// PARAM1: element new innerHTML

{
    function SetElementInnerHTML() {
        let elem = document.getElementById("@PARAM0");
        if (elem != null) {
            elem.innerHTML = "@PARAM1";
        }
        else {
            console.error("Element of id {@PARAM0} not found");
        }
    }
    SetElementInnerHTML();
}
