using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace WebCore
{
    /// <summary>
    /// Detecting process exit from the application.
    /// http://stackoverflow.com/questions/6783561/nullreferenceexception-with-no-stack-trace-when-hooking-setconsolectrlhandler
    /// </summary>
    public sealed class Exit
    {
        /// <summary>
        /// Sets actions detecting process exit.
        /// </summary>
        public static ICollection<Action> Actions = new List<Action>();

        /// <summary>
        /// Task execution timeout seconds.
        /// </summary>
        public static int ActionTimeoutSeconds = 2;

        /// <summary>
        /// Detecting process exit from the application.
        /// </summary>
        /// <param name="actions"></param>
        public static void Wait(params Action[] actions)
        {
            foreach (Action action in actions) Actions.Add(action);
            HandlerRoutine consoleCtrlHandler = ConsoleCtrlHandler;
            SetConsoleCtrlHandler(consoleCtrlHandler, true);
            Event.Wait();
            SetConsoleCtrlHandler(null, false);
            Return();
        }

        /// <summary>
        /// Execute all actions and return.
        /// </summary>
        public static void Return()
        {
            if (ActionTimeoutSeconds == 0) return;
            var tasks = Actions.ConvertToTask(TimeSpan.FromSeconds(ActionTimeoutSeconds));
            ActionTimeoutSeconds = 0;
            Task.WaitAll(tasks);
        }

        private static bool ConsoleCtrlHandler(int ctrltype)
        {
            if (ctrltype != 0 /* CTRL+C */) return false;
            Event.Set();
            return true;
        }

        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(HandlerRoutine handler, bool add);

        private static readonly ManualResetEventSlim Event = new ManualResetEventSlim();

        private delegate bool HandlerRoutine(int ctrlType);
    }
}
