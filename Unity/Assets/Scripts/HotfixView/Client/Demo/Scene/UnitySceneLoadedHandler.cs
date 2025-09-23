using UnityEngine.SceneManagement;

namespace ET.Client
{
    [EntitySystemOf(typeof(UnitySceneLoadedComponent))]
    [FriendOf(typeof(UnitySceneLoadedComponent))]
    public static partial class UnitySceneLoadedComponentSystem 
    {
        [EntitySystem]
        private static void Awake(this UnitySceneLoadedComponent self)
        {
            self.lastSceneName = SceneManager.GetActiveScene().name;
            Log.Info($"UnitySceneLoadedComponent: 初始化，当前场景: {self.lastSceneName}");
        }

        [EntitySystem]
        private static void Destroy(this UnitySceneLoadedComponent self)
        {
            Log.Info("UnitySceneLoadedComponent: 组件销毁");
        }
        
        [EntitySystem]
        private static void Update(this UnitySceneLoadedComponent self)
        {
            var currentScene = SceneManager.GetActiveScene();
            
            if (currentScene.name != self.lastSceneName && currentScene.isLoaded)
            {
                Log.Info($"UnitySceneLoadedComponent: 场景切换检测到 {self.lastSceneName} -> {currentScene.name}");
                self.lastSceneName = currentScene.name;
                
                if (currentScene.name == "Login" && !self.hasProcessedLogin)
                {
                    self.hasProcessedLogin = true;
                    Log.Info("UnitySceneLoadedComponent: 检测到Login场景加载完成，准备创建UISelectServer");
                    self.CreateUISelectServer().Coroutine();
                }
                else if (currentScene.name == "MainScene")
                {
                    Log.Info("UnitySceneLoadedComponent: 检测到MainScene场景加载完成，开始角色创建流程");
                    self.HandleMainSceneLoaded().Coroutine();
                }
            }
        }
        
        private static async ETTask CreateUISelectServer(this UnitySceneLoadedComponent self)
        {
            try
            {
                // 获取当前ET Scene
                Scene etScene = self.Scene();
                
                // 等待一帧确保Unity场景完全初始化
                await etScene.GetComponent<TimerComponent>().WaitFrameAsync();
                
                // 创建UISelectServer界面
                Log.Info("UnitySceneLoadedComponent: 开始创建UISelectServer界面");
                UI ui = await UIHelper.Create(etScene, UIType.UISelectServer, UILayer.High);
                Log.Info($"UnitySceneLoadedComponent: UISelectServer界面创建成功, UI对象: {ui != null}, GameObject: {ui?.GameObject?.name}");
                
                Log.Info("UnitySceneLoadedComponent: UISelectServer界面创建完成");
            }
            catch (System.Exception e)
            {
                Log.Error($"UnitySceneLoadedComponent: 创建UISelectServer界面失败: {e.Message}");
            }
        }
        
        private static async ETTask HandleMainSceneLoaded(this UnitySceneLoadedComponent self)
        {
            try
            {
                // 获取当前ET Scene
                Scene etScene = self.Scene();
                
                // 等待一帧确保Unity场景完全初始化
                await etScene.GetComponent<TimerComponent>().WaitFrameAsync();
                
                Log.Info("UnitySceneLoadedComponent: MainScene初始化完成，开始发送C2G_EnterMap请求");
                
                // 发送C2G_EnterMap消息来触发角色创建
                await EnterMapHelper.EnterMapAsync(etScene);
                
                Log.Info("UnitySceneLoadedComponent: 角色创建流程已启动");
            }
            catch (System.Exception e)
            {
                Log.Error($"UnitySceneLoadedComponent: MainScene处理失败: {e.Message}");
            }
        }
    }
}