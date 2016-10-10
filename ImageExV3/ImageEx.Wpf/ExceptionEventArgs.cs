using System;

namespace Controls
{
    public class ExceptionEventArgs : EventArgs
    {
        internal ExceptionEventArgs(Exception errorException)
        {
            if (errorException == null)
            {
                throw new ArgumentNullException(nameof(errorException));
            }

            ErrorException = errorException;
        }

        public Exception ErrorException
        {
            get;
        }
    }
}