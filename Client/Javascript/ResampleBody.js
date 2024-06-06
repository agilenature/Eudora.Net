{
    function ResampleBody() {
        window.chrome.webview.postMessage(
            {
                msgid: "BodyChanged",
                outerHTML: document.body.outerHTML,
                innerHTML: document.body.innerHTML,
                style: document.body.getAttribute("style")
            });
    }
    ResampleBody();
}