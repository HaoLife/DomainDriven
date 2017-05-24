using System;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.ConsoleApp.Command;
using Rainbow.DomainDriven.ConsoleApp.Domain;

namespace Rainbow.DomainDriven.ConsoleApp.Config
{
    public class CommandMappingProvider : ICommandMappingProvider
    {
        public void OnConfiguring(ICommandMapper mapper)
        {
            mapper.Map<ModifyUserNameCommand, User>(k => k.UserId);
        }
    }
}