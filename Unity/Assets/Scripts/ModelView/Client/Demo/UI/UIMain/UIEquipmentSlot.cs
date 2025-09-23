using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [ChildOf(typeof(UIMainComponent))]
    public class UIEquipmentSlot : Entity, IAwake
    {
        public GameObject GameObject;
        public int SlotIndex;
        public Equipment CurrentEquipment;
        
        public Image BackgroundImage; // 装备槽背景
        public Image EquipmentIcon;
        public Button IconButton; // Icon上的Button组件
        public Text EquipmentLevel; // 显示装备等级
    }
}