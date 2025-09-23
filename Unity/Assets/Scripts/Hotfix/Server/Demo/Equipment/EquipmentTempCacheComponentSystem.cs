namespace ET.Server
{
    [EntitySystemOf(typeof(EquipmentTempCacheComponent))]
    [FriendOf(typeof(EquipmentTempCacheComponent))]
    public static partial class EquipmentTempCacheComponentSystem
    {
        [EntitySystem]
        private static void Awake(this EquipmentTempCacheComponent self)
        {
            self.PlayerTempEquipments.Clear();
            self.PlayerTempSlots.Clear();
        }
        
        /// <summary>
        /// 缓存玩家生成的装备
        /// </summary>
        public static void CacheEquipment(this EquipmentTempCacheComponent self, long playerId, Equipment equipment, int slotIndex)
        {
            self.PlayerTempEquipments[playerId] = equipment;
            self.PlayerTempSlots[playerId] = slotIndex;
            
            Log.Info($"缓存临时装备: PlayerId={playerId}, EquipmentId={equipment.Id}, SlotIndex={slotIndex}");
        }
        
        /// <summary>
        /// 获取玩家的临时装备
        /// </summary>
        public static Equipment GetTempEquipment(this EquipmentTempCacheComponent self, long playerId)
        {
            self.PlayerTempEquipments.TryGetValue(playerId, out Equipment equipment);
            return equipment;
        }
        
        /// <summary>
        /// 获取玩家的临时装备槽位
        /// </summary>
        public static int GetTempSlotIndex(this EquipmentTempCacheComponent self, long playerId)
        {
            if (self.PlayerTempSlots.TryGetValue(playerId, out int slotIndex))
            {
                return slotIndex;
            }
            return -1;
        }
        
        /// <summary>
        /// 清除玩家的临时装备缓存
        /// </summary>
        public static void ClearTempEquipment(this EquipmentTempCacheComponent self, long playerId)
        {
            self.PlayerTempEquipments.Remove(playerId);
            self.PlayerTempSlots.Remove(playerId);
            
            Log.Info($"清除临时装备缓存: PlayerId={playerId}");
        }
        
        /// <summary>
        /// 检查玩家是否有临时装备
        /// </summary>
        public static bool HasTempEquipment(this EquipmentTempCacheComponent self, long playerId)
        {
            return self.PlayerTempEquipments.ContainsKey(playerId);
        }
    }
}