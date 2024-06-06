// To change a single style property of the target element
// PARAM0: id of element
// PARAM1: the CSS property - dash notiation is fine
// PARAM2: the new value of the property

{
    function SetElementStyleProperty() {
        let elm = document.getElementById("@PARAM0");
        if (elm != null) {
            elm.style["@PARAM1"] = "@PARAM2";
        }
    }
    SetElementStyleProperty();
}
