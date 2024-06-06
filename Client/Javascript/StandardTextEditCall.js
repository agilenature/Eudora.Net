// ...because Chrome and therefore WebView2 doesn't expose the clipboard
// However, the WebView2 control does fire the cut, copy, and paste events
// in response to the user pressing the appropriate keys.


function StandardTextEditCall() {

    let command = "@PARAM0";
    let kbEvent;

    if (command == "paste") {
        Navigator.clipboard.readText().then(function (text) {
            window.getSelection().getRangeAt(0).insertNode(document.createTextNode(text));
        });
    }
    else if (command == "copy") {
        Navigator.clipboard.writeText(window.getSelection().toString());
    }
    else if (command == "cut") {
        Navigator.clipboard.writeText(window.getSelection().toString());
        window.getSelection().deleteFromDocument();
    }

    //if (command == "cut") {
    //    kbEvent = new KeyboardEvent("keyup", {
    //        key: "X",
    //        code: "KeyX",
    //        ctrlKey: true,
    //        metaKey: false,
    //        altKey: false,
    //        shiftKey: false,
    //        repeat: false,
    //        location: 0,
    //        charCode: 0,
    //        keyCode: 88,
    //        which: 88
    //    });
    //}
    //else if (command == "copy") {
    //    kbEvent = new KeyboardEvent("keyup", {
    //        key: "C",
    //        code: "KeyC",
    //        ctrlKey: true,
    //        metaKey: false,
    //        altKey: false,
    //        shiftKey: false,
    //        repeat: false,
    //        location: 0,
    //        charCode: 0,
    //        keyCode: 67,
    //        which: 67
    //    });
    //}
    //else if (command == "paste") {
    //    kbEvent = new KeyboardEvent("keyup", {
    //        key: "V",
    //        code: "KeyV",
    //        ctrlKey: true,
    //        metaKey: false,
    //        altKey: false,
    //        shiftKey: false,
    //        repeat: false,
    //        location: 0,
    //        charCode: 0,
    //        keyCode: 86,
    //        which: 86
    //    });
    //}

    //document.dispatchEvent(kbEvent);
}