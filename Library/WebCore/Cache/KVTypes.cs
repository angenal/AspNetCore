using FASTER.core;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace WebCore.Cache
{
    public class Md5Key : IFasterEqualityComparer<Md5Key>
    {
        public byte[] Key { get; set; }

        public Md5Key()
        {
        }

        public Md5Key(byte[] key)
        {
            using (var h = MD5.Create()) Key = h.ComputeHash(key);
        }

        public Md5Key(string key)
        {
            using (var h = MD5.Create()) Key = h.ComputeHash(Encoding.UTF8.GetBytes(key));
        }

        public bool Equals(ref Md5Key k1, ref Md5Key k2)
        {
            return k1.Key.SequenceEqual(k2.Key);
        }

        public unsafe long GetHashCode64(ref Md5Key k)
        {
            fixed (byte* b = k.Key)
            {
                return Utility.HashBytes(b, k.Key.Length);
            }
        }
    }

    public class StringKey : IFasterEqualityComparer<StringKey>
    {
        public string Key { get; set; }

        public long GetHashCode64(ref StringKey key)
        {
            byte[] k = Encoding.UTF8.GetBytes(key.Key);
            return GetHashCode64(ref k);
        }

        unsafe long GetHashCode64(ref byte[] k)
        {
            fixed (byte* b = k)
            {
                return Utility.HashBytes(b, k.Length);
            }
        }

        public bool Equals(ref StringKey key1, ref StringKey key2)
        {
            return key1.Key.Equals(key2.Key);
        }
    }

    public class Md5KeySerializer : BinaryObjectSerializer<Md5Key>
    {
        public override void Serialize(ref Md5Key key)
        {
            writer.Write(key.Key);
        }

        public override void Deserialize(out Md5Key key)
        {
            key = new Md5Key
            {
                Key = reader.ReadBytes(16)
            };
        }
    }

    public class StringKeySerializer : BinaryObjectSerializer<StringKey>
    {
        public override void Serialize(ref StringKey key)
        {
            writer.Write(key.Key);
        }

        public override void Deserialize(out StringKey key)
        {
            key = new StringKey
            {
                Key = reader.ReadString()
            };
        }
    }

    public class DataValue
    {
        public byte[] Value { get; set; }
    }

    public class StringValue
    {
        public string Value { get; set; }
    }

    public class DataValueSerializer : BinaryObjectSerializer<DataValue>
    {
        public override void Serialize(ref DataValue value)
        {
            writer.Write(BitConverter.GetBytes(value.Value.Length));
            writer.Write(value.Value);
        }

        public override void Deserialize(out DataValue value)
        {
            value = new DataValue
            {
                Value = reader.ReadBytes(BitConverter.ToInt32(reader.ReadBytes(sizeof(int)), 0))
            };
        }
    }

    public class StringValueSerializer : BinaryObjectSerializer<StringValue>
    {
        public override void Serialize(ref StringValue value)
        {
            writer.Write(value.Value);
        }

        public override void Deserialize(out StringValue value)
        {
            value = new StringValue
            {
                Value = reader.ReadString()
            };
        }
    }

    public class StringInput
    {
        public string Value { get; set; }
    }

    public class StringOutput
    {
        public StringValue Value { get; set; }
    }

    // Empty String Context
    //public class StringContext { }
}
