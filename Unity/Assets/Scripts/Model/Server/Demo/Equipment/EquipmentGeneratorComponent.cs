using System;
using System.Collections.Generic;

namespace ET.Server
{
    /// <summary>
    /// 装备生成器组件
    /// 根据鼎炉等级和配置表生成装备
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class EquipmentGeneratorComponent : Entity, IAwake
    {
        /// <summary>
        /// 装备名称前缀
        /// </summary>
        private readonly string[] EquipmentNames = 
        {
            "武器", "头盔", "护甲", "腰带", "靴子", "项链", "戒指", "护腕", "护符"
        };
        
        /// <summary>
        /// 随机数生成器
        /// </summary>
        public Random Random { get; set; }
    }
}