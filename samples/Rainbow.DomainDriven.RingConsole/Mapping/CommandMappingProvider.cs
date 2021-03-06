using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.RingConsole.Command;
using Rainbow.DomainDriven.RingConsole.Domain;

namespace Rainbow.DomainDriven.RingConsole.Mapping
{
    public class CommandMappingProvider : MemoryCommandMappingProvider
    {
        public override void OnConfiguring(ICommandMapper mapper)
        {
            mapper.Map<CreateUserCommand, User>(k => k.UserId);

            mapper.Map<ModifyUserNameCommand, User>(k => k.UserId);
        }
    }
}