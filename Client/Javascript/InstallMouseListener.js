//window.addEventListener(
//    "mouseup",
//    function (event) {

//        let x = event.clientX;
//        let y = event.clientY;
//        const element = document.elementFromPoint(x, y);

//        window.chrome.webview.postMessage({
//            msgid: "MouseClick",
//            tagName: element.tagName,
//            id: element.id,
//            innerHTML: element.innerHTML,
//            outerHTML: element.outerHTML,
//            style: element.getAttribute("style"),
//            x: x,
//            y: y
//        });
//    }
//);