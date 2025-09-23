using System;


namespace ET.Server
{
    [MessageSessionHandler(SceneType.Gate)]
    public class C2G_LoginGateHandler : MessageSessionHandler<C2G_LoginGate, G2C_LoginGate>
    {
        protected override async ETTask Run(Session session, C2G_LoginGate request, G2C_LoginGate response)
        {
            Scene root = session.Root();
            GateSessionKeyComponent keyComponent = root.GetComponent<GateSessionKeyComponent>();
            string account = keyComponent.Get(request.Key);
            if (account == null)
            {
                response.Error = ErrorCode.ERR_SystemError;
                response.Message = "Gate key验证失败!";
                return;
            }
            
            session.RemoveComponent<SessionAcceptTimeoutComponent>();

            // 获取真正的PlayerID（从数据库获取的）
            long playerID = keyComponent.GetPlayerID(request.Key);
            string accountUUID = keyComponent.GetAccountUUID(request.Key);

            PlayerComponent playerComponent = root.GetComponent<PlayerComponent>();
            Player player = playerComponent.GetByAccount(account);
            if (player == null)
            {
                player = playerComponent.AddChild<Player, string>(account);
                playerComponent.Add(player);
                PlayerSessionComponent playerSessionComponent = player.AddComponent<PlayerSessionComponent>();
                playerSessionComponent.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.GateSession);
                await playerSessionComponent.AddLocation(LocationType.GateSession);
			
                player.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.UnOrderedMessage);
                await player.AddLocation(LocationType.Player);
			
                SessionPlayerComponent sessionPlayerComponent = session.AddComponent<SessionPlayerComponent>();
                sessionPlayerComponent.Player = player;
                sessionPlayerComponent.SessionKey = request.Key; // 保存Key用于后续获取PlayerID
                sessionPlayerComponent.PlayerID = playerID; // 缓存PlayerID避免重复查询
                playerSessionComponent.Session = session;
                
                Log.Info($"新玩家登录: Account={account}, PlayerID={playerID}, UUID={accountUUID}");
            }
            else
            {
                // 判断是否在战斗
                PlayerRoomComponent playerRoomComponent = player.GetComponent<PlayerRoomComponent>();
                if (playerRoomComponent.RoomActorId != default)
                {
                    CheckRoom(player, session, request.Key).Coroutine();
                }
                else
                {
                    PlayerSessionComponent playerSessionComponent = player.GetComponent<PlayerSessionComponent>();
                    playerSessionComponent.Session = session;
                    
                    // 设置SessionKey用于后续获取PlayerID和ServerId
                    SessionPlayerComponent sessionPlayerComponent = session.AddComponent<SessionPlayerComponent>();
                    sessionPlayerComponent.Player = player;
                    sessionPlayerComponent.SessionKey = request.Key;
                    sessionPlayerComponent.PlayerID = playerID; // 缓存PlayerID避免重复查询
                    
                    Log.Info($"玩家重复登录: Account={account}, PlayerID={playerID}");
                }
            }

            // 返回真正的PlayerID（从数据库获取的），而不是Player实体的InstanceId
            response.PlayerId = playerID;
            
            Log.Info($"Gate登录完成: Account={account}, 返回PlayerID={playerID} (Player.ID ={player.Id})");
            
            await ETTask.CompletedTask;
        }

        private static async ETTask CheckRoom(Player player, Session session, long sessionKey)
        {
            Fiber fiber = player.Fiber();
            await fiber.WaitFrameFinish();

            // 获取真正的PlayerID而不是Player.InstanceId
            Scene root = session.Root();
            GateSessionKeyComponent keyComponent = root.GetComponent<GateSessionKeyComponent>();
            long playerID = keyComponent.GetPlayerID(sessionKey);

            G2Room_Reconnect g2RoomReconnect = G2Room_Reconnect.Create();
            g2RoomReconnect.PlayerId = playerID;
            using Room2G_Reconnect room2GateReconnect = await fiber.Root.GetComponent<MessageSender>().Call(
                player.GetComponent<PlayerRoomComponent>().RoomActorId,
                g2RoomReconnect) as Room2G_Reconnect;
            G2C_Reconnect g2CReconnect = G2C_Reconnect.Create();
            g2CReconnect.StartTime = room2GateReconnect.StartTime;
            g2CReconnect.Frame = room2GateReconnect.Frame;
            g2CReconnect.UnitInfos.AddRange(room2GateReconnect.UnitInfos);
            session.Send(g2CReconnect);
            
            SessionPlayerComponent sessionPlayerComponent = session.AddComponent<SessionPlayerComponent>();
            sessionPlayerComponent.Player = player;
            sessionPlayerComponent.SessionKey = sessionKey;
            sessionPlayerComponent.PlayerID = playerID; // 缓存PlayerID避免重复查询
            player.GetComponent<PlayerSessionComponent>().Session = session;
        }
    }
}