using Eudora.Net.Core;
using System.IO;
using System.Reflection;

namespace Eudora.Net.Javascript
{
    internal static class JsDepot
    {
        ///////////////////////////////////////////////////////////
        #region Script resource paths
        /////////////////////////////

        private static readonly string path_DesignMode = "Eudora.Net.Javascript.DesignMode.js";

        private static readonly string path_SetInitialCursorPos = "Eudora.Net.Javascript.SetInitialCursorPosition.js";

        private static readonly string path_InsertElement = "Eudora.Net.Javascript.InsertElement.js";
        private static readonly string path_InsertInlineImage = "Eudora.Net.Javascript.InsertInlineImage.js";
        private static readonly string path_CreateSpanForSelection = "Eudora.Net.Javascript.CreateSpanForSelection.js";
        private static readonly string path_CreateDivForSelection = "Eudora.Net.Javascript.CreateDivForSelection.js";
        private static readonly string path_CheckForTextSelection = "Eudora.Net.Javascript.CheckForTextSelection.js";

        private static readonly string path_BodyObserver = "Eudora.Net.Javascript.BodyObserver.js";
        private static readonly string path_CursorMovedListener = "Eudora.Net.Javascript.CursorMovedListener.js";
        private static readonly string path_MouseListener = "Eudora.Net.Javascript.InstallMouseListener.js";
        private static readonly string path_KeyboardListener = "Eudora.Net.Javascript.InstallKeyboardListener.js";
        private static readonly string path_EnterKeyOverride = "Eudora.Net.Javascript.InstallEnterKeyOverride.js";

        private static readonly string path_ResampleBody = "Eudora.Net.Javascript.ResampleBody.js";
        private static readonly string path_GetBodyInnerHTML = "Eudora.Net.Javascript.GetBodyInnerHTML.js";
        private static readonly string path_SetBodyInnerHTML = "Eudora.Net.Javascript.SetBodyInnerHTML.js";
        private static readonly string path_GetBodyOuterHTML = "Eudora.Net.Javascript.GetBodyOuterHTML.js";
        private static readonly string path_SetBodyOuterHTML = "Eudora.Net.Javascript.SetBodyOuterHTML.js";
        private static readonly string path_GetBodyAttribute = "Eudora.Net.Javascript.GetBodyAttribute.js";
        private static readonly string path_SetBodyAttribute = "Eudora.Net.Javascript.SetBodyAttribute.js";
        private static readonly string path_SetBodyStyleProperty = "Eudora.Net.Javascript.SetBodyStyleProperty.js";

        private static readonly string path_SetElementId = "Eudora.Net.Javascript.SetElementId.js";
        private static readonly string path_GetElementInnerHTML = "Eudora.Net.Javascript.GetElementInnerHTML.js";
        private static readonly string path_SetElementInnerHTML = "Eudora.Net.Javascript.SetElementInnerHTML.js";
        private static readonly string path_GetElementOuterHTML = "Eudora.Net.Javascript.GetElementOuterHTML.js";
        private static readonly string path_SetElementOuterHTML = "Eudora.Net.Javascript.SetElementOuterHTML.js";
        private static readonly string path_GetElementAttribute = "Eudora.Net.Javascript.GetElementAttribute.js";
        private static readonly string path_SetElementAttribute = "Eudora.Net.Javascript.SetElementAttribute.js";
        private static readonly string path_SetElementStyleProperty = "Eudora.Net.Javascript.SetElementStyleProperty.js";

        private static readonly string path_SetActiveElementAttribute = "Eudora.Net.Javascript.SetActiveElementAttribute.js";

        private static readonly string path_SetEmailSignature = "Eudora.Net.Javascript.SetEmailSignature.js";

        private static readonly string path_StandardTextEditCall = "Eudora.Net.Javascript.StandardTextEditCall.js";

        /////////////////////////////
        #endregion Script resource paths
        ///////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////
        #region Scripts by name
        /////////////////////////////

        public static string js_DesignMode { get; private set; } = string.Empty;

        public static string js_SetInitialCursorPos { get; private set; } = string.Empty;

        public static string js_InsertElement { get; private set; } = string.Empty;
        public static string js_InsertInlineImage { get; private set; } = string.Empty;
        public static string js_CreateSpanForSelection { get; private set; } = string.Empty;
        public static string js_CreateDivForSelection { get; private set; } = string.Empty;
        public static string js_CheckForTextSelection { get; private set; } = string.Empty;

