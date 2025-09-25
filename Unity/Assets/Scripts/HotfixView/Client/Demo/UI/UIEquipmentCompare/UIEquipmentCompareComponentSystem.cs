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
                    Log.Info("CancelButton被点击 - 出售新装备");
                    self.OnSellNewEquipmentClick().Coroutine(); 
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
                var qualityConfig = EquipQualityConfigCategory.Instance.Get(equipment.Quality);
                self.OldQualityText.text = qualityConfig?.QualityName ?? "未知";
                self.OldQualityText.color = ParseColorFromHex(equipment.Color);
            }
            
            if (self.OldIconImage != null)
            {
                self.OldIconImage.color = ParseColorFromHex(equipment.Color);
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
                var qualityConfig = EquipQualityConfigCategory.Instance.Get(newEquipment.Quality);
                self.NewQualityText.text = qualityConfig?.QualityName ?? "未知";
                self.NewQualityText.color = ParseColorFromHex(newEquipment.Color);
            }
            
            if (self.NewIconImage != null)
            {
                self.NewIconImage.color = ParseColorFromHex(newEquipment.Color);
            }
        }
        
        private static async ETTask OnReplaceClick(this UIEquipmentCompareComponent self)
        {
            Log.Info("OnReplaceClick开始执行 - 出售旧装备并穿戴新装备");
            
            if (self.NewEquipment == null) 
            {
                Log.Warning("NewEquipment为null，无法替换");
                return;
            }
            
            if (self.OldEquipment == null) 
            {
                Log.Warning("OldEquipment为null，无法出售旧装备");
                return;
            }
            
            Scene root = self.Root();
            
            try
            {
                // 先出售旧装备
                ClientSenderComponent clientSenderComponent = root.GetComponent<ClientSenderComponent>();
                if (clientSenderComponent == null)
                {
                    Log.Error("ClientSenderComponent为空，无法发送装备出售请求");
                    return;
                }
                
                C2G_SellEquipment request = C2G_SellEquipment.Create();
                request.EquipmentId = self.OldEquipment.Id;
                
                Log.Info($"客户端发送出售旧装备请求，装备ID: {request.EquipmentId}");
                
                G2C_SellEquipment response = (G2C_SellEquipment)await clientSenderComponent.Call(request);
                
                if (response == null)
                {
                    Log.Error("服务端返回的装备出售响应为空");
                    return;
                }
                
                if (response.Error != ErrorCode.ERR_Success)
                {
                    Log.Error($"旧装备出售失败，错误码: {response.Error}, 错误信息: {response.Message}");
                    return;
                }
                
                if (response.Success)
                {
                    Log.Info($"旧装备出售成功，获得灵石: {response.SpiritStoneGained}, 经验: {response.ExpGained}");
                    
                    // 处理经验和灵石UI刷新
                    if (response.LevelChanged)
                    {
                        Log.Info($"角色升级! 新等级: {response.NewLevel}, 新境界: {response.NewMajorRealm}.{response.NewMinorRealm}, " +
                                $"当前经验: {response.NewCurrentExp}, 灵石: {response.NewSpiritStone}");
                        
                        await EventSystem.Instance.PublishAsync(root, new PlayerLevelChangedEvent
                        {
                            NewLevel = response.NewLevel,
                            NewMajorRealm = response.NewMajorRealm,
                            NewMinorRealm = response.NewMinorRealm,
                            NewCurrentExp = response.NewCurrentExp,
                            NewSpiritStone = response.NewSpiritStone,
                            SpiritStoneGained = response.SpiritStoneGained,
                            ExpGained = response.ExpGained
                        });
                        Log.Info($"PlayerLevelChangedEvent事件已发送");
                    }
                    else
                    {
                        // 只更新经验和灵石
                        Log.Info($"发送PlayerResourceChangedEvent事件");
                        await EventSystem.Instance.PublishAsync(root, new PlayerResourceChangedEvent
                        {
                            NewCurrentExp = response.NewCurrentExp,
                            NewSpiritStone = response.NewSpiritStone,
                            SpiritStoneGained = response.SpiritStoneGained,
                            ExpGained = response.ExpGained
                        });
                        Log.Info($"PlayerResourceChangedEvent事件已发送");
                    }
                    
                    // 出售旧装备后，穿戴新装备
                    await EventSystem.Instance.PublishAsync(root, new EquipmentWearEvent
                    {
                        SlotIndex = self.TargetSlot,
                        Equipment = self.NewEquipment
                    });
                    
                    Log.Info("旧装备已出售，新装备已穿戴");
                }
                else
                {
                    Log.Warning("服务端返回旧装备出售失败");
                    return;
                }
            }
            catch (System.Exception e)
            {
                Log.Error($"替换装备过程异常: {e}");
                return;
            }
            
            await UIHelper.Remove(root, UIType.UIEquipmentCompare);
        }
        
        private static async ETTask OnSellNewEquipmentClick(this UIEquipmentCompareComponent self)
        {
            Log.Info("OnSellNewEquipmentClick开始执行 - 出售新装备并保持旧装备");
            
            if (self.NewEquipment == null) 
            {
                Log.Warning("NewEquipment为null，无法出售");
                return;
            }
            
            Scene root = self.Root();
            
            try
            {
                // 发送出售装备消息到服务端
                ClientSenderComponent clientSenderComponent = root.GetComponent<ClientSenderComponent>();
                if (clientSenderComponent == null)
                {
                    Log.Error("ClientSenderComponent为空，无法发送装备出售请求");
                    return;
                }
                
                C2G_SellEquipment request = C2G_SellEquipment.Create();
                request.EquipmentId = self.NewEquipment.Id;
                
                Log.Info($"客户端发送出售新装备请求，装备ID: {request.EquipmentId}");
                
                G2C_SellEquipment response = (G2C_SellEquipment)await clientSenderComponent.Call(request);
                
                if (response == null)
                {
                    Log.Error("服务端返回的装备出售响应为空");
                    return;
                }
                
                if (response.Error != ErrorCode.ERR_Success)
                {
                    Log.Error($"新装备出售失败，错误码: {response.Error}, 错误信息: {response.Message}");
                    return;
                }
                
                if (response.Success)
                {
                    Log.Info($"新装备出售成功，获得灵石: {response.SpiritStoneGained}, 经验: {response.ExpGained}，保持旧装备");
                    
                    // 处理经验和灵石UI刷新
                    if (response.LevelChanged)
                    {
                        Log.Info($"角色升级! 新等级: {response.NewLevel}, 新境界: {response.NewMajorRealm}.{response.NewMinorRealm}, " +
                                $"当前经验: {response.NewCurrentExp}, 灵石: {response.NewSpiritStone}");
                        
                        await EventSystem.Instance.PublishAsync(root, new PlayerLevelChangedEvent
                        {
                            NewLevel = response.NewLevel,
                            NewMajorRealm = response.NewMajorRealm,
                            NewMinorRealm = response.NewMinorRealm,
                            NewCurrentExp = response.NewCurrentExp,
                            NewSpiritStone = response.NewSpiritStone,
                            SpiritStoneGained = response.SpiritStoneGained,
                            ExpGained = response.ExpGained
                        });
                        Log.Info($"PlayerLevelChangedEvent事件已发送");
                    }
                    else
                    {
                        // 只更新经验和灵石
                        Log.Info($"发送PlayerResourceChangedEvent事件");
                        await EventSystem.Instance.PublishAsync(root, new PlayerResourceChangedEvent
                        {
                            NewCurrentExp = response.NewCurrentExp,
                            NewSpiritStone = response.NewSpiritStone,
                            SpiritStoneGained = response.SpiritStoneGained,
                            ExpGained = response.ExpGained
                        });
                        Log.Info($"PlayerResourceChangedEvent事件已发送");
                    }
                    
                    // 出售新装备，保持旧装备不变，不需要发布装备穿戴事件
                    // 旧装备继续保持在装备槽中
                }
                else
                {
                    Log.Warning("服务端返回新装备出售失败");
                }
            }
            catch (System.Exception e)
            {
                Log.Error($"新装备出售请求异常: {e}");
            }
            
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
        
        /// <summary>
        /// 从十六进制颜色字符串解析Unity Color
        /// </summary>
        private static Color ParseColorFromHex(string hexColor)
        {
            if (string.IsNullOrEmpty(hexColor))
            {
                Log.Warning("装备颜色为空，使用默认白色");
                return Color.white;
            }
            
            // 去掉#号
            if (hexColor.StartsWith("#"))
            {
                hexColor = hexColor.Substring(1);
            }
            
            // 检查颜色字符串长度
            if (hexColor.Length != 6)
            {
                Log.Error($"无效的颜色格式: {hexColor}，使用默认白色");
                return Color.white;
            }
            
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
                Log.Error($"解析颜色失败: {hexColor}，错误: {e.Message}，使用默认白色");
                return Color.white;
            }
        }
    }
}