namespace Controls.Extensions
{
    public struct HttpProgress
    {
        internal HttpProgress(int bytesReceived, int? totalBytesToReceive)
        {
            BytesReceived = bytesReceived;
            TotalBytesToReceive = totalBytesToReceive;
        }

        public int BytesReceived
        {
            get;
        }

        public int? TotalBytesToReceive
        {
            get;
        }
    }
}