using System;

namespace ET.Server
{
    [Invoke((long)SceneType.Gate)]
    public class NetComponentOnReadInvoker_Gate: AInvokeHandler<NetComponentOnRead>
    {
        public override void Handle(NetComponentOnRead args)
        {
            HandleAsync(args).Coroutine();
        }

        private async ETTask HandleAsync(NetComponentOnRead args)
        {
            Session session = args.Session;
            object message = args.Message;
            Scene root = args.Session.Root();
            // 根据消息接口判断是不是Actor消息，不同的接口做不同的处理,比如需要转发给Chat Scene，可以做一个IChatMessage接口
            switch (message)
            {
                case ISessionMessage:
                {
                    MessageSessionDispatcher.Instance.Handle(session, message);
                    break;
                }
                case FrameMessage frameMessage:
                {
                    SessionPlayerComponent sessionPlayerComponent = session.GetComponent<SessionPlayerComponent>();
                    Player player = sessionPlayerComponent.Player;
                    ActorId roomActorId = player.GetComponent<PlayerRoomComponent>().RoomActorId;
                    
                    // 直接从SessionPlayerComponent获取PlayerID（已在登录时缓存）
                    long playerID = sessionPlayerComponent.PlayerID;
                    
                    frameMessage.PlayerId = playerID;
                    root.GetComponent<MessageSender>().Send(roomActorId, frameMessage);
                    break;
                }
                case IRoomMessage actorRoom:
                {
                    SessionPlayerComponent sessionPlayerComponent = session.GetComponent<SessionPlayerComponent>();
                    Player player = sessionPlayerComponent.Player;
                    ActorId roomActorId = player.GetComponent<PlayerRoomComponent>().RoomActorId;
                    
                    // 直接从SessionPlayerComponent获取PlayerID（已在登录时缓存）
                    long playerID = sessionPlayerComponent.PlayerID;
                    
                    actorRoom.PlayerId = playerID;
                    root.GetComponent<MessageSender>().Send(roomActorId, actorRoom);
                    break;
                }
                case ILocationMessage actorLocationMessage:
                {
                    SessionPlayerComponent sessionPlayerComponent = session.GetComponent<SessionPlayerComponent>();
                    
                    // 直接从SessionPlayerComponent获取PlayerID（已在登录时缓存）
                    long unitId = sessionPlayerComponent.PlayerID;
                    
                    root.GetComponent<MessageLocationSenderComponent>().Get(LocationType.Unit).Send(unitId, actorLocationMessage);
                    break;
                }
                case ILocationRequest actorLocationRequest: // gate session收到actor rpc消息，先向actor 发送rpc请求，再将请求结果返回客户端
                {
                    SessionPlayerComponent sessionPlayerComponent = session.GetComponent<SessionPlayerComponent>();
                    
                    // 直接从SessionPlayerComponent获取PlayerID（已在登录时缓存）
                    long unitId = sessionPlayerComponent.PlayerID;
                    int rpcId = actorLocationRequest.RpcId; // 这里要保存客户端的rpcId
                    long instanceId = session.InstanceId;
                    IResponse iResponse = await root.GetComponent<MessageLocationSenderComponent>().Get(LocationType.Unit).Call(unitId, actorLocationRequest);
                    iResponse.RpcId = rpcId;
                    // session可能已经断开了，所以这里需要判断
                    if (session.InstanceId == instanceId)
                    {
                        session.Send(iResponse);
                    }
                    break;
                }
                case IRequest actorRequest:  // 分发IActorRequest消息，目前没有用到，需要的自己添加
                {
                    break;
                }
                case IMessage actorMessage:  // 分发IActorMessage消息，目前没有用到，需要的自己添加
                {
                    break;
                }
				
                default:
                {
                    throw new Exception($"not found handler: {message}");
                }
            }
        }
    }
}