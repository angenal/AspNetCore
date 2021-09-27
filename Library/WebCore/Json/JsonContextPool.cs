namespace WebCore.Json
{
    public class JsonContextPool : JsonContextPoolBase<JsonOperationContext>
    {
        protected override JsonOperationContext CreateContext()
        {
            if (Platform.OS.Is32Bit)
                return new JsonOperationContext(4096, 16 * 1024, LowMemoryFlag);
                
            return new JsonOperationContext(32*1024, 16*1024, LowMemoryFlag);
        }
    }
}
