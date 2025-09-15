using System;
using System.Collections.Generic;

namespace ET
{
    [ChildOf]
    public class ServerInfo : Entity, IAwake
    {
        public int ServerId { get; set; }
        public string ServerName { get; set; }
        public string ServerIP { get; set; }
        public int ServerPort { get; set; }
        public ServerStatus Status { get; set; }
        public int OnlineCount { get; set; }
        public int MaxCount { get; set; }
        public new bool IsNew { get; set; }
        public bool IsRecommend { get; set; }
        public DateTime OpenTime { get; set; }
        public int ZoneId { get; set; }
    }

    public enum ServerStatus
    {
        Maintenance = 0,
        Smooth = 1,
        Normal = 2,
        Crowded = 3,
        Full = 4
    }

    [ChildOf]
    public class ServerZone : Entity, IAwake
    {
        public int ZoneId { get; set; }
        public string ZoneName { get; set; }
        public List<ServerInfo> ServerList { get; set; } = new List<ServerInfo>();
    }

    [ComponentOf(typeof(Scene))]
    public class ServerListComponent : Entity, IAwake, IDestroy
    {
        public List<ServerZone> ZoneList { get; set; } = new List<ServerZone>();
        public ServerInfo CurrentServer { get; set; }
        public ServerInfo LastLoginServer { get; set; }
    }
}