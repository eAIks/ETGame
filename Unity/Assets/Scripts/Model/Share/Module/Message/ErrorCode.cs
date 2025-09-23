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
        
        // 服务器列表相关错误码
        public const int ERR_ServerListLoadFailed = 200008;    // 服务器列表加载失败
        public const int ERR_ServerListNotLoaded = 200009;     // 服务器列表未加载
        public const int ERR_ServerNotFound = 200010;          // 服务器不存在
        public const int ERR_ServerMaintenance = 200011;       // 服务器维护中
        public const int ERR_ServerFull = 200012;              // 服务器已满
        public const int ERR_ServerSelectFailed = 200013;      // 选择服务器失败
        
        // 装备相关错误码
        public const int ERR_UnitNotFound = 200014;            // 玩家Unit不存在
        public const int ERR_ComponentNotFound = 200015;       // 组件不存在
        public const int ERR_EquipmentNotFound = 200016;       // 装备不存在
        public const int ERR_SlotIndexInvalid = 200017;        // 槽位索引无效
        public const int ERR_EquipmentTypeNotMatch = 200018;   // 装备类型与槽位不匹配
        public const int ERR_EquipFailed = 200019;             // 装备穿戴失败
        public const int ERR_InternalError = 200020;           // 服务器内部错误
        public const int ERR_SessionPlayerError = 200021;      // 会话玩家信息错误
        public const int ERR_PlayerComponentError = 200022;    // 玩家组件信息错误
        public const int ERR_PlayerNotFound = 200023;          // 玩家不存在
        
        // 装备生成相关错误码
        public const int ERR_SessionPlayerInvalid = 200024;    // 玩家会话无效
        public const int ERR_MapConfigNotFound = 200025;       // Map服务器配置未找到
        public const int ERR_MapServerNoResponse = 200026;     // Map服务器无响应
        public const int ERR_MapActorNotFound = 200027;        // 找不到Map服务器Actor
        public const int ERR_GenerateEquipmentFailed = 200028; // 装备生成失败
        
        // 装备替换相关错误码
        public const int ERR_ReplaceMapConfigNotFound = 200029;    // 装备替换Map配置未找到
        public const int ERR_ReplaceMapServerNoResponse = 200030;  // 装备替换Map服务器无响应
        public const int ERR_ReplaceMapActorNotFound = 200031;     // 装备替换找不到Map服务器Actor
        public const int ERR_ReplaceEquipmentFailed = 200032;      // 装备替换失败
    }
}