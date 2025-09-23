namespace ET.Server
{
	[MessageSessionHandler(SceneType.Gate)]
	public class C2G_MatchHandler : MessageSessionHandler<C2G_Match, G2C_Match>
	{
		protected override async ETTask Run(Session session, C2G_Match request, G2C_Match response)
		{
			SessionPlayerComponent sessionPlayerComponent = session.GetComponent<SessionPlayerComponent>();
			Player player = sessionPlayerComponent.Player;

			// 直接从SessionPlayerComponent获取PlayerID（已在登录时缓存）
			Scene root = session.Root();
			long playerID = sessionPlayerComponent.PlayerID;
			
			if (playerID == 0)
			{
				response.Error = ErrorCode.ERR_PlayerIDInvalid;
				response.Message = "玩家ID无效";
				return;
			}

			StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.Match;

			G2Match_Match g2MatchMatch = G2Match_Match.Create();
			g2MatchMatch.Id = playerID;
			await session.Root().GetComponent<MessageSender>().Call(startSceneConfig.ActorId, g2MatchMatch);
		}
	}
}