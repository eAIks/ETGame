namespace ET.Server
{
	[MessageSessionHandler(SceneType.Gate)]
	public class C2G_EnterMapHandler : MessageSessionHandler<C2G_EnterMap, G2C_EnterMap>
	{
		protected override async ETTask Run(Session session, C2G_EnterMap request, G2C_EnterMap response)
		{
			Log.Info("C2G_EnterMapHandler: 开始处理进入地图请求");
			
			Player player = session.GetComponent<SessionPlayerComponent>().Player;
			Log.Info($"C2G_EnterMapHandler: 获取到Player, Account={player?.Account}");

			// Gate服务器职责：验证玩家身份，获取必要信息，然后转发给Map服务器
			Scene root = session.Root();
			GateSessionKeyComponent keyComponent = root.GetComponent<GateSessionKeyComponent>();
			if (keyComponent == null)
			{
				Log.Error("C2G_EnterMapHandler: GateSessionKeyComponent未找到");
				response.Error = ErrorCode.ERR_SystemError;
				response.Message = "系统错误：GateSessionKeyComponent未找到";
				return;
			}
			
			long sessionKey = session.GetComponent<SessionPlayerComponent>().SessionKey;
			long playerID = keyComponent.GetPlayerID(sessionKey);
			int serverId = keyComponent.GetServerId(sessionKey);
			Log.Info($"C2G_EnterMapHandler: 验证身份成功 - PlayerID={playerID}, ServerId={serverId}");
			
			// 获取MainScene配置，准备转发请求到Map服务器
			StartSceneConfig startSceneConfig = null;
			
			// 添加调试信息：查看所有可用的Map配置
			Log.Info($"C2G_EnterMapHandler: 开始查找MainScene配置，Maps列表数量={StartSceneConfigCategory.Instance.Maps.Count}");
			
			foreach (StartSceneConfig mapConfig in StartSceneConfigCategory.Instance.Maps)
			{
				Log.Info($"C2G_EnterMapHandler: 发现Map配置 - Name={mapConfig.Name}, Zone={mapConfig.Zone}, Id={mapConfig.Id}");
				if (mapConfig.Name == "MainScene")
				{
					startSceneConfig = mapConfig;
					Log.Info($"C2G_EnterMapHandler: 在Maps列表中找到MainScene配置，Zone={mapConfig.Zone}, ActorId={mapConfig.ActorId}");
					break;
				}
			}
			
			if (startSceneConfig == null)
			{
				Log.Error("C2G_EnterMapHandler: MainScene配置未找到");
				
				// 尝试更全面的搜索：在所有配置中查找
				Log.Info("C2G_EnterMapHandler: 尝试在所有StartSceneConfig中查找MainScene");
				foreach (var kvp in StartSceneConfigCategory.Instance.GetAll())
				{
					StartSceneConfig config = kvp.Value;
					Log.Info($"C2G_EnterMapHandler: 发现配置 - Id={config.Id}, Name={config.Name}, SceneType={config.SceneType}, Zone={config.Zone}");
					if (config.Name == "MainScene")
					{
						startSceneConfig = config;
						Log.Info($"C2G_EnterMapHandler: 在全局配置中找到MainScene，SceneType={config.SceneType}");
						break;
					}
				}
			}
			
			if (startSceneConfig == null)
			{
				Log.Error("C2G_EnterMapHandler: MainScene配置未找到");
				response.Error = ErrorCode.ERR_SystemError;
				response.Message = "MainScene配置未找到";
				return;
			}
			
			// 发送请求到Map服务器处理角色数据查询/创建
			Log.Info("C2G_EnterMapHandler: 将角色数据处理请求转发到Map服务器");
			G2M_PlayerEnterMapRequest mapRequest = G2M_PlayerEnterMapRequest.Create();
			mapRequest.Account = player.Account;
			mapRequest.PlayerId = playerID;
			mapRequest.ServerId = serverId;
			
			M2G_PlayerEnterMapResponse mapResponse = await root.GetComponent<MessageSender>().Call(
				startSceneConfig.ActorId, mapRequest) as M2G_PlayerEnterMapResponse;
			
			if (mapResponse == null)
			{
				Log.Error("C2G_EnterMapHandler: Map服务器响应为空");
				response.Error = ErrorCode.ERR_SystemError;
				response.Message = "服务器内部错误";
				return;
			}
			
			if (mapResponse.Error != ErrorCode.ERR_Success)
			{
				Log.Error($"C2G_EnterMapHandler: Map服务器角色数据处理失败: {mapResponse.Error}, {mapResponse.Message}");
				response.Error = mapResponse.Error;
				response.Message = mapResponse.Message;
				return;
			}
			
			Log.Info($"C2G_EnterMapHandler: Map服务器角色数据处理成功，PlayerId={mapResponse.UnitId}");
			
			// 角色数据处理完成后，发送场景切换消息给客户端
			// 不创建Unit，但需要触发场景切换
			Log.Info("C2G_EnterMapHandler: 开始发送场景切换消息");
			
			// 通知客户端开始切换到MainScene
			M2C_StartSceneChange m2CStartSceneChange = M2C_StartSceneChange.Create();
			m2CStartSceneChange.SceneInstanceId = IdGenerater.Instance.GenerateInstanceId(); // 生成新的场景实例ID
			m2CStartSceneChange.SceneName = startSceneConfig.Name;
			
			// 直接发送给客户端session
			session.Send(m2CStartSceneChange);
			Log.Info($"C2G_EnterMapHandler: 已发送场景切换消息 - SceneName={startSceneConfig.Name}, SceneInstanceId={m2CStartSceneChange.SceneInstanceId}");
			
			response.MyId = player.Id;
			Log.Info($"C2G_EnterMapHandler: 角色数据处理和场景切换完成，返回PlayerId={player.Id}");
		}
	}
}