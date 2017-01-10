using EloBuddy; 
using LeagueSharp.Common; 
 namespace ElUtilitySuite.Logging
{
    /// <summary>
    ///     The entry type.
    /// </summary>
    internal enum LoggingEntryType
    {
        /// <summary>
        ///     A debug entry.
        /// </summary>
        Debug,

        /// <summary>
        ///     An error entry.
        /// </summary>
        Error,

        /// <summary>
        ///     A generic entry.
        /// </summary>
        Generic,

        /// <summary>
        ///     An info entry.
        /// </summary>
        Info,

        /// <summary>
        ///     A warning entry.
        /// </summary>
        Warning
    }
}