using System;

namespace Rainbow.DomainDriven
{
    public static class GuidExtensions
    {
        public static string ToShort(this Guid source)
        {
            return source.ToString("N");
        }
    }
}