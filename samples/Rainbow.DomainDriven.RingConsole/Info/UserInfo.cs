using AutoMapper.Attributes;
using Rainbow.DomainDriven.RingConsole.Domain;
using System;

namespace Rainbow.DomainDriven.RingConsole.Info
{

    [MapsFrom(typeof(User))]
    public class UserInfo
    {
        public string Id { get; set; }
        public string Name { get;  set; }
        public int Sex { get;  set; }
        public DateTime CreateTime { get;  set; }
        public DateTime LastUpdateTime { get;  set; }
    }
}