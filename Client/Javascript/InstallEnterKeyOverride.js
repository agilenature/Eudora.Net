{
    function InstallEnterHandler()
    {
        body.onkeydown = function (event) {
            if (event.key == 13) {
                event.preventDefault();

            }
        }
        body.onkeyup = function (event) {
            if (event.key == 13) {
                event.preventDefault();
                const selection = window.getSelection();
                const range = selection.getRangeAt(0);
                const br = document.createElement('br');
                range.insertNode(br);
            }
        }
        InstallEnterHandler();
    }
}