// Wraps the current selection in a span and assigns an ID
// PARAM0: the span's id (note: id allocation is handled by the WebView2Document)

{
    var sel = window.getSelection();
    if (sel.rangeCount) {
        {
            var range = sel.getRangeAt(0);
            var newNode = document.createElement('span');
            newNode.id = "@PARAM0";
            newNode.appendChild(range.extractContents());
            range.insertNode(newNode);
        }
    }
}