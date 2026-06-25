using System;

namespace medicalapp.Exceptions
{
    /// <summary>
    /// Custom exception for application-specific validation and logic errors.
    /// </summary>
    public class AppException : Exception
    {
        public AppException() : base() {}

        public AppException(string message) : base(message) {}

        public AppException(string message, Exception innerException) : base(message, innerException) {}
    }
}
