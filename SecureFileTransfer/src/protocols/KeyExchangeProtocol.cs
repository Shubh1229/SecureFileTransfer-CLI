using System.Net.Sockets;
using System.Text.Json;
using SecureFileTransfer.src.data_structures;
using SecureFileTransfer.src.helper;
using SecureFileTransfer.src.logging;
using SecureFileTransfer.src.security;

namespace SecureFileTransfer.src.protocols
{
    public static class KeyExchangeProtocol
    {
        public static SessionKeyModel RunClient(NetworkStream stream)
        {
            using var clientKeyPair = KeyExchangeService.CreateKeyPair();

            byte[] clientPublicKey = KeyExchangeService.ExportPublicKey(clientKeyPair);

            KeyExchangeModel clientKeyExchange = new()
            {
                PublicKeyBase64 = Convert.ToBase64String(clientPublicKey)
            };

            string clientJson = JsonSerializer.Serialize(clientKeyExchange);

            DebugLogger.Log("Client sending public key.");
            MessageHelper.SendMessage(stream, clientJson);

            DebugLogger.Log("Client waiting for host public key.");
            string? hostJson = MessageHelper.ReadMessage(stream);

            if (string.IsNullOrWhiteSpace(hostJson))
            {
                DebugLogger.Log("Client did not receive host public key.");
                return new SessionKeyModel();
            }

            KeyExchangeModel? hostKeyExchange =
                JsonSerializer.Deserialize<KeyExchangeModel>(hostJson);

            if (hostKeyExchange == null || string.IsNullOrWhiteSpace(hostKeyExchange.PublicKeyBase64))
            {
                DebugLogger.Log("Client failed to parse host public key.");
                return new SessionKeyModel();
            }

            byte[] hostPublicKey = Convert.FromBase64String(hostKeyExchange.PublicKeyBase64);
            byte[] sharedSecret = KeyExchangeService.DeriveSharedKey(clientKeyPair, hostPublicKey);
            byte[] aesKey = KeyExchangeService.DeriveAesKey(sharedSecret);

            DebugLogger.Log("Client session key established.");

            return new SessionKeyModel
            {
                Key = aesKey
            };
        }

        public static SessionKeyModel RunHost(NetworkStream stream)
        {
            using var hostKeyPair = KeyExchangeService.CreateKeyPair();

            DebugLogger.Log("Host waiting for client public key.");
            string? clientJson = MessageHelper.ReadMessage(stream);

            if (string.IsNullOrWhiteSpace(clientJson))
            {
                DebugLogger.Log("Host did not receive client public key.");
                return new SessionKeyModel();
            }

            KeyExchangeModel? clientKeyExchange =
                JsonSerializer.Deserialize<KeyExchangeModel>(clientJson);

            if (clientKeyExchange == null || string.IsNullOrWhiteSpace(clientKeyExchange.PublicKeyBase64))
            {
                DebugLogger.Log("Host failed to parse client public key.");
                return new SessionKeyModel();
            }

            byte[] clientPublicKey = Convert.FromBase64String(clientKeyExchange.PublicKeyBase64);

            byte[] hostPublicKey = KeyExchangeService.ExportPublicKey(hostKeyPair);

            KeyExchangeModel hostKeyExchange = new()
            {
                PublicKeyBase64 = Convert.ToBase64String(hostPublicKey)
            };

            string hostJson = JsonSerializer.Serialize(hostKeyExchange);

            DebugLogger.Log("Host sending public key.");
            MessageHelper.SendMessage(stream, hostJson);

            byte[] sharedSecret = KeyExchangeService.DeriveSharedKey(hostKeyPair, clientPublicKey);
            byte[] aesKey = KeyExchangeService.DeriveAesKey(sharedSecret);

            DebugLogger.Log("Host session key established.");

            return new SessionKeyModel
            {
                Key = aesKey
            };
        }
    }
}