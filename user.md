# 角色属性

#创建UserBase配置表包括以下属性
    1.气血
    2.攻击
    3.防御
    4.连击率
    5.反击率
    6.暴击率
    7.吸血率
    8.击晕率
    8.抗连击率
    9.抗反击率
    10.抗暴击率
    11.抗吸血率
    12.抗击晕率



#数据库操作流程
       查询/创建角色
      ├─ 查玩家表 player (game_server_<配表中的服务器ID>) 如果不存在该配表，创建该表
      ├─ 若无记录 → 读取 UserBase 初始化角色
      └─ 返回角色数据



#装备流程

    用户点击生成装备按钮
        - 发送C2G_GenerateEquipment消息到Gate服务器
        - Gate服务器进行转发，发送G2M_GenerateEquipment到Map服务器
        - Map服务器进行逻辑处理之后 发送M2G_GenerateEquipment到Gate服务器
        - Gate服务器转发Map消息 发送G2C_GenerateEquipment到Client
        - Client收到消息之后弹出UIEquipmentConfirm


    用户替换装备
        - 
