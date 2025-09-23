using System.Collections.Generic;

namespace ET.Server
{
    /// <summary>
    /// 装备临时缓存组件
    /// 用于缓存玩家生成的装备，等待确认替换
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class EquipmentTempCacheComponent : Entity, IAwake
    {
        /// <summary>
        /// 玩家临时装备缓存
        /// playerId -> Equipment
        /// </summary>
        public Dictionary<long, Equipment> PlayerTempEquipments = new Dictionary<long, Equipment>();
        
        /// <summary>
        /// 玩家装备槽位缓存
        /// playerId -> slotIndex
        /// </summary>
        public Dictionary<long, int> PlayerTempSlots = new Dictionary<long, int>();
    }
}