using UnityEngine;

namespace ET.Client
{
    // 临时版本 - 用UILobby的预制体来测试UI创建是否工作
    [UIEvent(UIType.UISelectServer)]
    public class UISelectServerEvent : AUIEvent
    {
        public override async ETTask<UI> OnCreate(UIComponent uiComponent, UILayer uiLayer)
        {
            Log.Info("UISelectServerEvent: 使用临时方案创建UI");
            
            try
            {
                // 使用正确的UISelectServer预制体
                string assetsName = $"Assets/Bundles/UI/Demo/UISelectServer.prefab";
                Log.Info($"UISelectServerEvent: 使用UISelectServer预制体: {assetsName}");
                
                GameObject bundleGameObject = await uiComponent.Scene().GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>(assetsName);
                if (bundleGameObject == null)
                {
                    Log.Error($"UISelectServerEvent: 预制体加载失败: {assetsName}");
                    throw new System.Exception($"Failed to load prefab: {assetsName}");
                }
                
                Transform parentLayer = uiComponent.UIGlobalComponent.GetLayer((int)uiLayer);
                Log.Info($"UISelectServerEvent: 父级Layer: {parentLayer?.name}");
                
                GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject, parentLayer);
                gameObject.name = "UISelectServer"; // 使用正确的名称
                Log.Info($"UISelectServerEvent: GameObject创建成功: {gameObject.name}, active: {gameObject.activeInHierarchy}");
                
                // prefab已经包含了正确的UI结构和ReferenceCollector，无需再创建
                Log.Info("UISelectServerEvent: 使用prefab中预设的UI结构");
                
                UI ui = uiComponent.AddChild<UI, string, GameObject>(UIType.UISelectServer, gameObject);
                // 现在添加UISelectServerComponent以启用系统
                ui.AddComponent<UISelectServerComponent>();
                Log.Info("UISelectServerEvent: UI创建成功");
                return ui;
            }
            catch (System.Exception e)
            {
                Log.Error($"UISelectServerEvent: 创建UI失败: {e.Message}");
                Log.Error($"异常堆栈: {e.StackTrace}");
                throw;
            }
        }

        public override void OnRemove(UIComponent uiComponent)
        {
            Log.Info("UISelectServerEvent: UI移除");
        }
    }
}