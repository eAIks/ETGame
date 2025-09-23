using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [EntitySystemOf(typeof(UIEquipmentDetailComponent))]
    [FriendOf(typeof(UIEquipmentDetailComponent))]
    public static partial class UIEquipmentDetailComponentSystem
    {
        [EntitySystem]
        private static void Awake(this UIEquipmentDetailComponent self)
        {
            ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
            
            self.NameText = rc.Get<GameObject>("NameText")?.GetComponent<Text>();
            self.LevelText = rc.Get<GameObject>("LevelText")?.GetComponent<Text>();
            self.AttackText = rc.Get<GameObject>("AttackText")?.GetComponent<Text>();
            self.DefenseText = rc.Get<GameObject>("DefenseText")?.GetComponent<Text>();
            self.HealthText = rc.Get<GameObject>("HealthText")?.GetComponent<Text>();
            self.QualityText = rc.Get<GameObject>("QualityText")?.GetComponent<Text>();
            self.DescriptionText = rc.Get<GameObject>("DescriptionText")?.GetComponent<Text>();
            self.IconImage = rc.Get<GameObject>("IconImage")?.GetComponent<Image>();
            self.CloseButton = rc.Get<GameObject>("CloseButton")?.GetComponent<Button>();
            
            if (self.CloseButton != null)
            {
                self.CloseButton.onClick.RemoveAllListeners();
                self.CloseButton.onClick.AddListener(() => { self.OnCloseClick().Coroutine(); });
            }
        }
        
        public static void ShowEquipment(this UIEquipmentDetailComponent self, Equipment equipment)
        {
            if (equipment == null) return;
            
            if (self.NameText != null)
                self.NameText.text = equipment.Name;
            
            if (self.LevelText != null)
                self.LevelText.text = $"等级: {equipment.Level}";
            
            if (self.AttackText != null)
                self.AttackText.text = $"攻击力: {equipment.Attack}";
            
            if (self.DefenseText != null)
                self.DefenseText.text = $"防御力: {equipment.Defense}";
            
            if (self.HealthText != null)
                self.HealthText.text = $"生命值: {equipment.Health}";
            
            if (self.QualityText != null)
            {
                self.QualityText.text = GetQualityName(equipment.Quality);
                self.QualityText.color = GetQualityColor(equipment.Quality);
            }
            
            if (self.DescriptionText != null)
                self.DescriptionText.text = equipment.Description;
            
            if (self.IconImage != null)
            {
                // TODO: 设置装备图标
                self.IconImage.color = GetQualityColor(equipment.Quality);
            }
        }
        
        private static async ETTask OnCloseClick(this UIEquipmentDetailComponent self)
        {
            Scene root = self.Root();
            await UIHelper.Remove(root, UIType.UIEquipmentDetail);
        }
        
        private static string GetQualityName(int quality)
        {
            switch (quality)
            {
                case 0: return "普通";
                case 1: return "精良";
                case 2: return "稀有";
                case 3: return "史诗";
                case 4: return "传说";
                default: return "未知";
            }
        }
        
        private static Color GetQualityColor(int quality)
        {
            switch (quality)
            {
                case 0: return Color.white;
                case 1: return Color.green;
                case 2: return Color.blue;
                case 3: return new Color(0.5f, 0, 0.5f);
                case 4: return new Color(1f, 0.5f, 0);
                default: return Color.gray;
            }
        }
    }
}