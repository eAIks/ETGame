using System;

namespace ET
{
    public abstract class MessageHandler<E, Message>: HandlerObject, IMHandler where E : Entity where Message : class, IMessage
    {
        protected abstract ETTask Run(E entity, Message message);

        public async ETTask Handle(Entity entity, Address fromAddress, MessageObject actorMessage)
        {
            if (entity == null)
            {
                Log.Error($"MessageHandler收到空实体，消息类型: {actorMessage?.GetType().FullName}, fromAddress: {fromAddress}");
                return;
            }
            
            if (actorMessage is not Message msg)
            {
                Log.Error($"消息类型转换错误: {actorMessage.GetType().FullName} to {typeof (Message).Name}");
                return;
            }

            if (entity is not E e)
            {
                Log.Error($"Actor类型转换错误: {entity.GetType().FullName} to {typeof (E).Name} --{typeof (Message).FullName}");
                return;
            }

            await this.Run(e, msg);
        }

        public Type GetRequestType()
        {
            if (typeof (ILocationMessage).IsAssignableFrom(typeof (Message)))
            {
                Log.Error($"message is IActorLocationMessage but handler is AMActorHandler: {typeof (Message)}");
            }

            return typeof (Message);
        }

        public Type GetResponseType()
        {
            return null;
        }
    }
    
    public abstract class MessageHandler<E, Request, Response>: HandlerObject, IMHandler where E : Entity where Request : MessageObject, IRequest where Response : MessageObject, IResponse
    {
        protected abstract ETTask Run(E unit, Request request, Response response);

        public async ETTask Handle(Entity entity, Address fromAddress, MessageObject actorMessage)
        {
            try
            {
                if (entity == null)
                {
                    Log.Error($"MessageHandler收到空实体，消息类型: {actorMessage?.GetType().FullName}, fromAddress: {fromAddress}");
                    return;
                }
                
                Fiber fiber = entity.Fiber();
                if (actorMessage is not Request request)
                {
                    Log.Error($"消息类型转换错误: {actorMessage.GetType().FullName} to {typeof (Request).Name}");
                    return;
                }

                if (entity is not E ee)
                {
                    Log.Error($"Actor类型转换错误: {entity.GetType().FullName} to {typeof (E).FullName} --{typeof (Request).FullName}");
                    return;
                }

                int rpcId = request.RpcId;
                Response response = ObjectPool.Instance.Fetch<Response>();
                try
                {
                    await this.Run(ee, request, response);
                }
                catch (RpcException exception)
                {
                    response.Error = exception.Error;
                    response.Message = exception.ToString();
                }
                catch (Exception exception)
                {
                    response.Error = ErrorCore.ERR_RpcFail;
                    response.Message = exception.ToString();
                }
                
                response.RpcId = rpcId;
                
                // 添加防御性检查防止空引用异常
                ProcessInnerSender processInnerSender = fiber.Root.GetComponent<ProcessInnerSender>();
                if (processInnerSender == null)
                {
                    Log.Error($"ProcessInnerSender组件为空！Fiber: {fiber.Id}, Root: {fiber.Root?.Id}, 消息类型: {typeof(Request).FullName}");
                    throw new Exception($"ProcessInnerSender组件未初始化，请检查服务器启动配置");
                }
                processInnerSender.Reply(fromAddress, response);
            }
            catch (Exception e)
            {
                Log.Error($"处理消息失败: {actorMessage.GetType().FullName}, Entity: {entity?.GetType().FullName}, Error: {e}");
                throw new Exception($"解释消息失败: {actorMessage.GetType().FullName}", e);
            }
        }

        public Type GetRequestType()
        {
            if (typeof (ILocationRequest).IsAssignableFrom(typeof (Request)))
            {
                Log.Error($"message is IActorLocationMessage but handler is AMActorRpcHandler: {typeof (Request)}");
            }

            return typeof (Request);
        }

        public Type GetResponseType()
        {
            return typeof (Response);
        }
    }
}