using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Kalshi.Client.Websocket.Validations;

namespace Kalshi.Client.Websocket.Authentication
{
    /// <summary>
    /// Creates Kalshi websocket authentication headers.
    /// </summary>
    public sealed class KalshiAuthentication
    {
        private readonly Func<string, byte[]> _signer;
        private readonly Func<DateTimeOffset> _timestampProvider;

        /// <summary>
        /// Create authentication from an API key ID and a signature delegate.
        /// </summary>
        public KalshiAuthentication(string apiKeyId, Func<string, byte[]> signer, Func<DateTimeOffset>? timestampProvider = null)
        {
            KalshiValidations.ValidateNotEmpty(apiKeyId, nameof(apiKeyId));
            KalshiValidations.ValidateInput(signer, nameof(signer));

            ApiKeyId = apiKeyId;
            _signer = signer;
            _timestampProvider = timestampProvider ?? (() => DateTimeOffset.UtcNow);
        }

        /// <summary>
        /// API key ID.
        /// </summary>
        public string ApiKeyId { get; }

        /// <summary>
        /// Create authentication backed by an RSA private key.
        /// </summary>
        public static KalshiAuthentication FromRsaPrivateKey(string apiKeyId, RSA privateKey, Func<DateTimeOffset>? timestampProvider = null)
        {
            KalshiValidations.ValidateInput(privateKey, nameof(privateKey));

            return new KalshiAuthentication(
                apiKeyId,
                payload =>
                {
                    var bytes = Encoding.UTF8.GetBytes(payload);
                    return privateKey.SignData(bytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
                },
                timestampProvider);
        }

        /// <summary>
        /// Creates websocket request headers for the current timestamp.
        /// </summary>
        public IDictionary<string, string> CreateWebsocketHeaders()
        {
            return CreateHeaders("GET", KalshiValues.WebsocketRequestPath);
        }

        /// <summary>
        /// Creates signed API request headers.
        /// </summary>
        public IDictionary<string, string> CreateHeaders(string method, string path)
        {
            KalshiValidations.ValidateNotEmpty(method, nameof(method));
            KalshiValidations.ValidateNotEmpty(path, nameof(path));

            var timestamp = _timestampProvider().ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture);
            var pathWithoutQuery = StripQuery(path);
            var payload = timestamp + method.ToUpperInvariant() + pathWithoutQuery;
            var signature = Convert.ToBase64String(_signer(payload));

            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["KALSHI-ACCESS-KEY"] = ApiKeyId,
                ["KALSHI-ACCESS-SIGNATURE"] = signature,
                ["KALSHI-ACCESS-TIMESTAMP"] = timestamp
            };
        }

        private static string StripQuery(string path)
        {
            var queryIndex = path.IndexOf('?');
            return queryIndex < 0 ? path : path.Substring(0, queryIndex);
        }
    }
}
