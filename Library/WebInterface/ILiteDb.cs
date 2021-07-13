using LiteDB;

namespace WebInterface
{
    /// <summary>
    /// LiteDB database interface.
    /// </summary>
    public interface ILiteDb
    {
        /// <summary>
        /// Database already open and init (or create if doesn't exist)
        /// </summary>
        LiteDatabase LiteDatabase { get; }

        /// <summary>
        /// Open database (or create if doesn't exist)
        /// </summary>
        LiteDatabase Open();
        /// <summary>
        /// Open Memory database (or create if doesn't exist)
        /// </summary>
        LiteDatabase OpenMemory();
    }
}
