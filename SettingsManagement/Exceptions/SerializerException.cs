﻿namespace SettingsManagement.Exceptions
{
    using System;

    /// <summary>
    /// Represents any errors related to the serializer.
    /// </summary>
    public class SerializerException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerializerException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public SerializerException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializerException"/> class with a specified error
        /// message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public SerializerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
