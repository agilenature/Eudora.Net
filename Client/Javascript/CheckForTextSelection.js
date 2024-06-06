// Returns true if there's current a selection

{
    function CheckForSelection() {
        var sel = window.getSelection();
        var rng = sel.getRangeAt(0);
        if (!rng.collapsed) {
            return "true";
        }
        return "false";
    }
    CheckForSelection();
}