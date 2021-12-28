using FASTER.core;
using System.Diagnostics;
using System.Text;

namespace WebCore.Cache
{
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

    public class StringValue
    {
        public string Value { get; set; }
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

    public class StringContext { }

    public sealed class StringFunctions : FunctionsBase<StringKey, StringValue, StringInput, StringOutput, StringContext>
    {
        public override void InitialUpdater(ref StringKey key, ref StringInput input, ref StringValue value, ref StringOutput output) => value.Value = input.Value;
        public override void CopyUpdater(ref StringKey key, ref StringInput input, ref StringValue oldValue, ref StringValue newValue, ref StringOutput output) => newValue = oldValue;
        public override bool InPlaceUpdater(ref StringKey key, ref StringInput input, ref StringValue value, ref StringOutput output) { value.Value += input.Value; return true; }

        public override void SingleReader(ref StringKey key, ref StringInput input, ref StringValue value, ref StringOutput dst) { dst = new StringOutput { Value = value }; }
        public override void ConcurrentReader(ref StringKey key, ref StringInput input, ref StringValue value, ref StringOutput dst) { dst = new StringOutput { Value = value }; }

        public override void ReadCompletionCallback(ref StringKey key, ref StringInput input, ref StringOutput output, StringContext ctx, Status status)
        {
            Debug.WriteLine(output.Value.Value == key.Key ? "Success" : "Error");
        }
    }
}
