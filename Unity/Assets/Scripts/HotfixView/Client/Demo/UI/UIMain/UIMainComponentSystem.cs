using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [EntitySystemOf(typeof(UIMainComponent))]
    [FriendOf(typeof(UIMainComponent))]
    [FriendOf(typeof(UIEquipmentSlot))]
    [FriendOf(typeof(UIRealmLevel))]
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
                self.GenerateEquipmentButton.interactable = true;
                
                var buttonCanvasGroup = self.GenerateEquipmentButton.GetComponent<CanvasGroup>();
                if (buttonCanvasGroup != null)
                {
                    buttonCanvasGroup.blocksRaycasts = true;
                    buttonCanvasGroup.interactable = true;
                }
                
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
            self.InitRealmLevel();
        }
        
        private static void InitEquipmentSlots(this UIMainComponent self)
        {
            ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
            
            for (int i = 0; i < 9; i++)
            {
                GameObject slotGo = rc.Get<GameObject>($"Slot{i}");
                if (slotGo != null)
                {
                    UIEquipmentSlot slot = self.AddChild<UIEquipmentSlot>();
                    slot.SlotIndex = i;
                    slot.GameObject = slotGo;
                    slot.InitializeComponents();
                    self.EquipmentSlots[i] = slot;
                }
            }
        }
        
        private static void InitRealmLevel(this UIMainComponent self)
        {
            ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
            
            if (rc == null)
            {
                return;
            }
            
            
            GameObject realmLevelGo = rc.Get<GameObject>("UIRealmLevel");
            
            if (realmLevelGo != null)
            {
                UIRealmLevel realmLevel = self.AddChild<UIRealmLevel>();
                realmLevel.GameObject = realmLevelGo;
                realmLevel.InitializeComponents();
                self.UIRealmLevel = realmLevel;
            }
        }
        
        public static void RefreshEquipmentSlots(this UIMainComponent self)
        {
            foreach (var kvp in self.EquipmentSlots)
            {
                int slotIndex = kvp.Key;
                UIEquipmentSlot slot = kvp.Value;
                if (slot != null)
                {
                    self.LocalEquipments.TryGetValue(slotIndex, out Equipment equipment);
                    slot.SetEquipment(equipment);
                }
            }
        }
        
        public static void RefreshRealmLevel(this UIMainComponent self)
        {
            UIRealmLevel realmLevel = self.UIRealmLevel;
            if (realmLevel != null)
            {
                realmLevel.SetRealmInfo(self.LocalRealmInfo);
            }
        }
        
        public static async ETTask RequestPlayerRealm(this UIMainComponent self)
        {
            
            try
            {
                Scene root = self.Root();
                
                ClientSenderComponent clientSenderComponent = root.GetComponent<ClientSenderComponent>();
                if (clientSenderComponent == null)
                {
                    return;
                }
                
                C2G_GetPlayerRealm request = C2G_GetPlayerRealm.Create();
                G2C_GetPlayerRealm response = (G2C_GetPlayerRealm)await clientSenderComponent.Call(request);
                
                if (response?.Error != ErrorCode.ERR_Success)
                {
                    return;
                }
                
                self.LocalRealmInfo = response.RealmInfo;
                self.RefreshRealmLevel();
            }
            catch (System.Exception e)
            {
                Log.Error($"请求玩家境界数据异常: {e.Message}");
            }
        }
        
        public static async ETTask RequestPlayerEquipments(this UIMainComponent self)
        {
            try
            {
                Scene root = self.Root();
                
                ClientSenderComponent clientSenderComponent = root.GetComponent<ClientSenderComponent>();
                if (clientSenderComponent == null)
                {
                    return;
                }
                
                C2G_GetPlayerEquipments request = C2G_GetPlayerEquipments.Create();
                G2C_GetPlayerEquipments response = (G2C_GetPlayerEquipments)await clientSenderComponent.Call(request);
                
                if (response?.Error != ErrorCode.ERR_Success)
                {
                    return;
                }
                
                self.LocalEquipments.Clear();
                
                if (response.Equipments != null && response.SlotIndexes != null)
                {
                    for (int i = 0; i < response.Equipments.Count && i < response.SlotIndexes.Count; i++)
                    {
                        EquipmentProto equipmentProto = response.Equipments[i];
                        int slotIndex = response.SlotIndexes[i];
                        
                        Equipment equipment = ConvertFromEquipmentProto(equipmentProto);
                        self.LocalEquipments[slotIndex] = equipment;
                    }
                }
                
                self.RefreshEquipmentSlots();
            }
            catch (System.Exception e)
            {
                Log.Error($"请求玩家装备数据异常: {e.Message}");
            }
        }
        
        public static void SetEquipment(this UIMainComponent self, int slotIndex, Equipment equipment)
        {
            
            if (equipment != null)
            {
                self.LocalEquipments[slotIndex] = equipment;
            }
            else
            {
                self.LocalEquipments.Remove(slotIndex);
            }
            
            if (self.EquipmentSlots.TryGetValue(slotIndex, out var slotRef))
            {
                UIEquipmentSlot slot = slotRef;
                if (slot != null)
                {
                    slot.SetEquipment(equipment);
                }
            }
        }
        
        public static Equipment GetEquipment(this UIMainComponent self, int slotIndex)
        {
            self.LocalEquipments.TryGetValue(slotIndex, out Equipment equipment);
            return equipment;
        }
        
        private static async ETTask OnGenerateEquipmentClick(this UIMainComponent self)
        {
            if (self.IsGeneratingEquipment)
            {
                return;
            }
            
            self.IsGeneratingEquipment = true;
            
            try
            {
                Scene root = self.Root();
                
                ClientSenderComponent clientSenderComponent = root.GetComponent<ClientSenderComponent>();
                if (clientSenderComponent == null)
                {
                    return;
                }
                
                C2G_GenerateEquipment request = C2G_GenerateEquipment.Create();
                
                G2C_GenerateEquipment response = (G2C_GenerateEquipment)await clientSenderComponent.Call(request);
                
                if (response?.Error != ErrorCode.ERR_Success)
                {
                    return;
                }
                
                Equipment newEquipment = ConvertFromEquipmentProto(response.Equipment);
                int slotIndex = response.SlotIndex;
                Equipment oldEquipment = self.GetEquipment(slotIndex);
            
                if (oldEquipment == null)
                {
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
                Description = "从服务端获取的装备",
                Color = proto.Color
            };
        }
        
       
    }
}