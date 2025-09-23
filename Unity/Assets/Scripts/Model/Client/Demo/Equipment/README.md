# 装备系统使用说明

## 功能概述
实现了一个九宫格装备系统，包含以下功能：
1. 9个装备槽位展示
2. 点击装备槽查看装备详情
3. 点击生成按钮随机生成装备
4. 确认/取消装备穿戴

## 核心组件

### 数据模型
- `Equipment` - 装备数据类
- `EquipmentComponent` - 玩家装备管理组件

### UI组件
- `UIMainComponent` - 主界面组件，管理装备槽
- `UIEquipmentSlot` - 装备槽组件
- `UIEquipmentDetailComponent` - 装备详情弹窗
- `UIEquipmentConfirmComponent` - 装备确认弹窗

## Unity编辑器设置

### UIEquipmentSlot.prefab (装备槽预制体)
创建一个可复用的装备槽预制体：
```
UIEquipmentSlot (GameObject)
├── Background (Image) - 装备槽背景框
├── Icon (GameObject) - 装备图标容器
│   ├── Image - 装备图标显示
│   └── Button - 点击查看装备详情（绑定在Icon上）
└── Name (Text) - 装备名称显示
```

**设计优势：**
- Icon作为Button，只有装备存在时才显示和可点击
- 无装备时Icon隐藏，用户无法点击，体验更直观
- 装备槽背景始终显示，保持UI布局稳定

### UIMain.prefab
主界面结构：
```
UIMain (Canvas)
├── EquipmentPanel (GameObject) - 装备面板容器
│   ├── Slot0 (UIEquipmentSlot prefab instance)
│   ├── Slot1 (UIEquipmentSlot prefab instance)  
│   ├── Slot2 (UIEquipmentSlot prefab instance)
│   ├── Slot3 (UIEquipmentSlot prefab instance)
│   ├── Slot4 (UIEquipmentSlot prefab instance)
│   ├── Slot5 (UIEquipmentSlot prefab instance)
│   ├── Slot6 (UIEquipmentSlot prefab instance)
│   ├── Slot7 (UIEquipmentSlot prefab instance)
│   └── Slot8 (UIEquipmentSlot prefab instance)
└── GenerateEquipmentButton (Button) - 统一的生成装备按钮
```

### UIEquipmentConfirm.prefab（首次装备确认）
```
UIEquipmentConfirm (Canvas)
├── Background (Image) - 半透明背景遮罩
├── Panel (GameObject) - 确认面板
│   ├── Title (Text) - "装备确认"
│   ├── PromptText (Text) - "是否装备以下装备？"
│   ├── EquipmentInfo (GameObject) - 装备信息
│   │   ├── IconImage (Image) - 装备图标
│   │   ├── NameText (Text) - 装备名称
│   │   ├── QualityText (Text) - 品质显示
│   │   └── AttributePanel (GameObject) - 属性面板
│   │       ├── AttackText (Text) - 攻击力
│   │       ├── DefenseText (Text) - 防御力
│   │       └── HealthText (Text) - 生命值
│   └── ButtonPanel (GameObject) - 按钮面板
│       ├── ConfirmButton (Button) - 确认装备
│       └── CancelButton (Button) - 取消
```

