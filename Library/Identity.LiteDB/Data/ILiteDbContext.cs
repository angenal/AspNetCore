using LiteDB;

namespace Identity.LiteDB.Data
{
    public interface ILiteDbContext
    {
        /// <summary>
        /// Database already open (or create if doesn't exist)
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
