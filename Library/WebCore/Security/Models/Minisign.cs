namespace WebCore.Security.Models
{
    public class MinisignSignature
    {
        public byte[] SignatureAlgorithm { get; set; }
        public byte[] KeyId { get; set; }
        public byte[] Signature { get; set; }
        public byte[] GlobalSignature { get; set; }
        public byte[] TrustedComment { get; set; }
    }
    public class MinisignKeyPair
    {
        public MinisignPublicKey MinisignPublicKey { get; set; }
        public MinisignPrivateKey MinisignPrivateKey { get; set; }
        public string MinisignPublicKeyFilePath { get; set; }
        public string MinisignPrivateKeyFilePath { get; set; }
    }
    public class MinisignPublicKey
    {
        public byte[] SignatureAlgorithm { get; set; }
        public byte[] KeyId { get; set; }
        public byte[] PublicKey { get; set; }
    }
    public class MinisignPrivateKey
    {
        public byte[] SignatureAlgorithm { get; set; }
        public byte[] KdfAlgorithm { get; set; }
        public byte[] ChecksumAlgorithm { get; set; }
        public byte[] KdfSalt { get; set; }
        public long KdfOpsLimit { get; set; }
        public long KdfMemLimit { get; set; }
        public byte[] KeyId { get; set; }
        public byte[] SecretKey { get; set; }
        public byte[] PublicKey { get; set; }
        public byte[] Checksum { get; set; }
    }
}
