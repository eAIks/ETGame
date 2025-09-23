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
                self.CancelButton.onClick.AddListener(() => { self.OnCancelClick().Coroutine(); });
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
                self.QualityText.text = GetQualityName(equipment.Quality);
                self.QualityText.color = GetQualityColor(equipment.Quality);
            }
            
            if (self.IconImage != null)
            {
                self.IconImage.color = GetQualityColor(equipment.Quality);
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
        
        private static async ETTask OnCancelClick(this UIEquipmentConfirmComponent self)
        {
            Scene root = self.Root();
            
            try
            {
                // 发送取消临时装备消息到服务端
                ClientSenderComponent clientSenderComponent = root.GetComponent<ClientSenderComponent>();
                if (clientSenderComponent != null)
                {
                    C2G_CancelTempEquipment request = C2G_CancelTempEquipment.Create();
                    
                    Log.Info("客户端发送取消临时装备请求");
                    
                    G2C_CancelTempEquipment response = (G2C_CancelTempEquipment)await clientSenderComponent.Call(request);
                    
                    if (response != null && response.Error == ErrorCode.ERR_Success)
                    {
                        Log.Info("取消临时装备成功");
                    }
                    else
                    {
                        Log.Warning($"取消临时装备失败: {response?.Message}");
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"取消临时装备请求异常: {e}");
            }
            
            await UIHelper.Remove(root, UIType.UIEquipmentConfirm);
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