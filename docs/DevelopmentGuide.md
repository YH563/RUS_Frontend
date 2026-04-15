# Godot 开发规范

本项目遵循 Godot 的开发规范，并在此基础上进行了一些扩展和优化，本文档仅针对 **C# 代码**进行说明。

## 命名规范

### 文件和文件夹
- **场景文件**：`snake_case`，如 `patient_list.tscn`、`robot_control_panel.tscn`
- **脚本文件**：`PascalCase`，与类名一致，如 `PatientManager.cs`、`RobotController.cs`
- **资源文件**：`snake_case`，如 `icon_robot.png`、`background_main.jpg`
- **文件夹**：`PascalCase`，如 `src/Robot`

### C# 代码元素
| 类型 | 命名规则 | 示例 |
|------|----------|------|
| 类、结构、枚举 | `PascalCase` | `PatientManager`, `RobotState` |
| 接口 | `I` + `PascalCase` | `INetworkClient` |
| 方法 | `PascalCase` | `ConnectToBackend()`, `UpdateJointAngles()` |
| 属性 | `PascalCase` | `IsConnected`, `CurrentPatient` |
| 公共字段 | `PascalCase` | `Health`, `MaxSpeed` |
| 私有字段 | `_camelCase` | `_ws`, `_jointAngles` |
| 常量 | `PascalCase` 或 `UPPER_SNAKE` | `MaxRetryCount`, `DEFAULT_URL` |
| 局部变量 | `camelCase` | `jointCount`, `responseData` |
| 信号（委托） | `PascalCase` + `EventHandler` | `PatientDataReceivedEventHandler` |
| 枚举成员 | `PascalCase` | `RobotStatus.Idle`, `RobotStatus.Moving` |


## 代码风格

### 脚本文件储存位置

所有脚本文件储存在 `src/` 文件夹下对应模块文件夹下。

### 命名空间

所有脚本均在命名空间下开发，便于代码隔离，保持与脚本文件储存路径一致，例如 `namespace SRC.Robot`。

### 注释
- **类注释**：使用 XML 文档注释 `///`，简要说明职责
- **方法注释**：说明参数、返回值、异常
- **复杂逻辑**：在代码上方添加普通注释

```csharp
/// <summary>
/// 负责与 ROS bridge 建立 WebSocket 连接，收发消息。
/// </summary>
public class RosBridgeClient : Node
{
	/// <summary>
	/// 订阅指定话题。
	/// </summary>
	/// <param name="topic">话题名称</param>
	/// <param name="messageType">消息类型（可选）</param>
	public void Subscribe(string topic, string messageType = "")
	{
		// 实现...
	}
}
```
### 公共成员

对于公共成员变量，一律使用属性，而非字段。
```csharp
// 属性（推荐）
public int Health { get; set; }

// 字段（不推荐）
public int Health;
```


## 脚本与节点的关系

### 单脚本挂载
- 每个场景根节点挂载一个主脚本，负责该场景的逻辑
- 子节点应尽量通过 `@onready` 获取引用，避免 `GetNode` 硬编码路径

```csharp
public partial class MainScene : Node
{
	[Export] public NodePath PatientListNodePath;
	private PatientList _patientList;
	
	public override void _Ready()
	{
		_patientList = GetNode<PatientList>(PatientListNodePath);
		// 或使用 % 语法
		// _patientList = GetNode<PatientList>("%PatientList");
	}
}
```

### 脚本复用
- 通用的 UI 控件（如按钮、弹窗）应做成独立场景，并挂载相应脚本，通过 `Export` 变量暴露可配置属性

## 信号（Signal）的使用规范

### 声明与连接
- 信号使用委托定义，命名以 `EventHandler` 结尾
- 在 `_Ready()` 中连接信号，避免在循环中重复连接

```csharp
[Signal]
public delegate void PatientSelectedEventHandler(string patientId);

public override void _Ready()
{
	// 连接其他节点的信号
	var button = GetNode<Button>("%SelectButton");
	button.Pressed += OnSelectButtonPressed;
	
	// 连接自己的信号（如果外部需要）
	PatientSelected += OnPatientSelected;
}

private void OnPatientSelected(string patientId)
{
	// 处理选中
}
```

### 信号与解耦
- 模块间通信优先使用信号，避免直接调用对方的方法
- 信号参数尽量简单（基本类型或自定义 `struct`），不要传递 `Node`

## 资源管理

### 资源路径

- 所有资源使用 res:// 相对路径
- 在代码中引用资源时，优先使用 [Export] 在编辑器赋值，避免硬编码路径

### 动态加载

- 使用 `ResourceLoader.Load<T>` 加载资源时，注意检查返回是否为空
- 场景切换使用 `SceneTree.ChangeSceneToFile` 或 `PackedScene.Instantiate`

## 日志器使用

### 概述

为便于日志记录，采用了自定义的日志器组件进行记录。

### 使用方式

#### 1.引入命名空间
```csharp
using SRC.Logger;
```

#### 2.基本调用
```csharp
Logger.Logger.Debug("变量 x 的值为 " + x, this);
Logger.Logger.Info("玩家登录成功", this);
Logger.Logger.Warn("配置文件缺失，使用默认值", this);
Logger.Logger.Error("数据库连接失败", this);
```

### 日志格式
```
[时间] [级别] [类名] 消息内容
```
注意：在静态方法内调用日志时，无法传入this对象，只写入日志信息即可。

## 测试

- Debug 时只测试当前场景，禁止将模块移入主场景中进行测试

## 模块开发文档

在 `docs` 文件夹下创建与模块同名的文件夹，并在该文件夹内创建与模块同名的 `.md` 文件，用于描述包的功能和使用方法，可参考[示例文档](./DocExample.md)。
