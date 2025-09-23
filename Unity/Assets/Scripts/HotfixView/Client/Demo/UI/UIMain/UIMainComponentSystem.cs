using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [EntitySystemOf(typeof(UIMainComponent))]
    [FriendOf(typeof(UIMainComponent))]
    [FriendOf(typeof(UIEquipmentSlot))]
    public static partial class UIMainComponentSystem
    {
        [EntitySystem]
        private static void Awake(this UIMainComponent self)
        {
            ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
            self.EquipmentPanel = rc.Get<GameObject>("EquipmentPanel");
            self.GenerateEquipmentButton = rc.Get<GameObject>("GenerateEquipmentButton")?.GetComponent<Button>();
            
            if (self.GenerateEquipmentButton != null)
            {
                // 确保按钮是可交互的
                self.GenerateEquipmentButton.interactable = true;
                
                // 确保按钮的CanvasGroup设置正确（如果存在）
                var buttonCanvasGroup = self.GenerateEquipmentButton.GetComponent<CanvasGroup>();
                if (buttonCanvasGroup != null)
                {
                    buttonCanvasGroup.blocksRaycasts = true;
                    buttonCanvasGroup.interactable = true;
                }
                
                // 设置按钮Canvas排序顺序（如果存在）
                var buttonCanvas = self.GenerateEquipmentButton.GetComponent<Canvas>();
                if (buttonCanvas != null)
                {
                    buttonCanvas.sortingOrder = 100;
                }
                
                self.GenerateEquipmentButton.onClick.RemoveAllListeners();
                self.GenerateEquipmentButton.onClick.AddListener(() => { 
                    self.OnGenerateEquipmentClick().Coroutine(); 
                });
            }
            
            self.InitEquipmentSlots();
        }
        
        private static void InitEquipmentSlots(this UIMainComponent self)
        {
            // 通过ReferenceCollector获取装备槽
            ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
            
            Log.Info("开始初始化装备槽");
            
            for (int i = 0; i < 9; i++)
            {
                GameObject slotGo = rc.Get<GameObject>($"Slot{i}");
                if (slotGo != null)
                {
                    UIEquipmentSlot slot = self.AddChild<UIEquipmentSlot>();
                    slot.SlotIndex = i;
                    slot.GameObject = slotGo;
                    slot.InitializeComponents(); // 设置GameObject后立即初始化组件
                    self.EquipmentSlots[i] = slot;
                    Log.Info($"装备槽{i}初始化成功");
                }
                else
                {
                    Log.Warning($"装备槽 Slot{i} 未在ReferenceCollector中找到");
                }
            }
            
            Log.Info($"装备槽初始化完成，共{self.EquipmentSlots.Count}个槽位");
        }
        
        public static void RefreshEquipmentSlots(this UIMainComponent self)
        {
            // 直接使用本地装备数据，无需依赖Unit
            foreach (var kvp in self.EquipmentSlots)
            {
                int slotIndex = kvp.Key;
                UIEquipmentSlot slot = kvp.Value;
                if (slot != null)
                {
                    // 从本地装备字典获取装备
                    self.LocalEquipments.TryGetValue(slotIndex, out Equipment equipment);
                    slot.SetEquipment(equipment);
                }
            }
        }
        
        /// <summary>
        /// 请求玩家装备数据
        /// </summary>
        public static async ETTask RequestPlayerEquipments(this UIMainComponent self)
        {
            try
            {
                Scene root = self.Root();
                
                // 获取客户端发送组件
                ClientSenderComponent clientSenderComponent = root.GetComponent<ClientSenderComponent>();
                if (clientSenderComponent == null)
                {
                    Log.Error("ClientSenderComponent组件为空，无法请求装备数据");
                    return;
                }
                
                Log.Info("开始请求玩家装备数据");
                
                // 发送获取装备请求
                C2G_GetPlayerEquipments request = C2G_GetPlayerEquipments.Create();
                G2C_GetPlayerEquipments response = (G2C_GetPlayerEquipments)await clientSenderComponent.Call(request);
                
                if (response == null)
                {
                    Log.Error("获取装备数据响应为空");
                    return;
                }
                
                if (response.Error != ErrorCode.ERR_Success)
                {
                    Log.Error($"获取装备数据失败，错误码: {response.Error}, 消息: {response.Message}");
                    return;
                }
                
                // 清空现有装备数据
                self.LocalEquipments.Clear();
                
                // 处理服务端返回的装备数据
                if (response.Equipments != null && response.SlotIndexes != null)
                {
                    for (int i = 0; i < response.Equipments.Count && i < response.SlotIndexes.Count; i++)
                    {
                        EquipmentProto equipmentProto = response.Equipments[i];
                        int slotIndex = response.SlotIndexes[i];
                        
                        Equipment equipment = ConvertFromEquipmentProto(equipmentProto);
                        self.LocalEquipments[slotIndex] = equipment;
                        
                        Log.Info($"加载装备: {equipment.Name}, 槽位: {slotIndex}");
                    }
                    
                    Log.Info($"装备数据加载完成，共加载 {response.Equipments.Count} 件装备");
                }
                else
                {
                    Log.Info("玩家暂无装备数据");
                }
                
                // 刷新装备槽UI
                self.RefreshEquipmentSlots();
            }
            catch (System.Exception e)
            {
                Log.Error($"请求玩家装备数据异常: {e.Message}");
            }
        }
        
        // 新增装备到指定槽位的方法
        public static void SetEquipment(this UIMainComponent self, int slotIndex, Equipment equipment)
        {
            Log.Info($"UIMainComponent.SetEquipment: 槽位{slotIndex}, 装备: {equipment?.Name ?? "null"}");
            
            if (equipment != null)
            {
                self.LocalEquipments[slotIndex] = equipment;
            }
            else
            {
                self.LocalEquipments.Remove(slotIndex);
            }
            
            // 立即刷新对应的装备槽UI
            if (self.EquipmentSlots.TryGetValue(slotIndex, out var slotRef))
            {
                UIEquipmentSlot slot = slotRef;
                if (slot != null)
                {
                    Log.Info($"找到装备槽{slotIndex}，准备更新UI");
                    slot.SetEquipment(equipment);
                }
                else
                {
                    Log.Warning($"装备槽{slotIndex}为null");
                }
            }
            else
            {
                Log.Warning($"未找到装备槽{slotIndex}");
            }
        }
        
        
        // 获取指定槽位的装备
        public static Equipment GetEquipment(this UIMainComponent self, int slotIndex)
        {
            self.LocalEquipments.TryGetValue(slotIndex, out Equipment equipment);
            return equipment;
        }
        
        private static async ETTask OnGenerateEquipmentClick(this UIMainComponent self)
        {
            // 防重复点击
            if (self.IsGeneratingEquipment)
            {
                return;
            }
            
            self.IsGeneratingEquipment = true;
            
            try
            {
                Scene root = self.Root();
                
                // 使用ClientSenderComponent发送请求，这是ET框架的标准方式
                ClientSenderComponent clientSenderComponent = root.GetComponent<ClientSenderComponent>();
                if (clientSenderComponent == null)
                {
                    Log.Error("ClientSenderComponent组件为空，可能客户端还未连接到服务器");
                    return;
                }
                
                Log.Info("ClientSenderComponent状态正常，准备发送装备生成请求");
                
                // 发送装备生成请求到Gate服务器，Gate会转发到Map服务器
                C2G_GenerateEquipment request = C2G_GenerateEquipment.Create();
                Log.Info($"客户端发送装备生成请求，RpcId: {request.RpcId}");
                
                G2C_GenerateEquipment response = (G2C_GenerateEquipment)await clientSenderComponent.Call(request);
                
                if (response == null)
                {
                    Log.Error("服务端响应为空，可能服务器连接中断");
                    return;
                }
                
                if (response.Error != ErrorCode.ERR_Success)
                {
                    Log.Error($"服务端装备生成失败，错误码: {response.Error}, 消息: {response.Message}");
                    return;
                }
                
                // 从服务端响应获取装备数据
                Equipment newEquipment = ConvertFromEquipmentProto(response.Equipment);
                int slotIndex = response.SlotIndex;
                Equipment oldEquipment = self.GetEquipment(slotIndex);
                
                Log.Info($"收到服务端生成的装备: {newEquipment.Name}, 槽位: {slotIndex}");
            
                if (oldEquipment == null)
                {
                    // 首次装备 - 使用确认弹窗
                    // 先检查是否已存在，如果存在则先移除
                    UIComponent uiComponent = root.GetComponent<UIComponent>();
                    UI existingConfirmUI = uiComponent?.Get(UIType.UIEquipmentConfirm);
                    if (existingConfirmUI != null)
                    {
                        await UIHelper.Remove(root, UIType.UIEquipmentConfirm);
                    }
                    
                    await UIHelper.Create(root, UIType.UIEquipmentConfirm, UILayer.High);
                    UI confirmUI = uiComponent?.Get(UIType.UIEquipmentConfirm);
                    UIEquipmentConfirmComponent confirmComponent = confirmUI?.GetComponent<UIEquipmentConfirmComponent>();
                    confirmComponent?.ShowConfirm(newEquipment, slotIndex);
                }
                else
                {
                    // 装备替换 - 使用对比弹窗
                    // 先检查是否已存在，如果存在则先移除
                    UIComponent uiComponent = root.GetComponent<UIComponent>();
                    UI existingCompareUI = uiComponent?.Get(UIType.UIEquipmentCompare);
                    if (existingCompareUI != null)
                    {
                        await UIHelper.Remove(root, UIType.UIEquipmentCompare);
                    }
                    
                    await UIHelper.Create(root, UIType.UIEquipmentCompare, UILayer.High);
                    UI compareUI = uiComponent?.Get(UIType.UIEquipmentCompare);
                    UIEquipmentCompareComponent compareComponent = compareUI?.GetComponent<UIEquipmentCompareComponent>();
                    compareComponent?.ShowCompare(oldEquipment, newEquipment, slotIndex);
                }
            }
            catch (System.Exception e)
            {
                Log.Error($"装备生成请求失败: {e.Message}");
            }
            finally
            {
                self.IsGeneratingEquipment = false;
            }
        }
        
        private static Equipment ConvertFromEquipmentProto(EquipmentProto proto)
        {
            return new Equipment
            {
                Id = proto.Id,
                Name = proto.Name,
                Level = proto.Level,
                Attack = proto.Attack,
                Defense = proto.Defense,
                Health = proto.Health,
                Quality = proto.Quality,
                SlotType = proto.EquipType,
                Icon = "icon_equipment",
                Description = "从服务端获取的装备"
            };
        }
        
        private static UnityEngine.Color GetQualityColor(int quality)
        {
            switch (quality)
            {
                case 0: return UnityEngine.Color.white;
                case 1: return UnityEngine.Color.green;
                case 2: return UnityEngine.Color.blue;
                case 3: return new UnityEngine.Color(0.5f, 0, 0.5f);
                case 4: return new UnityEngine.Color(1f, 0.5f, 0);
                default: return UnityEngine.Color.gray;
            }
        }
    }
}