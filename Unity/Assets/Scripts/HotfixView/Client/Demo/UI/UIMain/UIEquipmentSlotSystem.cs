using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [EntitySystemOf(typeof(UIEquipmentSlot))]
    [FriendOf(typeof(UIEquipmentSlot))]
    public static partial class UIEquipmentSlotSystem
    {
        [EntitySystem]
        private static void Awake(this UIEquipmentSlot self)
        {
            // GameObject在Awake时可能还未被赋值，延迟初始化
            if (self.GameObject == null)
            {
                return;
            }
            
            self.InitializeComponents();
        }
        
        public static void InitializeComponents(this UIEquipmentSlot self)
        {
            if (self.GameObject == null)
            {
                Log.Warning("UIEquipmentSlot: GameObject为null，无法初始化组件");
                return;
            }
            
            Transform transform = self.GameObject.transform;
            
            // 装备槽背景
            Transform backgroundTransform = transform.Find("Background");
            if (backgroundTransform != null)
            {
                self.BackgroundImage = backgroundTransform.GetComponent<Image>();
                Log.Info($"装备槽{self.SlotIndex}找到Background子对象: {backgroundTransform.name}");
            }
            else
            {
                Log.Warning($"装备槽{self.SlotIndex}未找到Background子对象");
                self.BackgroundImage = null;
            }
            
            // Icon容器和其子组件
            Transform iconTransform = transform.Find("Icon");
            if (iconTransform != null)
            {
                // Image组件在Icon的子对象"Image"上
                Transform imageTransform = iconTransform.Find("Image");
                self.EquipmentIcon = imageTransform?.GetComponent<Image>();
                
                // Button组件在Icon的子对象"Button"上  
                Transform buttonTransform = iconTransform.Find("Button");
                self.IconButton = buttonTransform?.GetComponent<Button>();
                
                Log.Info($"装备槽{self.SlotIndex}Icon子对象: Image={imageTransform != null}, Button={buttonTransform != null}");
            }
            else
            {
                Log.Warning($"装备槽{self.SlotIndex}未找到Icon子对象");
                self.EquipmentIcon = null;
                self.IconButton = null;
            }
            
            // 装备等级
            Transform levelTransform = transform.Find("Level");
            if (levelTransform != null)
            {
                self.EquipmentLevel = levelTransform.GetComponent<Text>();
                Log.Info($"装备槽{self.SlotIndex}找到Level子对象: {levelTransform.name}");
            }
            else
            {
                Log.Warning($"装备槽{self.SlotIndex}未找到Level子对象");
                self.EquipmentLevel = null;
            }
            
            if (self.IconButton != null)
            {
                self.IconButton.onClick.RemoveAllListeners();
                self.IconButton.onClick.AddListener(() => { self.OnSlotClick().Coroutine(); });
            }
            
            Log.Info($"装备槽{self.SlotIndex}组件初始化: BackgroundImage={self.BackgroundImage != null}, EquipmentIcon={self.EquipmentIcon != null}, EquipmentLevel={self.EquipmentLevel != null}");
        }
        
        public static void SetEquipment(this UIEquipmentSlot self, Equipment equipment)
        {
            self.CurrentEquipment = equipment;
            
            // 确保组件已初始化
            if (self.BackgroundImage == null || self.EquipmentIcon == null || self.IconButton == null || self.EquipmentLevel == null)
            {
                Log.Warning($"装备槽组件未初始化，重新初始化组件");
                self.InitializeComponents();
            }
            
            if (equipment != null)
            {
                Log.Info($"设置装备槽{self.SlotIndex}装备: {equipment.Name}，品质: {equipment.Quality}，颜色: {equipment.Color}");
                
                Color qualityColor = ParseColorFromHex(equipment.Color, equipment.Quality);
                
                // 设置背景颜色
                if (self.BackgroundImage != null)
                {
                    Color originalColor = self.BackgroundImage.color;
                    self.BackgroundImage.color = qualityColor;
                    Log.Info($"设置装备槽{self.SlotIndex}背景颜色: 从{originalColor}改为{qualityColor}");
                    Log.Info($"背景Image组件名称: {self.BackgroundImage.name}");
                    Log.Info($"背景Image enabled: {self.BackgroundImage.enabled}");
                    Log.Info($"背景Image GameObject active: {self.BackgroundImage.gameObject.activeInHierarchy}");
                    Log.Info($"背景Image 最终颜色: {self.BackgroundImage.color}");
                }
                else
                {
                    Log.Warning($"装备槽{self.SlotIndex}的BackgroundImage为null");
                }
                
                // 有装备时显示图标和启用按钮
                if (self.EquipmentIcon != null)
                {
                    self.EquipmentIcon.enabled = true;
                    self.EquipmentIcon.color = qualityColor;
                    // TODO: 设置装备图标sprite
                }
                
                if (self.IconButton != null)
                {
                    self.IconButton.gameObject.SetActive(true);
                    self.IconButton.interactable = true;
                }
                
                if (self.EquipmentLevel != null)
                {
                    self.EquipmentLevel.text = $"Lv.{equipment.Level}";
                }
            }
            else
            {
                Log.Info($"清空装备槽{self.SlotIndex}");
                
                // 设置背景为默认颜色
                if (self.BackgroundImage != null)
                {
                    self.BackgroundImage.color = Color.white;
                    Log.Info($"重置装备槽{self.SlotIndex}背景颜色为白色");
                }
                
                // 无装备时隐藏图标按钮
                if (self.EquipmentIcon != null)
                {
                    self.EquipmentIcon.enabled = false;
                }
                
                if (self.IconButton != null)
                {
                    self.IconButton.gameObject.SetActive(false);
                }
                
                if (self.EquipmentLevel != null)
                {
                    self.EquipmentLevel.text = "空";
                }
            }
        }
        
        private static async ETTask OnSlotClick(this UIEquipmentSlot self)
        {
            // 只有装备不为空时才可以点击查看详情
            if (self.CurrentEquipment == null)
            {
                Log.Info("该装备槽为空，无法查看详情");
                return;
            }
            
            Scene root = self.Root();
            await UIHelper.Create(root, UIType.UIEquipmentDetail, UILayer.Mid);
            
            UI ui = root.GetComponent<UIComponent>()?.Get(UIType.UIEquipmentDetail);
            UIEquipmentDetailComponent detailComponent = ui?.GetComponent<UIEquipmentDetailComponent>();
            detailComponent?.ShowEquipment(self.CurrentEquipment);
        }
        
        
        /// <summary>
        /// 从十六进制颜色字符串解析Unity Color，支持品质回退机制
        /// </summary>
        private static Color ParseColorFromHex(string hexColor, int quality = -1)
        {
            // 首先尝试解析hexColor
            if (!string.IsNullOrEmpty(hexColor))
            {
                // 去掉#号
                if (hexColor.StartsWith("#"))
                {
                    hexColor = hexColor.Substring(1);
                }
                
                // 检查颜色字符串长度
                if (hexColor.Length == 6)
                {
                    try
                    {
                        // 解析RGB值
                        int r = System.Convert.ToInt32(hexColor.Substring(0, 2), 16);
                        int g = System.Convert.ToInt32(hexColor.Substring(2, 2), 16);
                        int b = System.Convert.ToInt32(hexColor.Substring(4, 2), 16);
                        
                        Color color = new Color(r / 255f, g / 255f, b / 255f, 1f);
                        return color;
                    }
                    catch (System.Exception e)
                    {
                        Log.Warning($"解析颜色失败: #{hexColor}，错误: {e.Message}，尝试品质回退");
                    }
                }
                else
                {
                    Log.Warning($"无效的颜色格式: #{hexColor}，尝试品质回退");
                }
            }
            
            // 颜色解析失败，尝试从品质获取颜色
            if (quality > 0)
            {
                var qualityConfig = EquipQualityConfigCategory.Instance.Get(quality);
                if (qualityConfig != null && !string.IsNullOrEmpty(qualityConfig.Color))
                {
                    Log.Info($"使用品质 {quality} 的配置颜色: {qualityConfig.Color}");
                    return ParseColorFromHex(qualityConfig.Color, -1); // 避免递归
                }
            }
            
            Log.Warning("装备颜色解析失败，使用默认白色");
            return Color.white;
        }
        
        /// <summary>
        /// 从十六进制颜色字符串解析Unity Color
        /// </summary>
        private static Color ParseColorFromHex(string hexColor)
        {
            return ParseColorFromHex(hexColor, -1);
        }
    }
}