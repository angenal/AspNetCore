using System;
using System.Diagnostics;

namespace WebCore.Documents
{
    public class UploadProgress
    {
        public UploadProgress()
        {
            UploadType = UploadType.Regular;
            _sw = Stopwatch.StartNew();
        }

        private readonly Stopwatch _sw;

        public long UploadedInBytes { get; set; }

        public long TotalInBytes { get; set; }

        public UploadState UploadState { get; private set; }

        public UploadType UploadType { get; set; }

        public void ChangeState(UploadState newState)
        {
            UploadState = newState;
            if (newState == UploadState.Done)
                _sw.Stop();
        }

        public void SetTotal(long totalLength)
        {
            TotalInBytes = totalLength;
        }

        public void UpdateUploaded(long length)
        {
            UploadedInBytes += length;
        }

        public void ChangeType(UploadType newType)
        {
            UploadType = newType;
        }
    }

    public enum UploadState
    {
        PendingUpload,
        Uploading,
        PendingResponse,
        Done
    }

    public enum UploadType
    {
        Regular,
        Chunked
    }

}
