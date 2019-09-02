using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.RingConsole.Dto.View
{
    public class UserView
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Sex { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime LastUpdateTime { get; set; }

    }
}
