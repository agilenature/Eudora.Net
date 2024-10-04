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
        /// 0 will get the caller of GetCallAtOffset.
        /// 1 will get the function in which the Exception or Error was handled
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private static string GetCallAtOffset(string stack, int index)
        {
            string line = string.Empty;
            try
            {
                string[] lines = stack.Split($@"{Environment.NewLine}");
                line = lines[^index];
            }
            catch
            {

            }
            return line;
        }

        public static void Exception(Exception ex)
        {
            string stack = Environment.StackTrace;
            string line = GetCallAtOffset(stack, 1);
            Logger.Error(line);
            Logger.Exception(ex);
        }

        public static void Error(Exception ex)
        {
            string stack = Environment.StackTrace;
            string line = GetCallAtOffset(stack, 1);
            Logger.Error($"{line} : {ex.Message}");
        }

        public static void Warning(Exception ex)
        {
            string stack = Environment.StackTrace;
            string line = GetCallAtOffset(stack, 1);
            Logger.Warning($"{line} : {ex.Message}");
        }
    }
}
