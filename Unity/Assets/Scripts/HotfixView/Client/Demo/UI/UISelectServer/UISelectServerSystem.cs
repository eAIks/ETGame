using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [EntitySystemOf(typeof(UISelectServerComponent))]
    [FriendOf(typeof(UISelectServerComponent))]
    [FriendOf(typeof(ClientSenderComponent))]
    public static partial class UISelectServerSystem
    {
        [EntitySystem]
        private static void Awake(this UISelectServerComponent self)
        {
            Log.Info("UISelectServerSystem: Awake");
            
            // 修复AudioListener重复问题
            FixAudioListenerIssue();
            
            // 修复EventSystem重复问题
            FixEventSystemIssue();
            
            // 获取UI元素引用
            ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
            Log.Info($"UISelectServerSystem: ReferenceCollector获取结果: {rc != null}");
            
            if (rc != null)
            {
                Log.Info($"UISelectServerSystem: ReferenceCollector数据项数量: {rc.data.Count}");
                foreach (var item in rc.data)
                {
                    Log.Info($"UISelectServerSystem: ReferenceCollector项: {item.key} -> {item.gameObject?.name}");
                }
                
                self.serverNameText = rc.Get<GameObject>("ServerName");
                self.selectServerBtn = rc.Get<GameObject>("SelectServerBtn");
                self.enterGameBtn = rc.Get<GameObject>("EnterGameBtn");

                Log.Info($"UISelectServerSystem: 绑定结果 - ServerName: {self.serverNameText?.name}, SelectServerBtn: {self.selectServerBtn?.name}, EnterGameBtn: {self.enterGameBtn?.name}");
                
                if (self.serverNameText != null)
                {
                    var textComp = self.serverNameText.GetComponent<Text>();
                    Log.Info($"UISelectServerSystem: ServerName Text组件: {textComp != null}");
                    if (textComp != null)
                    {
                        textComp.text = "1区";
                        Log.Info("UISelectServerSystem: ServerName文本已更新为'1区'");
                    }
                }
                else
                {
                    Log.Error("UISelectServerSystem: ServerName GameObject为null");
                }
                // 绑定按钮事件
                if (self.selectServerBtn != null)
                {
                    self.selectServerBtn.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        self.OnSelectServerClick().Coroutine();
                    });
                }
                
                if (self.enterGameBtn != null)
                {
                    self.enterGameBtn.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        self.OnEnterGameClick().Coroutine();
                    });
                }
            }
            
            // 初始化服务器列表 - 异步等待完成
            Log.Info("UISelectServerSystem: 准备调用InitializeServerList");
            self.InitializeServerListAsync().Coroutine();
        }

        [EntitySystem]
        private static void Destroy(this UISelectServerComponent self)
        {
            Log.Info("UISelectServerSystem: Destroy");
        }
        
        /// <summary>
        /// 处理服务器选择事件，更新UI显示
        /// </summary>
        public static void OnServerSelected(this UISelectServerComponent self, ServerInfo server)
        {
            if (server == null) return;
            
            Log.Info($"UISelectServerSystem: 收到服务器选择事件: {server.ServerName}");
            
            // 更新当前选中的服务器
            self.currentSelectedServer = server;
            
            // 更新UI显示
            self.UpdateUI();
        }
        
        /// <summary>
        /// 异步初始化服务器列表包装方法
        /// </summary>
        private static async ETTask InitializeServerListAsync(this UISelectServerComponent self)
        {
            try
            {
                await self.InitializeServerList();
            }
            catch (System.Exception e)
            {
                Log.Error($"UISelectServerSystem: 初始化异常: {e.Message}");
                Log.Error($"UISelectServerSystem: 异常堆栈: {e.StackTrace}");
                // 异常情况下也使用默认数据
                self.CreateDefaultServerData();
            }
            finally
            {
                self.isInitialized = true;
                Log.Info("UISelectServerSystem: 初始化完成，设置isInitialized为true");
            }
        }
        
        /// <summary>
        /// 初始化服务器列表
        /// </summary>
        private static async ETTask InitializeServerList(this UISelectServerComponent self)
        {
            Log.Info("UISelectServerSystem: 开始初始化服务器列表");
            
            try
            {
                // 等待一帧确保UI完全初始化
                await self.Scene().GetComponent<TimerComponent>().WaitFrameAsync();
                
                Scene scene = self.Scene();
                ServerListComponent serverListComp = scene.GetComponent<ServerListComponent>();
                if (serverListComp == null)
                {
                    serverListComp = scene.AddComponent<ServerListComponent>();
                }
                
                // 首先尝试从服务器加载服务器列表
                Log.Info("UISelectServerSystem: 尝试从服务器获取服务器列表");
                bool success = await serverListComp.LoadServerListFromServer();
                
                Log.Info($"UISelectServerSystem: 从服务器获取结果: success={success}, ZoneList数量={serverListComp.ZoneList.Count}");
                if (success && serverListComp.ZoneList.Count > 0)
                {
                    Log.Info($"UISelectServerSystem: 成功从服务器获取到 {serverListComp.ZoneList.Count} 个区组的数据");
                    
                    // 获取默认选中的服务器
                    ServerInfo defaultServer = self.GetDefaultSelectedServer(serverListComp);
                    if (defaultServer != null)
                    {
                        serverListComp.CurrentServer = defaultServer;
                        self.currentSelectedServer = defaultServer;
                        Log.Info($"UISelectServerSystem: 默认选中服务器: {defaultServer.ServerName}");
                    }
                }
                else
                {
                    Log.Warning("UISelectServerSystem: 从服务器获取数据失败，使用默认测试数据");
                    self.CreateDefaultServerData();
                }
                
                // 更新UI显示
                self.UpdateUI();
                
            }
            catch (System.Exception e)
            {
                Log.Error($"UISelectServerSystem: 初始化服务器列表异常 {e.Message}");
                Log.Error($"UISelectServerSystem: 异常堆栈: {e.StackTrace}");
                
                // 即使出错也要尝试创建默认数据
                try 
                {
                    self.CreateDefaultServerData();
                }
                catch (System.Exception e2)
                {
                    Log.Error($"UISelectServerSystem: 创建默认数据也失败 {e2.Message}");
                    Log.Error($"UISelectServerSystem: 创建默认数据异常堆栈: {e2.StackTrace}");
                }
            }
        }
        
        
        /// <summary>
        /// 获取默认选中的服务器
        /// </summary>
        private static ServerInfo GetDefaultSelectedServer(this UISelectServerComponent self, ServerListComponent serverListComp)
        {
            
            // 其次返回推荐服务器
            foreach (var zone in serverListComp.ZoneList)
            {
                foreach (var server in zone.ServerList)
                {
                    if (server.IsRecommend)
                    {
                        Log.Info($"UISelectServerSystem: 使用推荐服务器: {server.ServerName}");
                        return server;
                    }
                }
            }
            
            // 最后返回最新服务器
            foreach (var zone in serverListComp.ZoneList)
            {
                foreach (var server in zone.ServerList)
                {
                    if (server.IsNew)
                    {
                        Log.Info($"UISelectServerSystem: 使用最新服务器: {server.ServerName}");
                        return server;
                    }
                }
            }
            
            // 如果都没有，返回第一个可用服务器
            foreach (var zone in serverListComp.ZoneList)
            {
                foreach (var server in zone.ServerList)
                {
                    if (server.Status != ServerStatus.Maintenance)
                    {
                        Log.Info($"UISelectServerSystem: 使用第一个可用服务器: {server.ServerName}");
                        return server;
                    }
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// 创建默认服务器数据（用于测试或网络失败时）
        /// </summary>
        private static void CreateDefaultServerData(this UISelectServerComponent self)
        {
            Log.Info("UISelectServerSystem: 创建默认服务器数据");
            
            Scene scene = self.Scene();
            ServerListComponent serverListComp = scene.GetComponent<ServerListComponent>();
            if (serverListComp == null)
            {
                serverListComp = scene.AddComponent<ServerListComponent>();
            }
            
            // 清空现有数据
            serverListComp.ZoneList.Clear();
            
            // 创建多个区组
            // 区组1 - 最新区组
            ServerZone newZone = serverListComp.AddChild<ServerZone>();
            newZone.ZoneId = 1;
            newZone.ZoneName = "璀璨星河";
            self.CreateServersForZone(newZone, 1, true);
            serverListComp.ZoneList.Add(newZone);
            
            // 区组2 - 热门区组
            ServerZone hotZone = serverListComp.AddChild<ServerZone>();
            hotZone.ZoneId = 2;
            hotZone.ZoneName = "烈焰荣耀";
            self.CreateServersForZone(hotZone, 2, false);
            serverListComp.ZoneList.Add(hotZone);
            
            // 区组3 - 经典区组
            ServerZone classicZone = serverListComp.AddChild<ServerZone>();
            classicZone.ZoneId = 3;
            classicZone.ZoneName = "永恒传说";
            self.CreateServersForZone(classicZone, 3, false);
            serverListComp.ZoneList.Add(classicZone);
            
            // 新用户默认选择最新区组的推荐服务器
            ServerInfo recommendedServer = self.GetRecommendedServerForNewUser(serverListComp.ZoneList);
            serverListComp.CurrentServer = recommendedServer;
            self.currentSelectedServer = recommendedServer;
            
            Log.Info($"UISelectServerSystem: 为新用户推荐服务器: {recommendedServer?.ServerName} (区组: {newZone.ZoneName})");
            Log.Info($"UISelectServerSystem: 默认数据创建完成，最终ZoneList数量: {serverListComp.ZoneList.Count}");
            
            // 更新UI显示
            self.UpdateUI();
        }
        
        /// <summary>
        /// 为区组创建服务器
        /// </summary>
        private static void CreateServersForZone(this UISelectServerComponent self, ServerZone zone, int zoneId, bool isNewZone)
        {
            // 每个区组创建3-5个服务器
            string[] serverNames = zoneId switch
            {
                1 => new[] { "璀璨星河1区", "璀璨星河1区", "璀璨星河1区", "璀璨星河1区" },
                2 => new[] { "烈焰荣耀1区", "烈焰荣耀1区", "烈焰荣耀1区", "烈焰荣耀1区", "烈焰荣耀1区" },
                3 => new[] { "永恒传说1区", "永恒传说1区", "永恒传说1区" },
                _ => new[] { "默认服务器" }
            };
            
            for (int i = 0; i < serverNames.Length; i++)
            {
                ServerInfo server = zone.AddChild<ServerInfo>();
                server.ServerId = zoneId * 100 + i + 1;
                server.ServerName = serverNames[i];
                server.ServerIP = "127.0.0.1";
                server.ServerPort = 10002 + server.ServerId;
                server.ZoneId = zoneId;
                server.OpenTime = System.DateTime.Now.AddDays(-(serverNames.Length - i - 1));
                
                // 设置服务器状态
                server.Status = i switch
                {
                    0 => ServerStatus.Smooth,
                    1 => ServerStatus.Normal, 
                    2 => ServerStatus.Crowded,
                    _ => ServerStatus.Normal
                };
                
                // 设置在线人数
                server.MaxCount = 2000;
                server.OnlineCount = server.Status switch
                {
                    ServerStatus.Smooth => UnityEngine.Random.Range(50, 500),
                    ServerStatus.Normal => UnityEngine.Random.Range(500, 1200),
                    ServerStatus.Crowded => UnityEngine.Random.Range(1200, 1800),
                    _ => 100
                };
                
                // 新区组的第一个服务器设为新服和推荐
                if (isNewZone && i == 0)
                {
                    server.IsNew = true;
                    server.IsRecommend = true;
                }
                else if (i == 0) // 每个区组的第一个服务器为推荐
                {
                    server.IsRecommend = true;
                }
                
                zone.ServerList.Add(server);
            }
        }
        
        /// <summary>
        /// 获取新用户推荐的服务器（最新区组的推荐服务器）
        /// </summary>
        private static ServerInfo GetRecommendedServerForNewUser(this UISelectServerComponent self, List<ServerZone> zoneList)
        {
            // 新用户优先推荐最新区组的推荐服务器
            foreach (var zone in zoneList)
            {
                foreach (var server in zone.ServerList)
                {
                    if (server.IsNew && server.IsRecommend)
                    {
                        return server;
                    }
                }
            }
            
            // 如果没有新服推荐，返回任意推荐服务器
            foreach (var zone in zoneList)
            {
                foreach (var server in zone.ServerList)
                {
                    if (server.IsRecommend)
                    {
                        return server;
                    }
                }
            }
            
            // 最后返回第一个可用服务器
            return zoneList.FirstOrDefault()?.ServerList?.FirstOrDefault();
        }
        
        /// <summary>
        /// 更新UI显示
        /// </summary>
        public static void UpdateUI(this UISelectServerComponent self)
        {
            ServerInfo server = null;
            
            try
            {
                Log.Info("UISelectServerSystem: UpdateUI开始执行");
                
                // 安全地获取EntityRef值
                try
                {
                    server = self.currentSelectedServer;
                    Log.Info($"UISelectServerSystem: 成功获取currentSelectedServer: {server?.ServerName ?? "null"}");
                }
                catch (System.Exception ex)
                {
                    Log.Error($"UISelectServerSystem: 获取currentSelectedServer失败: {ex.Message}");
                    return;
                }
                
                if (server == null) 
                {
                    Log.Warning("UISelectServerSystem: currentSelectedServer为null，无法更新UI");
                    return;
                }
                
                Log.Info($"UISelectServerSystem: 更新UI显示服务器信息: {server.ServerName}");
                
                // 更新服务器名称
                if (self.serverNameText != null)
                {
                    Text nameText = self.serverNameText.GetComponent<Text>();
                    if (nameText != null)
                    {
                        nameText.text = server.ServerName;
                        Log.Info($"UISelectServerSystem: 服务器名称已更新为: {server.ServerName}");
                    }
                    else
                    {
                        Log.Warning("UISelectServerSystem: serverNameText的Text组件为null");
                    }
                }
                else
                {
                    Log.Warning("UISelectServerSystem: serverNameText GameObject为null");
                }
                
                // 更新服务器状态
                if (self.serverStatusText != null)
                {
                    Text statusText = self.serverStatusText.GetComponent<Text>();
                    if (statusText != null)
                    {
                        statusText.text = self.GetStatusText(server.Status);
                        statusText.color = self.GetStatusColor(server.Status);
                    }
                }
                
                // 更新在线人数
                if (self.onlineCountText != null)
                {
                    Text countText = self.onlineCountText.GetComponent<Text>();
                    if (countText != null)
                    {
                        countText.text = $"{server.OnlineCount}/{server.MaxCount}";
                    }
                }
                
                // 显示/隐藏推荐标签
                if (self.recommendTag != null)
                {
                    self.recommendTag.SetActive(server.IsRecommend);
                }
                
                // 显示/隐藏新服标签
                if (self.newTag != null)
                {
                    self.newTag.SetActive(server.IsNew);
                }
            }
            catch (System.Exception e)
            {
                Log.Error($"UISelectServerSystem: UpdateUI失败: {e.Message}");
                Log.Error($"UISelectServerSystem: UpdateUI异常堆栈: {e.StackTrace}");
            }
        }
        
        /// <summary>
        /// 获取状态文本
        /// </summary>
        private static string GetStatusText(this UISelectServerComponent self, ServerStatus status)
        {
            return status switch
            {
                ServerStatus.Maintenance => "维护中",
                ServerStatus.Smooth => "流畅",
                ServerStatus.Normal => "良好",
                ServerStatus.Crowded => "拥挤",
                ServerStatus.Full => "爆满",
                _ => "未知"
            };
        }
        
        /// <summary>
        /// 获取状态颜色
        /// </summary>
        private static Color GetStatusColor(this UISelectServerComponent self, ServerStatus status)
        {
            return status switch
            {
                ServerStatus.Maintenance => Color.gray,
                ServerStatus.Smooth => Color.green,
                ServerStatus.Normal => Color.yellow,
                ServerStatus.Crowded => new Color(1f, 0.5f, 0f), // 橙色
                ServerStatus.Full => Color.red,
                _ => Color.white
            };
        }
        
        /// <summary>
        /// 选择服务器按钮点击
        /// </summary>
        private static async ETTask OnSelectServerClick(this UISelectServerComponent self)
        {
            Log.Info("UISelectServerSystem: OnSelectServerClick被调用");
            
            // 检查初始化状态
            if (!self.isInitialized)
            {
                Log.Warning("UISelectServerSystem: 初始化尚未完成，请稍后再试");
                return;
            }
            
            try
            {
                // 检查当前是否有可用的服务器数据
                Scene scene = self.Scene();
                ServerListComponent serverListComp = scene.GetComponent<ServerListComponent>();
                if (serverListComp == null)
                {
                    Log.Error("UISelectServerSystem: ServerListComponent未初始化，无法选择服务器");
                    return;
                }

                Log.Info($"UISelectServerSystem: 当前ServerListComponent.ZoneList数量: {serverListComp.ZoneList.Count}");
                if (serverListComp.ZoneList.Count == 0)
                {
                    Log.Warning("UISelectServerSystem: 没有可用的服务器数据，尝试重新获取");
                    
                    // 尝试重新获取服务器列表
                    bool success = await serverListComp.LoadServerListFromServer();
                    if (!success || serverListComp.ZoneList.Count == 0)
                    {
                        Log.Error("UISelectServerSystem: 获取服务器列表失败");
                        return;
                    }
                }

                Log.Info("UISelectServerSystem: 准备创建UIServerSelect界面");
                // 创建服务器选择界面
                UI serverSelectUI = await UIHelper.Create(self.Scene(), UIType.UIServerSelect, UILayer.High);
                Log.Info($"UISelectServerSystem: UIServerSelect界面创建完成: {serverSelectUI?.GameObject?.name ?? "null"}");
            }
            catch (System.Exception e)
            {
                Log.Error($"UISelectServerSystem: 打开服务器选择界面失败: {e.Message}");
                Log.Error($"异常堆栈: {e.StackTrace}");
            }
        }
        
        /// <summary>
        /// 进入游戏按钮点击
        /// </summary>
        private static async ETTask OnEnterGameClick(this UISelectServerComponent self)
        {
            ServerInfo server = self.currentSelectedServer;
            if (server == null)
            {
                Log.Error("UISelectServerSystem: 没有选中的服务器");
                return;
            }
            
            Log.Info($"UISelectServerSystem: 进入游戏，服务器: {server.ServerName}");
            
            try
            {
                Scene scene = self.Scene();
                ServerListComponent serverListComp = scene.GetComponent<ServerListComponent>();
                if (serverListComp == null)
                {
                    Log.Error("UISelectServerSystem: ServerListComponent为空，无法选择服务器");
                    return;
                }

                // 调用服务器选择接口
                Log.Info($"UISelectServerSystem: 正在选择服务器: {server.ServerName} (ID: {server.ServerId})");
                var result = await serverListComp.SelectServer(server.ServerId);
                
                if (result.success)
                {
                    Log.Info($"UISelectServerSystem: 服务器选择成功，地址: {result.address}, Key: {result.key}, GateId: {result.gateId}");
                    
                    // 更新当前服务器
                    serverListComp.CurrentServer = server;
                    serverListComp.LastLoginServer = server;
                    
                    // 现在连接Gate服务器
                    await self.ConnectToGate(result.address, result.key, result.gateId);
                    
                    // 关闭选择服务器界面，进入游戏大厅
                    await UIHelper.Remove(scene, UIType.UISelectServer);
                    await UIHelper.Create(scene, UIType.UILobby, UILayer.Mid);
                }
                else
                {
                    Log.Error($"UISelectServerSystem: 选择服务器失败: {server.ServerName}");
                }
                
            }
            catch (System.Exception e)
            {
                Log.Error($"UISelectServerSystem: 进入游戏失败: {e.Message}");
                Log.Error($"UISelectServerSystem: 异常堆栈: {e.StackTrace}");
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
                Log.Warning($"UISelectServerSystem: 发现 {audioListeners.Length} 个AudioListener，保留第一个，移除其余");
                
                // 保留第一个，移除其余
                for (int i = 1; i < audioListeners.Length; i++)
                {
                    Log.Info($"UISelectServerSystem: 移除多余的AudioListener: {audioListeners[i].gameObject.name}");
                    UnityEngine.Object.Destroy(audioListeners[i]);
                }
            }
            else if (audioListeners.Length == 0)
            {
                Log.Warning("UISelectServerSystem: 场景中没有AudioListener，这可能会导致音频问题");
            }
            else
            {
                Log.Info("UISelectServerSystem: AudioListener数量正常");
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
                Log.Warning($"UISelectServerSystem: 发现 {eventSystems.Length} 个EventSystem，保留第一个，移除其余");
                
                // 保留第一个，移除其余
                for (int i = 1; i < eventSystems.Length; i++)
                {
                    Log.Info($"UISelectServerSystem: 移除多余的EventSystem: {eventSystems[i].gameObject.name}");
                    UnityEngine.Object.Destroy(eventSystems[i].gameObject);
                }
            }
            else if (eventSystems.Length == 0)
            {
                Log.Warning("UISelectServerSystem: 场景中没有EventSystem，这可能会导致UI交互问题");
            }
            else
            {
                Log.Info("UISelectServerSystem: EventSystem数量正常");
            }
        }
        
        /// <summary>
        /// 连接到Gate服务器
        /// </summary>
        private static async ETTask ConnectToGate(this UISelectServerComponent self, string address, long key, long gateId)
        {
            try
            {
                Scene scene = self.Scene();
                
                Log.Info($"UISelectServerSystem: 开始连接Gate服务器: {address}");
                
                // 通过ClientSenderComponent获取NetClient的ActorId，然后直接使用ProcessInnerSender发送
                ClientSenderComponent clientSenderComponent = scene.GetComponent<ClientSenderComponent>();
                if (clientSenderComponent == null)
                {
                    Log.Error("UISelectServerSystem: ClientSenderComponent为空，无法连接Gate");
                    return;
                }
                
                // 创建连接Gate的消息
                Main2NetClient_ConnectGate gateConnectRequest = Main2NetClient_ConnectGate.Create();
                gateConnectRequest.Address = address;
                gateConnectRequest.Key = key;
                gateConnectRequest.GateId = gateId;
                
                Log.Info($"UISelectServerSystem: 通过ProcessInnerSender发送Gate连接请求到NetClient");
                var response = await scene.GetComponent<ProcessInnerSender>().Call(clientSenderComponent.netClientActorId, gateConnectRequest) as NetClient2Main_ConnectGate;
                
                if (response == null)
                {
                    Log.Error("UISelectServerSystem: Gate连接响应为空");
                    return;
                }
                
                if (response.Error != ErrorCode.ERR_Success)
                {
                    Log.Error($"UISelectServerSystem: Gate连接失败: {response.Error}, {response.Message}");
                    return;
                }
                
                // 更新玩家ID
                scene.GetComponent<PlayerComponent>().MyId = response.PlayerId;
                
                Log.Info($"UISelectServerSystem: Gate连接成功！玩家ID: {response.PlayerId}");
            }
            catch (System.Exception e)
            {
                Log.Error($"UISelectServerSystem: 连接Gate失败: {e.Message}");
                Log.Error($"UISelectServerSystem: 异常堆栈: {e.StackTrace}");
                throw;
            }
        }
        
    }
}