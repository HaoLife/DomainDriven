using Rainbow.DomainDriven.Domain;
using Rainbow.DomainDriven.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rainbow.DomainDriven.RingConsole.Event.UserEvent;

namespace Rainbow.DomainDriven.RingConsole.Domain
{
    public class User : AggregateRoot
        , IEventHandler<CreatedEvent>
        , IEventHandler<ModifyNamedEvent>
    {
        public User()
        {

        }
        public User(Guid id, string name, int sex)
        {
            this.RaiseEvent(new CreatedEvent() { AggregateRootId = id, Name = name, Sex = sex });
        }

        public string Name { get; protected set; }
        public int Sex { get; protected set; }
        public DateTime CreateTime { get; protected set; }
        public DateTime LastUpdateTime { get; protected set; }


        public void ModifyName(string name)
        {
            this.RaiseEvent(new ModifyNamedEvent() { Name = name });
        }
        public void ModifySex(int sex)
        {
            throw new NotImplementedException();
        }

        public void Handle(CreatedEvent evt)
        {
            this.Id = evt.AggregateRootId;
            this.Name = evt.Name;
            this.Sex = evt.Sex;
            this.CreateTime = new DateTime(evt.UTCTimestamp).ToLocalTime();
            this.LastUpdateTime = new DateTime(evt.UTCTimestamp).ToLocalTime();
        }

        public void Handle(ModifyNamedEvent evt)
        {
            this.Name = evt.Name;
            this.LastUpdateTime = new DateTime(evt.UTCTimestamp).ToLocalTime();
        }

    }
}
