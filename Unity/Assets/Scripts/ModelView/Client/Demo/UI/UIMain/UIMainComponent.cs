using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [ComponentOf(typeof(UI))]
    public class UIMainComponent : Entity, IAwake
    {
        public Dictionary<int, EntityRef<UIEquipmentSlot>> EquipmentSlots = new Dictionary<int, EntityRef<UIEquipmentSlot>>();
        public GameObject EquipmentPanel;
        public Button GenerateEquipmentButton;
        
        // 直接在UI组件中管理装备数据，无需依赖Unit
        public Dictionary<int, Equipment> LocalEquipments = new Dictionary<int, Equipment>();
        
        // 防重复点击标志
        public bool IsGeneratingEquipment = false;
        
        // 境界显示组件
        public EntityRef<UIRealmLevel> UIRealmLevel;
        
        // 本地境界信息
        public RealmInfoProto LocalRealmInfo;
    }
}