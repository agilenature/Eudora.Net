//window.onkeydown = function (event) {

//    var brect = window.getSelection().getRangeAt(0).getBoundingClientRect();
//    let x = brect.left;
//    let y = brect.top;

//    const element = document.elementFromPoint(x, y);

//    window.chrome.webview.postMessage({
//        msgid: "KeyDown",
//        key: event.keyCode,
//        tagName: element.tagName,
//        id: element.id,
//        innerHTML: element.innerHTML,
//        outerHTML: element.outerHTML,
//        style: element.getAttribute("style"),
//        x: x,
//        y: y
//    });
//};
