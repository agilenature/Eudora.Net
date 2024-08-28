using Eudora.Net.GUI;

namespace Eudora.Net.Core
{
    /// <summary>
    /// A roughly RAII approach to disabling the UX
    /// Example: using UXLocker lock = new();
    /// </summary>
    public class UXLocker : IDisposable
    {
        public UXLocker() 
        {
            MainWindow.Instance?.EnableUX(false);
        }

        void IDisposable.Dispose() 
        {
            MainWindow.Instance?.EnableUX(true);
        }
    }
}
