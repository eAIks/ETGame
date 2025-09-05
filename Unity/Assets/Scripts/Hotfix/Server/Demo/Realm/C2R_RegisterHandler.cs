using System;

namespace ET.Server
{
    [MessageSessionHandler(SceneType.Realm)]
    public class C2R_RegisterHandler : MessageSessionHandler<C2R_Register, R2C_Register>
    {
        protected override async ETTask Run(Session session, C2R_Register request, R2C_Register response)
        {
            // 简单验证
            if (string.IsNullOrEmpty(request.Account))
            {
                response.Error = ErrorCode.ERR_AccountNameFormError;
                response.Message = "账号不能为空";
                await CloseSession(session);
                return;
            }
            
            if (string.IsNullOrEmpty(request.Password))
            {
                response.Error = ErrorCode.ERR_PasswordFormError;
                response.Message = "密码不能为空";
                await CloseSession(session);
                return;
            }
            
            // 账号长度验证
            if (request.Account.Length < 3 || request.Account.Length > 20)
            {
                response.Error = ErrorCode.ERR_AccountNameFormError;
                response.Message = "账号长度必须在3-20个字符之间";
                await CloseSession(session);
                return;
            }
            
            // 密码长度验证
            if (request.Password.Length < 6 || request.Password.Length > 20)
            {
                response.Error = ErrorCode.ERR_PasswordFormError;
                response.Message = "密码长度必须在6-20个字符之间";
                await CloseSession(session);
                return;
            }
            
            // 并发安全的注册实现
            Scene root = session.Root();
            
            // 使用协程锁防止同账号并发注册
            // 锁的key使用账号名的哈希值，确保相同账号的注册请求串行化处理
            using (await root.GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.Register, request.Account.GetHashCode()))
            {
                try
                {
                    DBManagerComponent dbManagerComponent = root.GetComponent<DBManagerComponent>();
                    if (dbManagerComponent == null)
                    {
                        Log.Error("DBManagerComponent not found");
                        response.Error = ErrorCode.ERR_SystemError;
                        response.Message = "系统错误";
                        await CloseSession(session);
                        return;
                    }
                    
                    DBComponent dbComponent = dbManagerComponent.GetZoneDB(root.Zone());
                    
                    // 在锁保护下检查账号是否已存在
                    var existAccounts = await dbComponent.Query<Account>(d => d.AccountName == request.Account, "ET.AccountInfo");
                    if (existAccounts.Count > 0)
                    {
                        response.Error = ErrorCode.ERR_AccountAlreadyRegister;
                        response.Message = "账号已存在";
                        await CloseSession(session);
                        return;
                    }
                    
                    // 创建账号并保存到数据库
                    Account account = root.AddChildWithId<Account, string, string>(
                        IdGenerater.Instance.GenerateId(), 
                        request.Account, 
                        request.Password);
                    
                    try
                    {
                        await dbComponent.Save(account, "ET.AccountInfo");
                    }
                    catch (MongoDB.Driver.MongoWriteException mongoEx) when (mongoEx.WriteError?.Code == 11000)
                    {
                        // MongoDB唯一约束违反 (重复键错误)
                        Log.Warning($"账号 {request.Account} 违反唯一约束，可能是并发注册导致");
                        response.Error = ErrorCode.ERR_AccountAlreadyRegister;
                        response.Message = "账号已存在";
                        await CloseSession(session);
                        return;
                    }
                    
                    response.Error = ErrorCode.ERR_Success;
                    response.Message = "注册成功";
                    
                    Log.Info($"账号注册成功并已保存到数据库: {request.Account}");
                }
                catch (System.Exception ex)
                {
                    Log.Error($"注册过程中发生异常: {ex}");
                    response.Error = ErrorCode.ERR_SystemError;
                    response.Message = "注册失败，请稍后重试";
                    await CloseSession(session);
                    return;
                }
            }
            
            // 注册成功后延迟关闭会话，确保响应能正常发送给客户端
            CloseSession(session).Coroutine();
        }
        
        private async ETTask CloseSession(Session session)
        {
            await session.Root().GetComponent<TimerComponent>().WaitAsync(1000);
            session.Dispose();
        }
    }
}