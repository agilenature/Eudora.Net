// PARAM0: signature (in C#, signature.Content)

{
    function SetSignature() {
        console.log("SetSignature running");
        var sigdiv = document.getElementById("EudoraEmailSignature");
        if (sigdiv == null) {
            console.log("sigdiv is null");
            sigdiv = document.createElement("div");
            sigdiv.setAttribute("Id", "EudoraEmailSignature");
        }
        sigdiv.innerHTML = "@PARAM0";
        console.log(sigdiv.innerHTML);
        console.log("@PARAM0");

        // Must position this before Reply or Forward div
        const quoteDiv = document.getElementById("EudoraQuoteContent");
        if (quoteDiv == null) {
            console.log("quoteDiv is null");
            document.body.appendChild(sigdiv);
        }
        else {
            console.log("quoteDiv is not null");
            document.body.insertBefore(sigdiv, quoteDiv);
        }
    }
    console.log("Calling SetSignature");
    SetSignature();
}