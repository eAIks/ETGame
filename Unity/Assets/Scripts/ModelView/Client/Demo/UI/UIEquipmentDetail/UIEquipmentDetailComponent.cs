using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [ComponentOf(typeof(UI))]
    public class UIEquipmentDetailComponent : Entity, IAwake
    {
        public Text NameText;
        public Text LevelText;
        public Text AttackText;
        public Text DefenseText;
        public Text HealthText;
        public Text QualityText;
        public Text DescriptionText;
        public Image IconImage;
        public Button CloseButton;
    }
}