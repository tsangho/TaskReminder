# TaskReminder - 快速开始指南

## 构建和运行

```bash
cd /home/ho_tsang/task-reminder/TaskReminder
dotnet build
dotnet run
```

## 项目文件清单

### 核心文件（必须）
- ✅ `TaskReminder.csproj` - 项目文件
- ✅ `App.xaml` / `App.xaml.cs` - 应用程序入口
- ✅ `MainWindow.xaml` / `MainWindow.xaml.cs` - 主界面
- ✅ `Models/TaskItem.cs` - 任务数据模型
- ✅ `Data/DatabaseService.cs` - 数据库服务
- ✅ `ViewModels/TaskViewModel.cs` - MVVM 视图模型
- ✅ `Views/AddEditTaskDialog.xaml` / `.cs` - 添加/编辑对话框

### 文档文件
- 📄 `README.md` - 项目说明
- 📄 `DEVELOPMENT_PROGRESS.md` - 开发进度报告
- 📄 `QUICK_START.md` - 本文件

## 主要功能

### 1. 任务列表（主界面左侧）
- 显示所有任务的卡片列表
- 每个卡片显示：标题、描述、到期时间、重复类型
- 支持直接删除任务
- 支持点击选择查看详情

### 2. 任务详情（主界面右侧）
- 显示选中任务的完整信息
- 提供编辑和删除按钮
- 未选择时显示提示文字

### 3. 添加/编辑任务对话框
- 标题（必填，最多 100 字符）
- 描述（可选，最多 500 字符）
- 到期日期（DatePicker）
- 到期时间（小时 + 分钟选择器）
- 重复类型（不重复/每天/每周/每月/每季度/每年）
- 重复间隔（数字）

## 技术架构

```\n┌─────────────────┐\n│    MainWindow   │  ← View（XAML + Code-behind）\n│                 │\n│  +------------+ │\n│  │ TaskViewModel │  ← ViewModel（MVVM）\n│  +------------+ │\n│         │       │\n│         ▼       │\n│  +------------+ │\n│  │ TaskItem   │  ← Model\n│  +------------+ │\n│         │       │\n│         ▼       │\n│  +------------+ │\n│  │ Database   │  ← Data Layer (SQLite + Dapper)\n│  +------------+ │\n└─────────────────┘\n```\n\n## 数据流

1. **加载任务**：`TaskViewModel.LoadTasksAsync()` → 从数据库读取 → 填充 `ObservableCollection` → UI 自动更新
2. **添加任务**：用户输入 → `AddEditTaskDialog` → `TaskViewModel.AddTaskAsync()` → 写入数据库 → UI 自动更新
3. **编辑任务**：选择任务 → 打开对话框（带数据）→ 用户修改 → `TaskViewModel.UpdateTaskAsync()` → 更新数据库 → UI 自动更新
4. **删除任务**：点击删除 → 确认 → `TaskViewModel.DeleteTaskAsync()` → 从数据库删除 → UI 自动更新

## 关键代码示例

### 数据绑定（XAML）
```xml
<ListView ItemsSource="{Binding Tasks}" \n          SelectedItem="{Binding SelectedTask}\"\n          SelectionChanged=\"TaskListView_SelectionChanged\">\n    <!-- ItemTemplate 定义项目显示样式 -->\n</ListView>\n```\n\n### ViewModel 属性通知\n```csharp\npublic ObservableCollection<TaskItem> Tasks\n{\n    get => _tasks;\n    set { _tasks = value; OnPropertyChanged(); }\n}\n```\n\n### 对话框使用\n```csharp\nvar dialog = new AddEditTaskDialog();\ndialog.Owner = this;\nif (dialog.ShowDialog() == true)\n{\n    var newTask = dialog.ResultTask;\n    await viewModel.AddTaskAsync(newTask);\n}\n```\n\n## 下一步开发建议

### 立即可做
1. 运行并测试 UI
2. 实现系统托盘（NotifyIcon）
3. 添加后台定时检查器
4. 实现通知提醒

### 优化方向
1. 添加加载动画
2. 添加搜索/过滤功能
3. 添加任务分类/标签
4. 添加深色模式
5. 添加设置界面

## 常见问题

### Q: 如何修改数据库路径？
A: 在 `MainWindow.xaml.cs` 中修改 `DatabaseService` 的构造函数参数：
```csharp\n_databaseService = new DatabaseService(\"tasks.db\"); // 修改路径\n```\n\n### Q: 如何更改主题颜色？
A: MaterialDesignInXamlToolkit 支持多种主题，修改 `App.xaml` 中的主题设置。\n\n### Q: 如何添加新的字段？\nA: 1. 在 `TaskItem.cs` 添加属性\n2. 在数据库迁移脚本中添加列\n3. 在 XAML 中添加对应的输入控件\n4. 在 ViewModel 中添加绑定\n\n---\n\n**开发完成时间：** 2026-03-29 23:36  \n**状态：** 基础 UI 完成，等待测试
