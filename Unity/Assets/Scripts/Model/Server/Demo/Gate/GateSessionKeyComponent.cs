using System.Collections.Generic;

namespace ET.Server
{
    [ComponentOf(typeof(Scene))]
    public class GateSessionKeyComponent : Entity, IAwake
    {
        public readonly Dictionary<long, string> sessionKey = new();
        public readonly Dictionary<long, long> sessionPlayerID = new();
        public readonly Dictionary<long, string> sessionAccountUUID = new();
        public readonly Dictionary<long, int> sessionServerId = new();
    }
}