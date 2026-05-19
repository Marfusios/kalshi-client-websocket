namespace Kalshi.Client.Websocket.Exceptions
{
    /// <summary>
    /// Exception thrown when an invalid value is passed to the client.
    /// </summary>
    public class KalshiBadInputException : KalshiException
    {
        /// <inheritdoc />
        public KalshiBadInputException(string message)
            : base(message)
        {
        }
    }
}
