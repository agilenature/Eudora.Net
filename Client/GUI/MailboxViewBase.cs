using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eudora.Net.GUI
{
    public class MailboxViewBase : ChildWindowBase
    {
        public DelegateCommand CommandDeleteMessage { get; set; }
        public DelegateCommand CommandNextMessage { get; set; }
        public DelegateCommand CommandPreviousMessage { get; set; }
        public DelegateCommand CommandReply { get; set; }
        public DelegateCommand CommandReplyAll { get; set; }
        public DelegateCommand CommandForward { get; set; }

        public MailboxViewBase()
        {
        }

    }
}
