using MemoryPack;
using System;

namespace ET
{
    [EnableClass]
    [Serializable]
    [MemoryPackable]
    public partial class Equipment
    {
        [MemoryPackOrder(0)]
        public long Id { get; set; }
        
        [MemoryPackOrder(1)]
        public string Name { get; set; }
        
        [MemoryPackOrder(2)]
        public int SlotType { get; set; } // 0-8 对应9个装备槽
        
        [MemoryPackOrder(3)]
        public int Attack { get; set; }
        
        [MemoryPackOrder(4)]
        public int Defense { get; set; }
        
        [MemoryPackOrder(5)]
        public int Health { get; set; }
        
        [MemoryPackOrder(6)]
        public int Quality { get; set; } // 0:白 1:绿 2:蓝 3:紫 4:橙
        
        [MemoryPackOrder(7)]
        public int Level { get; set; } // 装备等级
        
        [MemoryPackOrder(8)]
        public string Icon { get; set; }
        
        [MemoryPackOrder(9)]
        public string Description { get; set; }
    }
}