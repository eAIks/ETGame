using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [ComponentOf(typeof(UI))]
    public class UIEquipmentConfirmComponent : Entity, IAwake
    {
        // UI元素
        public Text Title;
        public Text PromptText;
        public Text NameText;
        public Text LevelText;
        public Text AttackText;
        public Text DefenseText;
        public Text HealthText;
        public Text QualityText;
        public Image IconImage;
        public Button ConfirmButton;
        public Button CancelButton;
        
        // 数据
        public Equipment TempEquipment;
        public int TargetSlot;
    }
}