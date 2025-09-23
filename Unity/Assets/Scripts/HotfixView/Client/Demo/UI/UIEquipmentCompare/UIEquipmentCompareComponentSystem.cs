using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [EntitySystemOf(typeof(UIEquipmentCompareComponent))]
    [FriendOf(typeof(UIEquipmentCompareComponent))]
    [FriendOf(typeof(UIEquipmentSlot))]
    [FriendOf(typeof(UIMainComponent))]
    public static partial class UIEquipmentCompareComponentSystem
    {
        [EntitySystem]
        private static void Awake(this UIEquipmentCompareComponent self)
        {
            ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
            
            // 通用元素
            self.Title = rc.Get<GameObject>("Title")?.GetComponent<Text>();
            self.PromptText = rc.Get<GameObject>("PromptText")?.GetComponent<Text>();
            
            // 旧装备面板
            self.OldEquipmentPanel = rc.Get<GameObject>("OldEquipmentPanel");
            self.OldLabel = rc.Get<GameObject>("OldLabel")?.GetComponent<Text>();
            self.OldIconImage = rc.Get<GameObject>("OldIconImage")?.GetComponent<Image>();
            self.OldNameText = rc.Get<GameObject>("OldNameText")?.GetComponent<Text>();
            self.OldQualityText = rc.Get<GameObject>("OldQualityText")?.GetComponent<Text>();
            self.OldAttackText = rc.Get<GameObject>("OldAttackText")?.GetComponent<Text>();
            self.OldDefenseText = rc.Get<GameObject>("OldDefenseText")?.GetComponent<Text>();
            self.OldHealthText = rc.Get<GameObject>("OldHealthText")?.GetComponent<Text>();
            
            // 新装备面板
            self.NewEquipmentPanel = rc.Get<GameObject>("NewEquipmentPanel");
            self.NewLabel = rc.Get<GameObject>("NewLabel")?.GetComponent<Text>();
            self.NewIconImage = rc.Get<GameObject>("NewIconImage")?.GetComponent<Image>();
            self.NewNameText = rc.Get<GameObject>("NewNameText")?.GetComponent<Text>();
            self.NewQualityText = rc.Get<GameObject>("NewQualityText")?.GetComponent<Text>();
            self.NewAttackText = rc.Get<GameObject>("NewAttackText")?.GetComponent<Text>();
            self.NewDefenseText = rc.Get<GameObject>("NewDefenseText")?.GetComponent<Text>();
            self.NewHealthText = rc.Get<GameObject>("NewHealthText")?.GetComponent<Text>();
            
            // 按钮
            self.ConfirmButton = rc.Get<GameObject>("ConfirmButton")?.GetComponent<Button>();
            self.CancelButton = rc.Get<GameObject>("CancelButton")?.GetComponent<Button>();
            
            Log.Info($"UIEquipmentCompare按钮初始化: ConfirmButton={self.ConfirmButton != null}, CancelButton={self.CancelButton != null}");
            
            // 绑定事件
            self.BindButtons();
        }
        
        private static void BindButtons(this UIEquipmentCompareComponent self)
        {
            if (self.ConfirmButton != null)
            {
                self.ConfirmButton.onClick.RemoveAllListeners();
                self.ConfirmButton.onClick.AddListener(() => { 
                    Log.Info("ConfirmButton被点击");
                    self.OnReplaceClick().Coroutine(); 
                });
                Log.Info("ConfirmButton事件绑定成功");
            }
            else
            {
                Log.Warning("ConfirmButton为null，无法绑定事件");
            }
            
            if (self.CancelButton != null)
            {
                self.CancelButton.onClick.RemoveAllListeners();
                self.CancelButton.onClick.AddListener(() => { 
                    Log.Info("CancelButton被点击");
                    self.OnKeepClick().Coroutine(); 
                });
                Log.Info("CancelButton事件绑定成功");
            }
            else
            {
                Log.Warning("CancelButton为null，无法绑定事件");
            }
        }
        
        public static void ShowCompare(this UIEquipmentCompareComponent self, Equipment oldEquipment, Equipment newEquipment, int slot)
        {
            self.OldEquipment = oldEquipment;
            self.NewEquipment = newEquipment;
            self.TargetSlot = slot;
            
            // 设置标题和提示
            if (self.Title != null)
                self.Title.text = "装备替换";
            
            if (self.PromptText != null)
                self.PromptText.text = "发现新装备，是否替换当前装备？";
            
            // 显示旧装备信息
            self.ShowOldEquipment();
            
            // 显示新装备信息（带对比）
            self.ShowNewEquipment();
        }
        
        private static void ShowOldEquipment(this UIEquipmentCompareComponent self)
        {
            var equipment = self.OldEquipment;
            if (equipment == null) return;
            
            if (self.OldLabel != null)
                self.OldLabel.text = "当前装备";
            
            if (self.OldNameText != null)
                self.OldNameText.text = equipment.Name;
            
            if (self.OldAttackText != null)
                self.OldAttackText.text = $"攻击力: {equipment.Attack}";
            
            if (self.OldDefenseText != null)
                self.OldDefenseText.text = $"防御力: {equipment.Defense}";
            
            if (self.OldHealthText != null)
                self.OldHealthText.text = $"生命值: {equipment.Health}";
            
            if (self.OldQualityText != null)
            {
                self.OldQualityText.text = GetQualityName(equipment.Quality);
                self.OldQualityText.color = GetQualityColor(equipment.Quality);
            }
            
            if (self.OldIconImage != null)
            {
                self.OldIconImage.color = GetQualityColor(equipment.Quality);
            }
        }
        
        private static void ShowNewEquipment(this UIEquipmentCompareComponent self)
        {
            var newEquipment = self.NewEquipment;
            var oldEquipment = self.OldEquipment;
            if (newEquipment == null || oldEquipment == null) return;
            
            if (self.NewLabel != null)
                self.NewLabel.text = "新装备";
            
            if (self.NewNameText != null)
                self.NewNameText.text = newEquipment.Name;
            
            if (self.NewAttackText != null)
                self.NewAttackText.text = GetCompareText("攻击力", newEquipment.Attack, oldEquipment.Attack);
            
            if (self.NewDefenseText != null)
                self.NewDefenseText.text = GetCompareText("防御力", newEquipment.Defense, oldEquipment.Defense);
            
            if (self.NewHealthText != null)
                self.NewHealthText.text = GetCompareText("生命值", newEquipment.Health, oldEquipment.Health);
            
            if (self.NewQualityText != null)
            {
                self.NewQualityText.text = GetQualityName(newEquipment.Quality);
                self.NewQualityText.color = GetQualityColor(newEquipment.Quality);
            }
            
            if (self.NewIconImage != null)
            {
                self.NewIconImage.color = GetQualityColor(newEquipment.Quality);
            }
        }
        
        private static async ETTask OnReplaceClick(this UIEquipmentCompareComponent self)
        {
            Log.Info("OnReplaceClick开始执行");
            
            if (self.NewEquipment == null) 
            {
                Log.Warning("NewEquipment为null，无法替换");
                return;
            }
            
            Scene root = self.Root();
            
            // 通过事件系统更新装备，避免循环依赖
            await EventSystem.Instance.PublishAsync(root, new EquipmentWearEvent
            {
                SlotIndex = self.TargetSlot,
                Equipment = self.NewEquipment
            });
            
            Log.Info("装备替换事件已发布，准备关闭对比界面");
            await UIHelper.Remove(root, UIType.UIEquipmentCompare);
        }
        
        private static async ETTask OnKeepClick(this UIEquipmentCompareComponent self)
        {
            Log.Info("OnKeepClick开始执行，保持当前装备");
            
            Scene root = self.Root();
            await UIHelper.Remove(root, UIType.UIEquipmentCompare);
        }
        
        private static string GetCompareText(string attributeName, int newValue, int oldValue)
        {
            string arrow = "";
            if (newValue > oldValue)
            {
                arrow = " ↑";
            }
            else if (newValue < oldValue)
            {
                arrow = " ↓";
            }
            
            return $"{attributeName}: {newValue}{arrow}";
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