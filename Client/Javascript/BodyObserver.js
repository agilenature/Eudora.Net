function callbackBodyChanged(mutations)
{
    window.chrome.webview.postMessage(
        {
            msgid: "BodyChanged",
            outerHTML: document.body.outerHTML,
            innerHTML: document.body.innerHTML,
            style: document.body.getAttribute("style")
        });
}
let bodyChangedObserver = new MutationObserver(callbackBodyChanged);
bodyChangedObserver.observe(document.body, { childList: true, attributes: true, subtree: true });


function handleSelectionChange(event) {
    //console.log(event.type, window.getSelection().getRangeAt(0));
    var brect = window.getSelection().getRangeAt(0).getBoundingClientRect();
    let x = brect.left;
    let y = brect.top;
    const pos = { x, y };
    
    const element = document.elementFromPoint(x, y);

    window.chrome.webview.postMessage({
        msgid: "SelectionChange",
        tagName: element.tagName,
        id: element.id,
        innerHTML: element.innerHTML,
        outerHTML: element.outerHTML,
        style: element.getAttribute("style"),
        position: pos
    });
}

document.addEventListener("selectionchange", handleSelectionChange);