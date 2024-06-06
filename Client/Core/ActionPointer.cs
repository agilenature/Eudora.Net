using System.Collections.Concurrent;

namespace Eudora.Net.Core
{
    public class ActionPointer<T>
    {
        public Action<T>? FunctionPtr { get; set; } = null;
        public T? Param { get; set; } = default;

        public ActionPointer(Action<T> action, T param)
        {
            FunctionPtr = action;
            Param = param;
        }

        public void Execute()
        {
            if(FunctionPtr is not null && Param is not null)
            {
                FunctionPtr(Param);
            }
        }
    }

    public class ActionPointerString
    {
        public Action<string>? FunctionPtr { get; set; } = null;
        public string Param { get; set; } = string.Empty;

        public ActionPointerString()
        { }

        public ActionPointerString(Action<string> action, string param)
        {
            FunctionPtr = action;
            Param = param;
        }

        public void Execute()
        {
            if (FunctionPtr is not null)
            {
                FunctionPtr(Param);
            }
        }
    }

    public class ActionPointerStringQueue
    {
        private static object Locker = new();
        private ConcurrentQueue<ActionPointerString> Actions = [];
        public delegate void Delegate_Notifier();
        public event Delegate_Notifier? NotifyAdd = null;


        public void Push(ActionPointerString action)
        {
            lock (Locker)
            {
                Actions.Enqueue(action);
                NotifyAdd?.Invoke();
            }
        }

        public ActionPointerString? Pop()
        {
            lock (Locker)
            {
                var action = new ActionPointerString();
                if (Actions.TryDequeue(out action))
                {
                    return action;
                }
                return null;
            }
        }
    }

}
