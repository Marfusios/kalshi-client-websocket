using System;

namespace Kalshi.Client.Websocket.Exceptions
{
    /// <summary>
    /// Base exception for Kalshi client errors.
    /// </summary>
    public class KalshiException : Exception
    {
        /// <inheritdoc />
        public KalshiException()
        {
        }

        /// <inheritdoc />
        public KalshiException(string message)
            : base(message)
        {
        }

        /// <inheritdoc />
        public KalshiException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
