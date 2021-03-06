namespace KPK.SettingsManagement.Exceptions
{
    using System;

    /// <summary>
    /// Represents anything wrong with reading and writing setting files. Includes directory-related exceptions.
    /// </summary>
    public class SettingsFileAccessException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsFileAccessException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public SettingsFileAccessException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsFileAccessException"/> class with a specified error
        /// message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public SettingsFileAccessException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
