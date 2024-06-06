using System.Windows.Input;

namespace Eudora.Net.Core
{
    public static class SendKeys
    {
        public static void SendCtrlCombo(Key key)
        {
            var keys = Key.LeftCtrl | key;
            InputManager.Current.ProcessInput(new KeyEventArgs(
                Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, keys)
            { 
                RoutedEvent = Keyboard.KeyDownEvent
            });
        }
    }
}
