using System;

namespace Controls
{
    public class ExceptionEventArgs : EventArgs
    {
        internal ExceptionEventArgs(Exception errorException)
        {
            ErrorException = errorException;
        }

        public Exception ErrorException
        {
            get;
        }
    }
}