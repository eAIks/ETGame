namespace ET.Server
{
	[MessageSessionHandler(SceneType.Gate)]
	public class C2G_EnterMapHandler : MessageSessionHandler<C2G_EnterMap, G2C_EnterMap>
	{
		protected override async ETTask Run(Session session, C2G_EnterMap request, G2C_EnterMap response)
		{
			Log.Info("C2G_EnterMapHandler: 开始处理进入地图请求");
			
			// 获取玩家账号信息
			Player player = session.GetComponent<SessionPlayerComponent>().Player;
			if (player == null)
			{
				Log.Error("C2G_EnterMapHandler: Player未找到");
				response.Error = ErrorCode.ERR_SystemError;
				response.Message = "玩家信息未找到";
				return;
			}
			
			// 获取ServerId
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
			int serverId = keyComponent.GetServerId(sessionKey);
			
			Log.Info($"C2G_EnterMapHandler: 获取到信息 - Account={player.Account}, ServerId={serverId}");
			
			// 获取MainScene配置
			StartSceneConfig startSceneConfig = null;
			foreach (StartSceneConfig mapConfig in StartSceneConfigCategory.Instance.Maps)
			{
				if (mapConfig.Name == "MainScene")
				{
					startSceneConfig = mapConfig;
					break;
				}
			}
			
			if (startSceneConfig == null)
			{
				Log.Error("C2G_EnterMapHandler: MainScene配置未找到");
				response.Error = ErrorCode.ERR_SystemError;
				response.Message = "MainScene配置未找到";
				return;
			}
			
			// 转发Account和ServerId到Map服务器，由Map服务器判断进入是否成功以及是否存在PlayerID
			Log.Info("C2G_EnterMapHandler: 转发请求到Map服务器");
			G2M_PlayerEnterMapRequest mapRequest = G2M_PlayerEnterMapRequest.Create();
			mapRequest.Account = player.Account;
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
			
			// 直接返回Map服务器的处理结果
			response.Error = mapResponse.Error;
			response.Message = mapResponse.Message;
			
			if (mapResponse.Error == ErrorCode.ERR_Success)
			{
				// 通知客户端开始切换到MainScene
				M2C_StartSceneChange m2CStartSceneChange = M2C_StartSceneChange.Create();
				m2CStartSceneChange.SceneInstanceId = IdGenerater.Instance.GenerateInstanceId(); // 生成新的场景实例ID
				m2CStartSceneChange.SceneName = startSceneConfig.Name;
			
				// 直接发送给客户端session
				session.Send(m2CStartSceneChange);
				Log.Info($"C2G_EnterMapHandler: 已发送场景切换消息 - SceneName={startSceneConfig.Name}, SceneInstanceId={m2CStartSceneChange.SceneInstanceId}");

				response.MyId = mapResponse.PlayerId;
				Log.Info($"C2G_EnterMapHandler: Map服务器处理成功，返回PlayerId={mapResponse.PlayerId}");
			}
			else
			{
				Log.Error($"C2G_EnterMapHandler: Map服务器处理失败: {mapResponse.Error}, {mapResponse.Message}");
			}
		}
	}
}