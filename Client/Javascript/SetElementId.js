// Param0: new id
{
    function SetElementId() {
        var brect = window.getSelection().getRangeAt(0).getBoundingClientRect();
        let x = brect.left;
        let y = brect.top;

        const element = document.elementFromPoint(x, y);
        element.id = "@PARAM0";
    }
    SetElementId();
}