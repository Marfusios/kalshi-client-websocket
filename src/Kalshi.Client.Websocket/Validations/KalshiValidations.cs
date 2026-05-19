using System.Collections.Generic;
using System.Linq;
using Kalshi.Client.Websocket.Exceptions;

namespace Kalshi.Client.Websocket.Validations
{
    internal static class KalshiValidations
    {
        public static void ValidateInput<T>(T value, string name)
        {
            if (value == null)
            {
                throw new KalshiBadInputException($"Argument '{name}' is null");
            }
        }

        public static void ValidateNotEmpty(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new KalshiBadInputException($"Argument '{name}' is empty");
            }
        }

        public static string[] ValidateArray(IEnumerable<string> values, string name)
        {
            ValidateInput(values, name);

            var array = values.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            if (array.Length == 0)
            {
                throw new KalshiBadInputException($"Argument '{name}' must contain at least one value");
            }

            return array;
        }

        public static long[] ValidateArray(IEnumerable<long> values, string name)
        {
            ValidateInput(values, name);

            var array = values.Where(x => x > 0).ToArray();
            if (array.Length == 0)
            {
                throw new KalshiBadInputException($"Argument '{name}' must contain at least one positive value");
            }

            return array;
        }

        public static void ValidateOperation(string operation)
        {
            ValidateNotEmpty(operation, nameof(operation));

            if (operation != "subscribe" && operation != "unsubscribe")
            {
                throw new KalshiBadInputException("Operation must be 'subscribe' or 'unsubscribe'");
            }
        }
    }
}
