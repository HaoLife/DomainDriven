namespace Rainbow.DomainDriven.Domain
{
    public static class AggregateRootExtensions
    {

        public static string GetIndex(this IAggregateRoot root, params string[] values)
        {
            return $"Index:{root.GetType().Name}:{string.Join(":", values)}";
        }
    }
}