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

        /// <summary>
        /// Executes the specified command.
        /// </summary>
        /// <param name="fileName">executable file path</param>
        /// <param name="arguments">command arguments</param>
        /// <param name="standardOutput"></param>
        /// <param name="standardError"></param>
        /// <returns></returns>
        public static bool Execute(string fileName, string arguments, out string standardOutput, out string standardError)
        {
            Process process = new Process
            {
                StartInfo =
                {
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
            standardOutput = process.StandardOutput.ReadToEnd();
            standardError = process.StandardError.ReadToEnd();
            return process.ExitCode == 0;
        }
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
            using (Process process = Process.Start(new ProcessStartInfo
            {
                FileName = shell,
                Arguments = command,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            }))
            {
                process.WaitForExit();
                return process.StandardOutput.ReadToEnd();
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
