using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eudora.Net.Core
{
    public class EmailEnums
    {
        public enum MessageStatus
        {
            Draft,
            Sealed,
            Default
        }

        public enum eReadStatus
        {
            Read,
            Unread
        }

        public enum eSendStatus
        {
            Sent,
            Unsent
        }

        public enum MessageOrigin
        {
            Outgoing,
            IncomingTo,
            IncomingCC,
            IncomingBCC,
            Default
        }

        public enum eMailPriority
        {
            Lowest,
            Low,
            Normal,
            High,
            Highest
        }
    }
}
