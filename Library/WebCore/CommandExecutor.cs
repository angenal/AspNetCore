using System.Diagnostics;

namespace WebCore
{
    /// <summary>
    /// Enumerate the various command executors that are available.
    /// </summary>
    public static class CommandExecutor
    {
        /// <summary>
        /// Gets a command executor that uses /bin/bash to execute commands.
        /// </summary>
        public static ICommandExecutor Bash { get; } = new BashCommandExecutor();
    }
    /// <summary>
    /// An implementation of <see cref="ICommandExecutor"/> that uses /bin/bash to execute commands.
    /// </summary>
    internal class BashCommandExecutor : CommandExecutorBase
    {
        /// <summary>
        /// Executes the specified command.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <returns>The command output.</returns>
        public override string Execute(string command)
        {
            return RunWithShell("/bin/bash", $"-c \"{command.Replace("\"", "\\\"")}\"").Trim('\r').Trim('\n').TrimEnd().TrimStart();
        }
    }
    /// <summary>
    /// A base implementation of <see cref="ICommandExecutor"/>.
    /// </summary>
    public abstract class CommandExecutorBase : ICommandExecutor
    {
        /// <summary>
        /// Executes the specified command.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <returns>The command output.</returns>
        public abstract string Execute(string command);

        /// <summary>
        /// Runs the specified command with the specified shell.
        /// </summary>
        /// <param name="shell">The shell to use.</param>
        /// <param name="command">The command to run.</param>
        /// <returns>The output.</returns>
        protected string RunWithShell(string shell, string command)
        {
            var psi = new ProcessStartInfo();
            psi.FileName = shell;
            psi.Arguments = command;
            psi.RedirectStandardOutput = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;

            using (var process = Process.Start(psi))
            {
                process?.WaitForExit();
                return process?.StandardOutput.ReadToEnd();
            }
        }
    }
    /// <summary>
    /// Provides functionality to execute a command.
    /// </summary>
    public interface ICommandExecutor
    {
        /// <summary>
        /// Executes the specified command.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <returns>The command output.</returns>
        string Execute(string command);
    }
}
