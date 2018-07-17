using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Domain
{
    public class DomainException : Exception
    {
        public DomainCode Code { get; private set; }

        public DomainException(DomainCode code) : base()
        {
            this.Code = code;
        }
        public DomainException(DomainCode code, string message) : base(message)
        {
            this.Code = code;
        }
        public DomainException(DomainCode code, string message, Exception ex) : base(message, ex)
        {
            this.Code = code;
        }
    }
}
