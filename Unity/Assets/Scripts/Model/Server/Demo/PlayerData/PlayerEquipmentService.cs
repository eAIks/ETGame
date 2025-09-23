using System.Collections.Generic;

namespace ET.Server
{
    [ComponentOf(typeof(Scene))]
    public class PlayerEquipmentService : Entity, IAwake
    {
        // 玩家装备数据 playerId -> slotIndex -> Equipment
        public Dictionary<long, Dictionary<int, Equipment>> PlayerEquipmentSlots = new Dictionary<long, Dictionary<int, Equipment>>();
    }
}