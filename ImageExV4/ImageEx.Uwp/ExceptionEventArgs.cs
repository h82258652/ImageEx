using System;

namespace Controls
{
    public class ExceptionEventArgs : EventArgs
    {
        internal ExceptionEventArgs(string errorMessage)
        {
            if (errorMessage == null)
            {
                throw new ArgumentNullException(nameof(errorMessage));
            }

            ErrorMessage = errorMessage;
        }

        public string ErrorMessage
        {
            get;
        }
    }
}