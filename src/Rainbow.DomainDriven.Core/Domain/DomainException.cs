using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rainbow.DomainDriven.Domain
{
    public class DomainException : Exception
    {
        public int Code { get; private set; }

        public DomainException(int code) : base()
        {
            this.Code = code;
        }
        public DomainException(int code, string message) : base(message)
        {
            this.Code = code;
        }
        public DomainException(int code, string message, Exception ex) : base(message, ex)
        {
            this.Code = code;
        }
    }
}
