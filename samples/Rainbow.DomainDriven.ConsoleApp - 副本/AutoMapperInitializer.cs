using AutoMapper;
using Rainbow.DomainDriven.ConsoleApp.Domain;
using Rainbow.DomainDriven.ConsoleApp.Info;

namespace Rainbow.DomainDriven.ConsoleApp
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