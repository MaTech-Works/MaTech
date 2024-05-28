using System;

namespace Optional.Unsafe
{
    /// <summary>
    /// Thrown when assertion failed upon calling methods including AssertSome and AssertNone
    /// </summary>
    public class OptionAssertionException : Exception
    {
        internal OptionAssertionException()
            : base()
        {
        }

        internal OptionAssertionException(string message)
            : base(message)
        {
        }
    }
}
