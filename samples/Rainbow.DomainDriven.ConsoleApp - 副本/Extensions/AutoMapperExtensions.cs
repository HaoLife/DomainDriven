namespace System
{
    public static class AutoMapperExtensions
    {
        public static TDestination Map<TDestination>(this object source)
        {
            return AutoMapper.Mapper.Map<TDestination>(source);
        }
    }
}