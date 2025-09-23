using System.Collections.Generic;

namespace ET.Server
{
    [EntitySystemOf(typeof(PlayerEquipmentService))]
    [FriendOf(typeof(PlayerEquipmentService))]
    public static partial class PlayerEquipmentServiceSystem
    {
        [EntitySystem]
        private static void Awake(this PlayerEquipmentService self)
        {
            self.PlayerEquipmentSlots.Clear();
        }
        
        public static bool IsPlayerExist(this PlayerEquipmentService self, long playerId)
        {
            return self.PlayerEquipmentSlots.ContainsKey(playerId);
        }
        
        public static void EnsurePlayerData(this PlayerEquipmentService self, long playerId)
        {
            if (!self.PlayerEquipmentSlots.ContainsKey(playerId))
            {
                self.PlayerEquipmentSlots[playerId] = new Dictionary<int, Equipment>();
                
                // 初始化9个装备槽
                for (int i = 0; i < 9; i++)
                {
                    self.PlayerEquipmentSlots[playerId][i] = null;
                }
            }
        }
        
        public static Dictionary<int, Equipment> GetPlayerEquipments(this PlayerEquipmentService self, long playerId)
        {
            self.EnsurePlayerData(playerId);
            return self.PlayerEquipmentSlots[playerId];
        }
        
        // 直接创建新装备的方法，不再需要从背包中查找
        public static Equipment CreateEquipment(long equipmentId, string name, int slotType, int attack, int defense, int health, int quality, int level, string icon, string description)
        {
            return new Equipment
            {
                Id = equipmentId,
                Name = name,
                SlotType = slotType,
                Attack = attack,
                Defense = defense,
                Health = health,
                Quality = quality,
                Level = level,
                Icon = icon,
                Description = description
            };
        }
        
        public static Equipment GetEquippedEquipment(this PlayerEquipmentService self, long playerId, int slotIndex)
        {
            self.EnsurePlayerData(playerId);
            
            if (slotIndex >= 0 && slotIndex < 9)
            {
                self.PlayerEquipmentSlots[playerId].TryGetValue(slotIndex, out Equipment equipment);
                return equipment;
            }
            
            return null;
        }
        
        public static void SetEquippedEquipment(this PlayerEquipmentService self, long playerId, int slotIndex, Equipment equipment)
        {
            self.EnsurePlayerData(playerId);
            
            if (slotIndex >= 0 && slotIndex < 9)
            {
                // 替换装备
                self.PlayerEquipmentSlots[playerId][slotIndex] = equipment;
            }
        }
        
    }
}