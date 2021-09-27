namespace WebCore.Platform.Posix.macOS
{
    /// <summary>
    /// An component that gets the system drive serial number.
    /// </summary>
    public class SystemDriveSerialNumber
    {
        /// <summary>
        /// Command executor.
        /// </summary>
        private readonly ICommandExecutor _commandExecutor;

        /// <summary>
        /// Initializes a new instance class.
        /// </summary>
        public SystemDriveSerialNumber() : this(CommandExecutor.Bash) { }

        /// <summary>
        /// Initializes a new instance class.
        /// </summary>
        /// <param name="commandExecutor">The command executor to use.</param>
        internal SystemDriveSerialNumber(ICommandExecutor commandExecutor)
        {
            _commandExecutor = commandExecutor;
        }

        /// <summary>
        /// Gets the component value.
        /// </summary>
        /// <returns>The component value.</returns>
        public string GetValue()
        {
            var output = _commandExecutor.Execute("system_profiler SPSerialATADataType | sed -En 's/.*Serial Number: ([\\d\\w]*)//p'");

            return output;
        }
    }
}
