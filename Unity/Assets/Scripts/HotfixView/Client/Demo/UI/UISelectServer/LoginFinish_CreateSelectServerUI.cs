namespace ET.Client
{
    [Event(SceneType.Demo)]
    public class LoginFinish_CreateSelectServerUI : AEvent<Scene, LoginFinish>
    {
        protected override async ETTask Run(Scene scene, LoginFinish args)
        {
            Log.Info("LoginFinish_CreateSelectServerUI: 登录成功");
            
            try
            {
                // 修复AudioListener重复问题
                FixAudioListenerIssue();
                
                // 修复EventSystem重复问题
                FixEventSystemIssue();
                
                // 直接创建服务器选择界面，避免场景切换导致SessionComponent丢失
                Log.Info("LoginFinish_CreateSelectServerUI: 直接创建服务器选择界面，保持网络连接");
                await UIHelper.Create(scene, UIType.UISelectServer, UILayer.Mid);
                
                // 注释掉场景切换逻辑，因为它会导致SessionComponent丢失
                /*
                if (loginSceneExists)
                {
                    Log.Info("LoginFinish_CreateSelectServerUI: 跳转到Login场景");
                    UnityEngine.SceneManagement.SceneManager.LoadScene("Login");
                    
                    // 跳转后UISelectServer的创建将由LoginSceneLoaded_CreateSelectServerUI处理
                    Log.Info("LoginFinish_CreateSelectServerUI: 场景跳转完成，UISelectServer将在场景加载后自动创建");
                }
                else
                {
                    Log.Warning("LoginFinish_CreateSelectServerUI: Login场景未添加到Build Settings");
                    Log.Info("LoginFinish_CreateSelectServerUI: 使用备选方案 - 直接创建服务器选择界面");
                    await UIHelper.Create(scene, UIType.UISelectServer, UILayer.Mid);
                }
                */
            }
            catch (System.Exception e)
            {
                Log.Error($"LoginFinish_CreateSelectServerUI: 处理失败: {e.Message}");
                Log.Info("LoginFinish_CreateSelectServerUI: 回退到切换MainScene");
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
            }
        }
        
        /// <summary>
        /// 修复AudioListener重复问题
        /// </summary>
        private static void FixAudioListenerIssue()
        {
            // 查找所有AudioListener
            UnityEngine.AudioListener[] audioListeners = UnityEngine.Object.FindObjectsOfType<UnityEngine.AudioListener>();
            
            if (audioListeners.Length > 1)
            {
                Log.Warning($"LoginFinish_CreateSelectServerUI: 发现 {audioListeners.Length} 个AudioListener，保留第一个，移除其余");
                
                // 保留第一个，移除其余
                for (int i = 1; i < audioListeners.Length; i++)
                {
                    Log.Info($"LoginFinish_CreateSelectServerUI: 移除多余的AudioListener: {audioListeners[i].gameObject.name}");
                    UnityEngine.Object.Destroy(audioListeners[i]);
                }
            }
            else if (audioListeners.Length == 0)
            {
                Log.Warning("LoginFinish_CreateSelectServerUI: 场景中没有AudioListener，这可能会导致音频问题");
            }
            else
            {
                Log.Info("LoginFinish_CreateSelectServerUI: AudioListener数量正常");
            }
        }
        
        /// <summary>
        /// 修复EventSystem重复问题
        /// </summary>
        private static void FixEventSystemIssue()
        {
            // 查找所有EventSystem
            UnityEngine.EventSystems.EventSystem[] eventSystems = UnityEngine.Object.FindObjectsOfType<UnityEngine.EventSystems.EventSystem>();
            
            if (eventSystems.Length > 1)
            {
                Log.Warning($"LoginFinish_CreateSelectServerUI: 发现 {eventSystems.Length} 个EventSystem，保留第一个，移除其余");
                
                // 保留第一个，移除其余
                for (int i = 1; i < eventSystems.Length; i++)
                {
                    Log.Info($"LoginFinish_CreateSelectServerUI: 移除多余的EventSystem: {eventSystems[i].gameObject.name}");
                    UnityEngine.Object.Destroy(eventSystems[i].gameObject);
                }
            }
            else if (eventSystems.Length == 0)
            {
                Log.Warning("LoginFinish_CreateSelectServerUI: 场景中没有EventSystem，这可能会导致UI交互问题");
            }
            else
            {
                Log.Info("LoginFinish_CreateSelectServerUI: EventSystem数量正常");
            }
        }
    }
}