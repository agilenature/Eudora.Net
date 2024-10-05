using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eudora.Net.Core
{
    internal class FaultReporter
    {
        /// <summary>
        /// Offset is from the end of the stack, not the beginning.
        /// offset = 0: the last thing on the stack
        /// offset = 1: stack.last - 1
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        private static string GetCallAtOffset(string stack, int offset = 2)
        {
            string line = string.Empty;
            try
            {
                string[] lines = stack.Split($@"{Environment.NewLine}");
                line = lines[^offset];
            }
            catch
            {

            }
            return line;
        }

        public static void Exception(Exception ex)
        {
            string stack = Environment.StackTrace;
            string line = GetCallAtOffset(stack);
            Logger.Error(line);
            Logger.Exception(ex);
        }

        public static void Error(Exception ex)
        {
            string stack = Environment.StackTrace;
            string line = GetCallAtOffset(stack);
            Logger.Error($"{line} : {ex.Message}");
        }

        public static void Warning(Exception ex)
        {
            string stack = Environment.StackTrace;
            string line = GetCallAtOffset(stack);
            Logger.Warning($"{line} : {ex.Message}");
        }
    }
}
