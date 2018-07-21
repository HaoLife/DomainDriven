using AutoMapper;
using Rainbow.DomainDriven.RingConsole.Domain;
using Rainbow.DomainDriven.RingConsole.Info;

namespace Rainbow.DomainDriven.RingConsole
{
    public class AutoMapperInitializer
    {
        public static void Init()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<User, UserInfo>();
            });
        }
    }
}