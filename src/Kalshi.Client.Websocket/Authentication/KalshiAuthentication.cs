using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Kalshi.Client.Websocket.Exceptions;
using Kalshi.Client.Websocket.Validations;

namespace Kalshi.Client.Websocket.Authentication
{
    /// <summary>
    /// Creates Kalshi websocket authentication headers.
    /// </summary>
    public sealed class KalshiAuthentication
    {
        private const string PemBeginMarker = "-----BEGIN ";
        private const string PemEndMarker = "-----END ";
        private const string PemMarkerSuffix = "-----";

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
        /// Create authentication from a PEM encoded RSA private key (PKCS#8 "PRIVATE KEY" or PKCS#1 "RSA PRIVATE KEY").
        /// The imported key stays alive as long as the returned authentication instance is referenced.
        /// </summary>
        public static KalshiAuthentication FromPemPrivateKey(string apiKeyId, string privateKeyPem, Func<DateTimeOffset>? timestampProvider = null)
        {
            KalshiValidations.ValidateNotEmpty(privateKeyPem, nameof(privateKeyPem));

            var rsa = RSA.Create();
            try
            {
                ImportPem(rsa, privateKeyPem);
            }
            catch
            {
                rsa.Dispose();
                throw;
            }

            return FromRsaPrivateKey(apiKeyId, rsa, timestampProvider);
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

        private static void ImportPem(RSA rsa, string pem)
        {
            var label = ParsePem(pem, out var der);

#if NETSTANDARD2_0
            throw new PlatformNotSupportedException(
                "PEM private key import is not supported on netstandard2.0, use FromRsaPrivateKey with an already imported RSA instance.");
#else
            switch (label)
            {
                case "PRIVATE KEY":
                    rsa.ImportPkcs8PrivateKey(der, out _);
                    break;
                case "RSA PRIVATE KEY":
                    rsa.ImportRSAPrivateKey(der, out _);
                    break;
                case "ENCRYPTED PRIVATE KEY":
                    throw new KalshiBadInputException("Encrypted PEM private keys are not supported, decrypt the key first");
                default:
                    throw new KalshiBadInputException($"Unsupported PEM label '{label}', expected 'PRIVATE KEY' or 'RSA PRIVATE KEY'");
            }
#endif
        }

        private static string ParsePem(string pem, out byte[] der)
        {
            var beginIndex = pem.IndexOf(PemBeginMarker, StringComparison.Ordinal);
            if (beginIndex < 0)
            {
                throw new KalshiBadInputException("Private key is not in PEM format, missing '-----BEGIN' marker");
            }

            var labelStart = beginIndex + PemBeginMarker.Length;
            var labelEnd = pem.IndexOf(PemMarkerSuffix, labelStart, StringComparison.Ordinal);
            if (labelEnd < 0)
            {
                throw new KalshiBadInputException("Private key is not in PEM format, malformed '-----BEGIN' marker");
            }

            var label = pem.Substring(labelStart, labelEnd - labelStart).Trim();
            var bodyStart = labelEnd + PemMarkerSuffix.Length;
            var endIndex = pem.IndexOf(PemEndMarker + label, bodyStart, StringComparison.Ordinal);
            if (endIndex < 0)
            {
                throw new KalshiBadInputException($"Private key is not in PEM format, missing '-----END {label}' marker");
            }

            var body = new StringBuilder();
            for (var i = bodyStart; i < endIndex; i++)
            {
                var c = pem[i];
                if (!char.IsWhiteSpace(c))
                {
                    body.Append(c);
                }
            }

            try
            {
                der = Convert.FromBase64String(body.ToString());
            }
            catch (FormatException e)
            {
                throw new KalshiBadInputException($"Private key PEM body is not valid base64: {e.Message}");
            }

            return label;
        }

        private static string StripQuery(string path)
        {
            var queryIndex = path.IndexOf('?');
            return queryIndex < 0 ? path : path.Substring(0, queryIndex);
        }
    }
}
