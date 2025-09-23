using System;

namespace ET.Server
{
    [MessageSessionHandler(SceneType.Realm)]
    public class C2R_RegisterHandler : MessageSessionHandler<C2R_Register, R2C_Register>
    {
        protected override async ETTask Run(Session session, C2R_Register request, R2C_Register response)
        {
            if (string.IsNullOrEmpty(request.Account) || request.Account.Length < 3 || request.Account.Length > 20)
            {
                response.Error = ErrorCode.ERR_AccountNameFormError;
                await CloseSession(session);
                return;
            }
            
            if (string.IsNullOrEmpty(request.Password) || request.Password.Length < 6 || request.Password.Length > 20)
            {
                response.Error = ErrorCode.ERR_PasswordFormError;
                await CloseSession(session);
                return;
            }
            
            Scene root = session.Root();
            
            using (await root.GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.Register, request.Account.GetHashCode()))
            {
                try
                {
                    DBManagerComponent dbManagerComponent = root.GetComponent<DBManagerComponent>();
                    if (dbManagerComponent == null)
                    {
                        response.Error = ErrorCode.ERR_SystemError;
                        await CloseSession(session);
                        return;
                    }
                    
                    DBComponent dbComponent = dbManagerComponent.GetZoneDB(root.Zone());
                    
                    var existAccounts = await dbComponent.Query<Account>(d => d.AccountName == request.Account, "ET.Server.AccountInfo");
                    if (existAccounts.Count > 0)
                    {
                        response.Error = ErrorCode.ERR_AccountAlreadyRegister;
                        await CloseSession(session);
                        return;
                    }
                    
                    Account account = root.AddChildWithId<Account, string, string>(
                        IdGenerater.Instance.GenerateId(), 
                        request.Account, 
                        request.Password);
                    
                    try
                    {
                        await dbComponent.Save(account, "ET.Server.AccountInfo");
                    }
                    catch (MongoDB.Driver.MongoWriteException mongoEx) when (mongoEx.WriteError?.Code == 11000)
                    {
                        response.Error = ErrorCode.ERR_AccountAlreadyRegister;
                        await CloseSession(session);
                        return;
                    }
                    
                    response.Error = ErrorCode.ERR_Success;
                }
                catch (System.Exception ex)
                {
                    Log.Error($"注册异常: {ex}");
                    response.Error = ErrorCode.ERR_SystemError;
                    await CloseSession(session);
                    return;
                }
            }
            
            CloseSession(session).Coroutine();
        }
        
        private async ETTask CloseSession(Session session)
        {
            await session.Root().GetComponent<TimerComponent>().WaitAsync(1000);
            session.Dispose();
        }
    }
}