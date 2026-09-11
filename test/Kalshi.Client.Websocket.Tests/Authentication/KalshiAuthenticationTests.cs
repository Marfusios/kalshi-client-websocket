using System;
using System.Security.Cryptography;
using System.Text;
using Kalshi.Client.Websocket.Authentication;
using Kalshi.Client.Websocket.Exceptions;
using Xunit;

namespace Kalshi.Client.Websocket.Tests.Authentication
{
    public class KalshiAuthenticationTests
    {
        [Fact]
        [Trait("Cat", "Base")]
        public void CreateWebsocketHeaders_WhenRsaKeyProvided_CreatesVerifiableSignature()
        {
            using var rsa = RSA.Create(2048);
            var auth = KalshiAuthentication.FromRsaPrivateKey(
                "key-id",
                rsa,
                () => DateTimeOffset.FromUnixTimeMilliseconds(1000));

            var headers = auth.CreateWebsocketHeaders();

            Assert.Equal("key-id", headers["KALSHI-ACCESS-KEY"]);
            Assert.Equal("1000", headers["KALSHI-ACCESS-TIMESTAMP"]);
            var payload = Encoding.UTF8.GetBytes("1000GET/trade-api/ws/v2");
            var signature = Convert.FromBase64String(headers["KALSHI-ACCESS-SIGNATURE"]);
            Assert.True(rsa.VerifyData(payload, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pss));
        }

        [Fact]
        [Trait("Cat", "Base")]
        public void FromPemPrivateKey_WhenPkcs8PemProvided_CreatesVerifiableSignature()
        {
            using var rsa = RSA.Create(2048);
            var pem = rsa.ExportPkcs8PrivateKeyPem();

            var auth = KalshiAuthentication.FromPemPrivateKey(
                "key-id",
                pem,
                () => DateTimeOffset.FromUnixTimeMilliseconds(2000));

            var headers = auth.CreateWebsocketHeaders();

            Assert.Equal("key-id", headers["KALSHI-ACCESS-KEY"]);
            Assert.Equal("2000", headers["KALSHI-ACCESS-TIMESTAMP"]);
            var payload = Encoding.UTF8.GetBytes("2000GET/trade-api/ws/v2");
            var signature = Convert.FromBase64String(headers["KALSHI-ACCESS-SIGNATURE"]);
            Assert.True(rsa.VerifyData(payload, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pss));
        }

        [Fact]
        [Trait("Cat", "Base")]
        public void FromPemPrivateKey_WhenPkcs1PemWithWindowsLineEndingsProvided_CreatesVerifiableSignature()
        {
            using var rsa = RSA.Create(2048);
            var pem = rsa.ExportRSAPrivateKeyPem().Replace("\n", "\r\n");

            var auth = KalshiAuthentication.FromPemPrivateKey(
                "key-id",
                pem,
                () => DateTimeOffset.FromUnixTimeMilliseconds(3000));

            var headers = auth.CreateWebsocketHeaders();

            var payload = Encoding.UTF8.GetBytes("3000GET/trade-api/ws/v2");
            var signature = Convert.FromBase64String(headers["KALSHI-ACCESS-SIGNATURE"]);
            Assert.True(rsa.VerifyData(payload, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pss));
        }

        [Fact]
        [Trait("Cat", "Base")]
        public void FromPemPrivateKey_WhenPemIsNotAPrivateKey_ThrowsBadInput()
        {
            var pem = "-----BEGIN CERTIFICATE-----\nAAAA\n-----END CERTIFICATE-----";

            Assert.Throws<KalshiBadInputException>(() => KalshiAuthentication.FromPemPrivateKey("key-id", pem));
        }

        [Fact]
        [Trait("Cat", "Base")]
        public void FromPemPrivateKey_WhenPemHasNoMarkers_ThrowsBadInput()
        {
            Assert.Throws<KalshiBadInputException>(() => KalshiAuthentication.FromPemPrivateKey("key-id", "not a pem"));
        }
    }
}
