using System.Security.Cryptography;

namespace SecureFileTransfer.src.security
{
    public static class KeyExchangeService
    {
        public static ECDiffieHellman CreateKeyPair()
        {
            return ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
        }

        public static byte[] ExportPublicKey(ECDiffieHellman keyPair)
        {
            return keyPair.PublicKey.ExportSubjectPublicKeyInfo();
        }

        public static byte[] DeriveSharedKey(ECDiffieHellman keyPair, byte[] remotePublicKeyBytes)
        {
            using ECDiffieHellman remotePublicKey = ECDiffieHellman.Create();
            remotePublicKey.ImportSubjectPublicKeyInfo(remotePublicKeyBytes, out _);

            return keyPair.DeriveKeyMaterial(remotePublicKey.PublicKey);
        }

        public static byte[] DeriveAesKey(byte[] sharedSecret)
        {
            return SHA256.HashData(sharedSecret);
        }
    }
}