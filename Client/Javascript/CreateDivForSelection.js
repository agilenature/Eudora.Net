// Wraps the current selection in a div and assigns an ID
// PARAM0: the div's id (note: id allocation is handled by the WebView2Document)

{
    var sel = window.getSelection();
    if (sel.rangeCount) {
        {
            var range = sel.getRangeAt(0);
            var newNode = document.createElement('div');
            newNode.id = "@PARAM0";
            newNode.appendChild(range.extractContents());
            range.insertNode(newNode);
        }
    }
}