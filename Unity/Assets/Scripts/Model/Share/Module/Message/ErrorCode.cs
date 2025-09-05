namespace ET
{
    public static partial class ErrorCode
    {
        public const int ERR_Success = 0;

        // 1-11004 是SocketError请看SocketError定义
        //-----------------------------------
        // 100000-109999是Core层的错误
        
        // 110000以下的错误请看ErrorCore.cs
        
        // 这里配置逻辑层的错误码
        // 110000 - 200000是抛异常的错误
        // 200001以上不抛异常
        
        // 账号相关错误码
        public const int ERR_AccountNotExist = 200002;        // 账号不存在
        public const int ERR_PasswordError = 200003;          // 密码错误  
        public const int ERR_AccountAlreadyRegister = 200004;  // 账号已存在
        public const int ERR_AccountNameFormError = 200005;    // 账号格式错误
        public const int ERR_PasswordFormError = 200006;       // 密码格式错误
        public const int ERR_SystemError = 200007;             // 系统错误
    }
}