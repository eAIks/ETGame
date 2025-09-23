using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [ComponentOf(typeof(UI))]
    public class UIEquipmentCompareComponent : Entity, IAwake
    {
        // 通用UI元素
        public Text Title;
        public Text PromptText;
        
        // 旧装备面板（左侧或上方）
        public GameObject OldEquipmentPanel;
        public Text OldLabel;
        public Image OldIconImage;
        public Text OldNameText;
        public Text OldQualityText;
        public Text OldAttackText;
        public Text OldDefenseText;
        public Text OldHealthText;
        
        // 新装备面板（右侧或下方）
        public GameObject NewEquipmentPanel;
        public Text NewLabel;
        public Image NewIconImage;
        public Text NewNameText;
        public Text NewQualityText;
        public Text NewAttackText;
        public Text NewDefenseText;
        public Text NewHealthText;
        
        // 按钮
        public Button ConfirmButton;
        public Button CancelButton;
        
        // 数据
        public Equipment OldEquipment;
        public Equipment NewEquipment;
        public int TargetSlot;
    }
}