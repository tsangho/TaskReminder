# TaskReminder - 定时提醒工具

一个基于 WPF 和 Material Design 的桌面定时提醒工具。

## 项目状态

**最后更新：** 2026-03-29  
**开发进度：** 阶段一第 2 步完成 - 系统托盘 + 定时检查 + 通知功能

### 已完成功能 ✅

#### 基础功能
- [x] 数据库层（SQLite + Dapper）
- [x] 数据模型（TaskItem）
- [x] MVVM 架构（TaskViewModel）
- [x] 主界面（MainWindow）
- [x] 任务列表 ListView（左侧）
- [x] 任务详情区域（右侧）
- [x] 新建任务按钮
- [x] 刷新按钮
- [x] 数据绑定到 ViewModel
- [x] 添加/编辑对话框（AddEditTaskDialog）
- [x] 标题输入
- [x] 描述输入
- [x] 日期选择器
- [x] 时间选择器（小时 + 分钟）
- [x] 重复类型选择
- [x] 重复间隔设置
- [x] Material Design 界面风格
- [x] ObservableCollection 数据绑定
- [x] INotifyPropertyChanged 实现

#### 阶段一第 2 步 - 后台运行 + 定时检查 + 通知（新增）
- [x] 系统托盘集成（TrayIconService）
  - [x] 使用 `System.Windows.Forms.NotifyIcon`
  - [x] 托盘图标显示
  - [x] 托盘菜单：显示主界面、退出
  - [x] 最小化到托盘（点击关闭时隐藏）
- [x] 定时检查器（ReminderChecker）
  - [x] 使用 `System.Timers.Timer` 每分钟检查
  - [x] 查询数据库中到期任务
  - [x] 过滤已完成任务
  - [x] 过滤 LastReminderTime 避免重复提醒
  - [x] 触发通知
- [x] Windows Toast 通知（NotificationService）
  - [x] 使用 `Windows.UI.Notifications`
  - [x] 实现 `ShowNotification(title, message)` 方法
  - [x] 点击通知打开主界面
- [x] 集成到 MainWindow
  - [x] 注入 TrayIconService
  - [x] 注入 ReminderChecker
  - [x] 窗口关闭时最小化到托盘
  - [x] 托盘菜单点击打开主界面

### 待完成功能 🚧

- [ ] 任务完成状态切换（UI 交互）
- [ ] 设置界面
- [ ] 开机自启
- [ ] 自定义通知声音
- [ ] 通知历史记录

## 技术栈

- **框架：** .NET 8 / WPF
- **UI 库：** MaterialDesignInXamlToolkit
- **数据库：** SQLite
- **ORM：** Dapper
- **架构模式：** MVVM
- **系统服务：**
  - System.Windows.Forms（托盘图标）
  - Windows.UI.Notifications（Toast 通知）
  - System.Timers（定时检查）

## 项目结构

```
TaskReminder/
├── Models/
│   └── TaskItem.cs              # 任务数据模型
├── ViewModels/
│   └── TaskViewModel.cs         # MVVM 视图模型
├── Views/
│   └── AddEditTaskDialog.xaml   # 添加/编辑对话框
├── Services/                     # 新增服务层
│   ├── TrayIconService.cs       # 系统托盘服务
│   ├── ReminderChecker.cs       # 定时检查服务
│   └── NotificationService.cs   # 通知服务
├── Data/
│   └── DatabaseService.cs       # 数据访问层
├── MainWindow.xaml              # 主界面
├── App.xaml                     # 应用程序入口
└── README.md                    # 本文件
```

## 学习笔记

### 1. WPF ListView 数据绑定（MVVM 模式）

**关键点：**
- 使用 `ObservableCollection<T>` 而非 `List<T>`，这样集合变化时会自动通知 UI
- 实现 `INotifyPropertyChanged` 接口，属性变化时触发 `PropertyChanged` 事件
- 使用 `ItemsSource` 绑定到 ViewModel 的集合属性
- 使用 `SelectedItem` 双向绑定选中的项目

**示例代码：**
```csharp
public ObservableCollection<TaskItem> Tasks { get; set; }
// ViewModel 中
Tasks.Add(newTask); // 自动更新 UI
```

### 2. MaterialDesignInXamlToolkit 控件使用

**常用控件：**
- `materialDesign:Card` - 卡片容器
- `materialDesign:PackIcon` - 图标库
- `materialDesign:HintAssist` - 浮动提示文本
- `MaterialDesignRaisedButton` - 主按钮样式
- `MaterialDesignOutlinedButton` - 边框按钮样式

**XAML 命名空间：**
```xml
xmlns:materialDesign="http://materialdesigninxaml.net/winfx/xaml/themes"
```

### 3. WPF 对话框模式

**实现方式：**
- 创建新的 `Window` 作为对话框
- 设置 `WindowStartupLocation="CenterOwner"`
- 使用 `dialog.ShowDialog()` 模态显示
- 通过 `DialogResult` 和 `ResultTask` 属性返回结果

### 4. INotifyPropertyChanged 实现

```csharp
public class TaskViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

### 5. 系统托盘集成（NotifyIcon）

**关键点：**
- 使用 `System.Windows.Forms.NotifyIcon`
- 需要在 csproj 中添加 `<UseWindowsForms>true</UseWindowsForms>`
- 托盘图标在窗口关闭时隐藏，点击托盘图标恢复窗口

**示例代码：**
```csharp
_trayIcon = new NotifyIcon
{
    Icon = System.Drawing.SystemIcons.Application,
    Text = "任务提醒",
    Visible = true
};
```

### 6. 定时检查器（System.Timers.Timer）

**关键点：**
- 使用 `System.Timers.Timer` 进行后台定时检查
- 设置间隔为 60 秒（1 分钟）
- 检查到期任务并过滤已发送提醒的任务
- 1 小时内不重复发送相同提醒

**示例代码：**
```csharp
_timer = new Timer(60000); // 60 秒
_timer.Elapsed += OnTimerElapsed;
_timer.AutoReset = true;
```

### 7. Windows Toast 通知

**关键点：**
- 使用 `Windows.UI.Notifications` 命名空间
- 需要安装 `Microsoft.Windows.SDK.BuildTools` NuGet 包
- 支持点击通知打开应用
- 降级方案：Toast 失败时使用控制台输出

**示例代码：**
```csharp
var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
var toast = new ToastNotification(toastXml);
ToastNotificationManager.CreateToastNotifier().Show(toast);
```

## 下一步计划

1. ~~系统托盘集成~~ ✅ 完成
2. ~~定时检查器~~ ✅ 完成
3. ~~通知系统~~ ✅ 完成
4. **任务完成状态切换** - 完善 UI 交互
5. **设置界面** - 用户偏好配置
6. **开机自启** - Windows 启动项配置

## 开发环境

- Visual Studio 2022 / VS Code
- .NET 8 SDK
- NuGet 包：
  - MaterialDesignThemes
  - Dapper
  - Microsoft.Data.Sqlite
  - Microsoft.Windows.SDK.BuildTools

## 使用说明

1. 点击"新建任务"按钮
2. 填写任务信息（标题、描述、到期时间等）
3. 点击"保存"
4. 任务将显示在列表中
5. 可点击任务查看详情或编辑
6. 关闭窗口时最小化到托盘
7. 任务到期时自动发送通知

---

**开发团队：** 猫工头  
**董事长指令：** 快速迭代，追求卓越