### UIEquipmentCompare.prefab（装备替换对比）
```
UIEquipmentCompare (Canvas)
├── Background (Image) - 半透明背景遮罩
├── Panel (GameObject) - 对比面板
│   ├── Title (Text) - "装备替换"
│   ├── PromptText (Text) - "发现新装备，是否替换当前装备？"
│   ├── UpperSection (GameObject) - 上半部分：当前装备
│   │   ├── CurrentLabel (Text) - "当前装备"
│   │   └── OldEquipmentInfo (GameObject) - 当前装备信息
│   │       ├── OldIconImage (Image) - 当前装备图标
│   │       ├── OldNameText (Text) - 当前装备名称
│   │       ├── OldQualityText (Text) - 当前装备品质
│   │       └── OldAttributePanel (GameObject) - 当前装备属性
│   │           ├── OldAttackText (Text) - 当前装备攻击力
│   │           ├── OldDefenseText (Text) - 当前装备防御力
│   │           └── OldHealthText (Text) - 当前装备生命值
│   ├── Divider (Image) - 分割线
│   ├── LowerSection (GameObject) - 下半部分：新装备
│   │   ├── NewLabel (Text) - "新装备"
│   │   ├── NewEquipmentInfo (GameObject) - 新装备信息
│   │   │   ├── NewIconImage (Image) - 新装备图标
│   │   │   ├── NewNameText (Text) - 新装备名称
│   │   │   ├── NewQualityText (Text) - 新装备品质
│   │   │   └── NewAttributePanel (GameObject) - 新装备属性（带对比箭头）
│   │   │       ├── NewAttackText (Text) - 新装备攻击力 ↑↓
│   │   │       ├── NewDefenseText (Text) - 新装备防御力 ↑↓
│   │   │       └── NewHealthText (Text) - 新装备生命值 ↑↓
│   │   └── ButtonPanel (GameObject) - 按钮面板
│   │       ├── ReplaceButton (Button) - 替换装备
│   │       └── KeepButton (Button) - 保持当前
```

### UIEquipmentDetail.prefab（装备详情查看）
```
UIEquipmentDetail (Canvas)
├── Background (Image) - 半透明背景遮罩
├── Panel (GameObject) - 弹窗面板
│   ├── Title (Text) - 标题"装备详情"
│   ├── IconImage (Image) - 装备图标
│   ├── NameText (Text) - 装备名称
│   ├── QualityText (Text) - 品质显示
│   ├── AttributePanel (GameObject) - 属性面板
│   │   ├── AttackText (Text) - 攻击力显示
│   │   ├── DefenseText (Text) - 防御力显示
│   │   └── HealthText (Text) - 生命值显示
│   ├── DescriptionPanel (GameObject) - 描述面板
│   │   ├── DescTitle (Text) - "描述"标题
│   │   └── DescriptionText (Text) - 装备描述
│   └── CloseButton (Button) - 关闭按钮
```

## ReferenceCollector 配置

### UIMain.prefab 的 ReferenceCollector
需要在 UIMain 的 GameObject 上添加 ReferenceCollector 组件，并配置以下引用：

| Key | GameObject |
|-----|------------|
| EquipmentPanel | 装备面板容器 |
| GenerateEquipmentButton | 生成装备按钮 |
| Slot0 | 装备槽0的GameObject |
| Slot1 | 装备槽1的GameObject |
| Slot2 | 装备槽2的GameObject |
| Slot3 | 装备槽3的GameObject |
| Slot4 | 装备槽4的GameObject |
| Slot5 | 装备槽5的GameObject |
| Slot6 | 装备槽6的GameObject |
| Slot7 | 装备槽7的GameObject |
| Slot8 | 装备槽8的GameObject |

### UIEquipmentDetail.prefab 的 ReferenceCollector
| Key | GameObject |
|-----|------------|
| NameText | 装备名称文本 |
| AttackText | 攻击力文本 |
| DefenseText | 防御力文本 |
| HealthText | 生命值文本 |
| QualityText | 品质文本 |
| DescriptionText | 描述文本 |
| IconImage | 装备图标 |
| CloseButton | 关闭按钮 |

### UIEquipmentConfirm.prefab 的 ReferenceCollector
| Key | GameObject |
|-----|------------|
| NameText | 装备名称文本 |
| AttackText | 攻击力文本 |
| DefenseText | 防御力文本 |
| HealthText | 生命值文本 |
| QualityText | 品质文本 |
| IconImage | 装备图标 |
| ConfirmButton | 确认按钮 |
| CancelButton | 取消按钮 |

## 使用方法
1. 在Unity中创建上述prefab结构
2. 为每个prefab添加ReferenceCollector组件并配置引用
3. 玩家登录后会自动显示主界面，装备系统会自动初始化

## 优势
- **复用性强**: UIEquipmentSlot.prefab 可以在其他地方复用
- **维护方便**: 修改装备槽样式只需修改一个prefab
- **扩展性好**: 可以轻松添加更多装备槽或修改布局
- **性能优化**: 预制体实例化比动态创建UI更高效