        public static string js_InstallBodyObserver { get; private set; } = string.Empty;
        public static string js_InstallCursorMoveListener { get; private set; } = string.Empty;
        public static string js_InstallMouseListener { get; private set; } = string.Empty;
        public static string js_InstallKeyboardListener { get; private set; } = string.Empty;
        public static string js_InstallEnterKeyOverride { get; private set; } = string.Empty;

        public static string js_ResampleBody { get; private set; } = string.Empty;
        public static string js_GetBodyInnerHTML { get; private set; } = string.Empty;
        public static string js_SetBodyInnerHTML { get; private set; } = string.Empty;
        public static string js_GetBodyOuterHTML { get; private set; } = string.Empty;
        public static string js_SetBodyOuterHTML { get; private set; } = string.Empty;
        public static string js_GetBodyAttribute { get; private set; } = string.Empty;
        public static string js_SetBodyAttribute { get; private set; } = string.Empty;
        public static string js_SetBodyStyleProperty { get; private set; } = string.Empty;

        public static string js_SetElementId { get; private set; } = string.Empty;
        public static string js_GetElementInnerHTML { get; private set; } = string.Empty;
        public static string js_SetElementInnerHTML { get; private set; } = string.Empty;
        public static string js_GetElementOuterHTML { get; private set; } = string.Empty;
        public static string js_SetElementOuterHTML { get; private set; } = string.Empty;
        public static string js_GetElementAttribute { get; private set; } = string.Empty;
        public static string js_SetElementAttribute { get; private set; } = string.Empty;
        public static string js_SetElementStyleProperty { get; private set; } = string.Empty;

        public static string js_SetActiveElementAttribute { get; private set; } = string.Empty;

        public static string js_SetEmailSignature { get; private set; } = string.Empty;

        public static string js_StandardTextEditCall { get; private set; } = string.Empty;

        /////////////////////////////
        #endregion Scripts by name
        ///////////////////////////////////////////////////////////



        static JsDepot()
        {
            js_DesignMode = LoadScript(path_DesignMode);

            js_SetInitialCursorPos = LoadScript(path_SetInitialCursorPos);

            js_InsertElement = LoadScript(path_InsertElement);
            js_InsertInlineImage = LoadScript(path_InsertInlineImage);
            js_CreateSpanForSelection = LoadScript(path_CreateSpanForSelection);
            js_CreateDivForSelection = LoadScript(path_CreateDivForSelection);
            js_CheckForTextSelection = LoadScript(path_CheckForTextSelection);

            js_InstallBodyObserver = LoadScript(path_BodyObserver);
            js_InstallCursorMoveListener = LoadScript(path_CursorMovedListener);
            js_InstallMouseListener = LoadScript(path_MouseListener);
            js_InstallKeyboardListener = LoadScript(path_KeyboardListener);
            js_InstallEnterKeyOverride = LoadScript(path_EnterKeyOverride);
            
            js_ResampleBody = LoadScript(path_ResampleBody);

            js_GetBodyInnerHTML = LoadScript(path_GetBodyInnerHTML);
            js_GetBodyOuterHTML = LoadScript(path_GetBodyOuterHTML);
            js_GetBodyAttribute = LoadScript(path_GetBodyAttribute);

            js_SetBodyInnerHTML = LoadScript(path_SetBodyInnerHTML);
            js_SetBodyOuterHTML = LoadScript(path_SetBodyOuterHTML);
            js_SetBodyAttribute = LoadScript(path_SetBodyAttribute);
            js_SetBodyStyleProperty = LoadScript(path_SetBodyStyleProperty);

            js_SetElementId = LoadScript(path_SetElementId);

            js_GetElementInnerHTML = LoadScript(path_GetElementInnerHTML);
            js_GetElementOuterHTML = LoadScript(path_GetElementOuterHTML);
            js_GetElementAttribute = LoadScript(path_GetElementAttribute);

            js_SetElementInnerHTML = LoadScript(path_SetElementInnerHTML);
            js_SetElementOuterHTML = LoadScript(path_SetElementOuterHTML);
            js_SetElementAttribute = LoadScript(path_SetElementAttribute);
            js_SetElementStyleProperty = LoadScript(path_SetElementStyleProperty);

            js_SetActiveElementAttribute = LoadScript(path_SetActiveElementAttribute);

            js_SetEmailSignature = LoadScript(path_SetEmailSignature);

            js_StandardTextEditCall = LoadScript(path_StandardTextEditCall);
        }

        
        static string LoadScript(string jsName)
        {
            string script = string.Empty;
            try
            {
                using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(jsName))
                {
                    if (resource is not null)
                    {
                        using (var stream = new StreamReader(resource))
                        {
                            script = stream.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return script;
        }
    }
}
