namespace WebCore.Platform.Posix.macOS
{
    /// <summary>
    /// An component that gets the platform serial number.
    /// </summary>
    public class PlatformSerialNumber
    {
        /// <summary>
        /// Command executor.
        /// </summary>
        private readonly ICommandExecutor _commandExecutor;

        /// <summary>
        /// Initializes a new instance class.
        /// </summary>
        public PlatformSerialNumber() : this(CommandExecutor.Bash) { }

        /// <summary>
        /// Initializes a new instance class.
        /// </summary>
        /// <param name="commandExecutor">The command executor to use.</param>
        internal PlatformSerialNumber(ICommandExecutor commandExecutor)
        {
            _commandExecutor = commandExecutor;
        }

        /// <summary>
        /// Gets the component value.
        /// </summary>
        /// <returns>The component value.</returns>
        public string GetValue()
        {
            var output = _commandExecutor.Execute("ioreg -l | grep IOPlatformSerialNumber | sed 's/.*= //' | sed 's/\"//g'");

            return output;
        }
    }
}
