// Sets the value of the specified attribute
// PARAM0: attribute
// PARAM1: attribute value

{
    function setActiveElementAttribute() {
        let element = document.activeElement;
        element.setAttribute("@PARAM0", "@PARAM1");
    }
    setActiveElementAttribute();
}