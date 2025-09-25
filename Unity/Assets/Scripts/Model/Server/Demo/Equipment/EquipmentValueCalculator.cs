using System;

namespace ET.Server
{
    /// <summary>
    /// 装备价值计算器
    /// 用于计算装备出售时获得的灵石和经验值
    /// </summary>
    public static class EquipmentValueCalculator
    {
        /// <summary>
        /// 品质基础灵石倍数
        /// 0:白 1:绿 2:蓝 3:紫 4:橙
        /// </summary>
        [StaticField]
        private static readonly float[] QualitySpiritStoneMultipliers = { 1.0f, 1.5f, 2.0f, 3.0f, 5.0f };
        
        /// <summary>
        /// 品质基础经验倍数
        /// </summary>
        [StaticField]
        private static readonly float[] QualityExpMultipliers = { 1.0f, 1.2f, 1.5f, 2.0f, 3.0f };
        
        /// <summary>
        /// 基础灵石值（每级装备的基础灵石）
        /// </summary>
        private const int BaseSpiritStonePerLevel = 1;
        
        /// <summary>
        /// 基础经验值（每级装备的基础经验）
        /// </summary>
        private const int BaseExpPerLevel = 1;
        
        /// <summary>
        /// 计算装备出售获得的灵石
        /// </summary>
        /// <param name="equipment">装备信息</param>
        /// <returns>灵石数量</returns>
        public static long CalculateSpiritStone(Equipment equipment)
        {
            if (equipment == null) return 0;
            
            // 基础值 
            Random random = new Random();
            float baseValue = random.Next(5,10) ;
            
            // 品质加成
            float qualityMultiplier = GetQualitySpiritStoneMultiplier(equipment.Quality);
            
            // 属性加成（根据装备总属性值）
            float attributeBonus = CalculateAttributeBonus(equipment);
            
            // 总灵石 = (基础值 + 属性加成) * 品质倍数
            long totalSpiritStone = (long)((baseValue + attributeBonus) * qualityMultiplier);
            
            return Math.Max(1, totalSpiritStone); // 最少获得1灵石
        }
        
        /// <summary>
        /// 计算装备出售获得的经验值
        /// </summary>
        /// <param name="equipment">装备信息</param>
        /// <returns>经验值</returns>
        public static long CalculateExperience(Equipment equipment)
        {
            if (equipment == null) return 0;
            
            // 基础值
            Random random = new Random();
            float baseValue = random.Next(5,10) ;
            
            // 品质加成
            float qualityMultiplier = GetQualityExpMultiplier(equipment.Quality);
            
            // 属性加成
            float attributeBonus = CalculateAttributeBonus(equipment);
            
            // 总经验 = (基础值 + 属性加成) * 品质倍数
            long totalExp = (long)((baseValue + attributeBonus) * qualityMultiplier);
            
            return Math.Max(1, totalExp); // 最少获得1经验
        }
        
        /// <summary>
        /// 获取品质对应的灵石倍数
        /// </summary>
        private static float GetQualitySpiritStoneMultiplier(int quality)
        {
            if (quality >= 0 && quality < QualitySpiritStoneMultipliers.Length)
            {
                return QualitySpiritStoneMultipliers[quality];
            }
            return 1.0f;
        }
        
        /// <summary>
        /// 获取品质对应的经验倍数
        /// </summary>
        private static float GetQualityExpMultiplier(int quality)
        {
            if (quality >= 0 && quality < QualityExpMultipliers.Length)
            {
                return QualityExpMultipliers[quality];
            }
            return 1.0f;
        }
        
        /// <summary>
        /// 计算装备属性加成
        /// </summary>
        private static float CalculateAttributeBonus(Equipment equipment)
        {
            // 根据装备总属性值计算额外奖励
            int totalAttributes = equipment.Attack + equipment.Defense + equipment.Health;
            return totalAttributes * 0.1f; // 每点属性增加0.1的基础奖励
        }
    }
}