using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace NATS.Services
{
    /// <summary>
    /// Detecting Process Exit From Console Application
    /// http://stackoverflow.com/questions/6783561/nullreferenceexception-with-no-stack-trace-when-hooking-setconsolectrlhandler
    /// </summary>
    public sealed class Exit
    {
        private delegate bool HandlerRoutine(int ctrlType);

        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(HandlerRoutine handler, bool add);

        private static readonly ManualResetEventSlim Event = new ManualResetEventSlim();

        public static void Wait(Action action = null)
        {
            if (action != null) Action = action;
            HandlerRoutine consoleCtrlHandler = ConsoleCtrlHandler;
            SetConsoleCtrlHandler(consoleCtrlHandler, true);
            Event.Wait();
            SetConsoleCtrlHandler(null, false);
            Action?.Invoke();
        }

        private static bool ConsoleCtrlHandler(int ctrltype)
        {
            if (ctrltype != 0 /* CTRL+C */) return false;
            Event.Set();
            return true;
        }

        // An action detecting process exit.
        public static Action Action = null;
    }
}
