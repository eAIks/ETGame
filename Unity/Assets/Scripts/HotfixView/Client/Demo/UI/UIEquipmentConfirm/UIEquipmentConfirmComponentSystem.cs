using System;
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [EntitySystemOf(typeof(UIEquipmentConfirmComponent))]
    [FriendOf(typeof(UIEquipmentConfirmComponent))]
    [FriendOf(typeof(UIEquipmentSlot))]
    [FriendOf(typeof(UIMainComponent))]
    public static partial class UIEquipmentConfirmComponentSystem
    {
        [EntitySystem]
        private static void Awake(this UIEquipmentConfirmComponent self)
        {
            ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
            
            // UI元素
            self.Title = rc.Get<GameObject>("Title")?.GetComponent<Text>();
            self.PromptText = rc.Get<GameObject>("PromptText")?.GetComponent<Text>();
            self.NameText = rc.Get<GameObject>("NameText")?.GetComponent<Text>();
            self.LevelText = rc.Get<GameObject>("LevelText")?.GetComponent<Text>();
            self.AttackText = rc.Get<GameObject>("AttackText")?.GetComponent<Text>();
            self.DefenseText = rc.Get<GameObject>("DefenseText")?.GetComponent<Text>();
            self.HealthText = rc.Get<GameObject>("HealthText")?.GetComponent<Text>();
            self.QualityText = rc.Get<GameObject>("QualityText")?.GetComponent<Text>();
            self.IconImage = rc.Get<GameObject>("IconImage")?.GetComponent<Image>();
            self.ConfirmButton = rc.Get<GameObject>("ConfirmButton")?.GetComponent<Button>();
            self.CancelButton = rc.Get<GameObject>("CancelButton")?.GetComponent<Button>();
            
            // 绑定事件
            if (self.ConfirmButton != null)
            {
                self.ConfirmButton.onClick.RemoveAllListeners();
                self.ConfirmButton.onClick.AddListener(() => { self.OnConfirmClick().Coroutine(); });
            }
            
            if (self.CancelButton != null)
            {
                self.CancelButton.onClick.RemoveAllListeners();
                self.CancelButton.onClick.AddListener(() => { self.OnSellClick().Coroutine(); });
            }
        }
        
        public static void ShowConfirm(this UIEquipmentConfirmComponent self, Equipment equipment, int slot)
        {
            self.TempEquipment = equipment;
            self.TargetSlot = slot;
            
            if (equipment == null) return;
            
            // 设置标题
            if (self.Title != null)
                self.Title.text = "装备确认";
            
            if (self.PromptText != null)
                self.PromptText.text = "是否装备以下装备？";
            
            // 显示装备信息
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
                var qualityConfig = EquipQualityConfigCategory.Instance.Get(equipment.Quality);
                self.QualityText.text = qualityConfig?.QualityName ?? "未知";
                self.QualityText.color = ParseColorFromHex(equipment.Color);
            }
            
            if (self.IconImage != null)
            {
                self.IconImage.color = ParseColorFromHex(equipment.Color);
            }
        }
        
        private static async ETTask OnConfirmClick(this UIEquipmentConfirmComponent self)
        {
            if (self.TempEquipment == null) return;
            
            Scene root = self.Root();
            
            try
            {
                // 发送替换装备消息到服务端
                ClientSenderComponent clientSenderComponent = root.GetComponent<ClientSenderComponent>();
                if (clientSenderComponent == null)
                {
                    Log.Error("ClientSenderComponent为空，无法发送装备替换请求");
                    return;
                }
                
                C2G_ReplaceEquipment request = C2G_ReplaceEquipment.Create();
                request.EquipmentId = self.TempEquipment.Id;
                request.SlotIndex = self.TargetSlot;
                
                Log.Info($"客户端发送装备替换请求，装备ID: {request.EquipmentId}, 槽位: {request.SlotIndex}");
                
                G2C_ReplaceEquipment response = (G2C_ReplaceEquipment)await clientSenderComponent.Call(request);
                
                if (response == null)
                {
                    Log.Error("服务端返回的装备替换响应为空");
                    return;
                }
                
                if (response.Error != ErrorCode.ERR_Success)
                {
                    Log.Error($"装备替换失败，错误码: {response.Error}, 错误信息: {response.Message}");
                    return;
                }
                
                if (response.Success)
                {
                    Log.Info("装备替换成功");
                    
                    // 通过事件系统更新装备UI，避免循环依赖
                    await EventSystem.Instance.PublishAsync(root, new EquipmentWearEvent
                    {
                        SlotIndex = self.TargetSlot,
                        Equipment = self.TempEquipment
                    });
                }
                else
                {
                    Log.Warning("服务端返回装备替换失败");
                }
            }
            catch (Exception e)
            {
                Log.Error($"装备替换请求异常: {e}");
            }
            
            await UIHelper.Remove(root, UIType.UIEquipmentConfirm);
        }
        
        private static async ETTask OnSellClick(this UIEquipmentConfirmComponent self)
        {
            Log.Info($"[客户端调试] OnSellClick开始执行");
            
            if (self.TempEquipment == null)
            {
                Log.Warning($"[客户端调试] TempEquipment为空，退出出售流程");
                return;
            }
            
            Scene root = self.Root();
            Log.Info($"[客户端调试] 获取root scene: {root?.GetType()?.Name}");
            
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
                request.EquipmentId = self.TempEquipment.Id;
                
                Log.Info($"客户端发送装备出售请求，装备ID: {request.EquipmentId}");
                
                Log.Info($"[客户端调试] 开始等待服务端响应...");
                G2C_SellEquipment response = (G2C_SellEquipment)await clientSenderComponent.Call(request);
                
                Log.Info($"[客户端调试] 收到服务端响应，response是否为空: {response == null}");
                
                if (response == null)
                {
                    Log.Error("服务端返回的装备出售响应为空");
                    return;
                }
                
                Log.Info($"[客户端调试] 响应错误码: {response.Error}");
                
                if (response.Error != ErrorCode.ERR_Success)
                {
                    Log.Error($"装备出售失败，错误码: {response.Error}, 错误信息: {response.Message}");
                    return;
                }
                
                Log.Info($"[客户端调试] 响应成功标志: {response.Success}");
                
                if (response.Success)
                {
                    Log.Info($"装备出售成功，获得灵石: {response.SpiritStoneGained}, 经验: {response.ExpGained}");
                    Log.Info($"[调试] 服务端返回信息: LevelChanged={response.LevelChanged}, NewLevel={response.NewLevel}, NewMajorRealm={response.NewMajorRealm}, NewMinorRealm={response.NewMinorRealm}");
                    
                    // 如果角色升级了，记录日志并发布事件通知UI更新
                    if (response.LevelChanged)
                    {
                        Log.Info($"角色升级! 新等级: {response.NewLevel}, 新境界: {response.NewMajorRealm}.{response.NewMinorRealm}, " +
                                $"当前经验: {response.NewCurrentExp}, 灵石: {response.NewSpiritStone}");
                        
                        // 发布角色升级事件，通知UI更新
                        Log.Info($"[事件发送] 准备发送PlayerLevelChangedEvent事件, root类型: {root?.GetType()?.Name}");
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
                        Log.Info($"[事件发送] PlayerLevelChangedEvent事件已发送");
                    }
                    else
                    {
                        // 只更新经验和灵石
                        Log.Info($"[事件发送] 准备发送PlayerResourceChangedEvent事件");
                        await EventSystem.Instance.PublishAsync(root, new PlayerResourceChangedEvent
                        {
                            NewCurrentExp = response.NewCurrentExp,
                            NewSpiritStone = response.NewSpiritStone,
                            SpiritStoneGained = response.SpiritStoneGained,
                            ExpGained = response.ExpGained
                        });
                        Log.Info($"[事件发送] PlayerResourceChangedEvent事件已发送");
                    }
                }
                else
                {
                    Log.Warning("服务端返回装备出售失败");
                }
            }
            catch (Exception e)
            {
                Log.Error($"装备出售请求异常: {e}");
            }
            
            await UIHelper.Remove(root, UIType.UIEquipmentConfirm);
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