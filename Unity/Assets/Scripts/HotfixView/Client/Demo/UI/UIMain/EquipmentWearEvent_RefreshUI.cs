namespace ET.Client
{
    [Event(SceneType.Demo)]
    public class EquipmentWearEvent_RefreshUI : AEvent<Scene, EquipmentWearEvent>
    {
        protected override async ETTask Run(Scene scene, EquipmentWearEvent args)
        {
            Log.Info($"装备事件：在槽位{args.SlotIndex}装备了{args.Equipment?.Name ?? "空"}");
            
            UIComponent uiComponent = scene.GetComponent<UIComponent>();
            if (uiComponent == null)
            {
                Log.Error("UIComponent为null");
                return;
            }
            
            UI mainUI = uiComponent.Get(UIType.UIMain);
            if (mainUI == null)
            {
                Log.Error("UIMain不存在");
                return;
            }
            
            UIMainComponent mainComponent = mainUI.GetComponent<UIMainComponent>();
            if (mainComponent == null)
            {
                Log.Error("UIMainComponent不存在");
                return;
            }
            
            Log.Info("准备调用SetEquipment更新UI");
            mainComponent.SetEquipment(args.SlotIndex, args.Equipment);
            await ETTask.CompletedTask;
        }
    }
}