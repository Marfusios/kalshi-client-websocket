using System;
using System.Security.Cryptography;
using System.Text;
using Kalshi.Client.Websocket.Authentication;
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
    }
}
