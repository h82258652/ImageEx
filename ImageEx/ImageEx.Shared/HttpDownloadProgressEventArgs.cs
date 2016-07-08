#if WINDOWS_UWP

using Windows.Web.Http;

#else

using Controls.Extensions;

#endif

namespace Controls
{
    public class HttpDownloadProgressEventArgs
    {
        public HttpDownloadProgressEventArgs(HttpProgress progress)
        {
            Progress = progress;
        }

        public HttpProgress Progress
        {
            get;
        }
    }
}