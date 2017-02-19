using System;

namespace Rainbow.DomainDriven.Mongo
{
    public class LockException : Exception
    {
        public LockException(string message) : base(message)
        {

        }
    }
}