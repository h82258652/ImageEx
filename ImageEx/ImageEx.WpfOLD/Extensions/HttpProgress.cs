namespace Controls.Extensions
{
    public class HttpProgress
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

        //public double? Percent
        //{
        //    get
        //    {
        //        if (TotalBytesToReceive.HasValue)
        //        {
        //            return 100d * BytesReceived / TotalBytesToReceive;
        //        }
        //        return null;
        //    }
        //}

        public int? TotalBytesToReceive
        {
            get;
        }
    }
}