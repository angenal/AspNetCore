using FASTER.core;
using System.Diagnostics;

namespace WebCore.Cache
{
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
