using System;

namespace Es.Throttle
{
    public class CASException : Exception
    {
        public CASException(string message) : base(message)
        {
        }
    }
}