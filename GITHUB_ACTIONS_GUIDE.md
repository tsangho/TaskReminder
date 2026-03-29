# GitHub Actions 自动编译指南

## 📋 概述

本项目已配置 GitHub Actions 自动编译流程，每次提交代码后会自动：
- ✅ 在 Windows 环境编译
- ✅ 生成独立版 .exe（无需安装 .NET）
- ✅ 生成依赖框架版 .exe（需 .NET 8）
- ✅ 打包成 ZIP 文件
- ✅ 上传到 Actions 页面供下载

---

## 🚀 使用方法

### 方法 1：自动触发（推荐）

1. **提交代码到 GitHub**
   ```bash
   git add .
   git commit -m "feat: 添加新功能"
   git push origin main
   ```

2. **等待编译完成**
   - 访问：https://github.com/YOUR_USERNAME/TaskReminder/actions
   - 点击最新的 workflow 运行记录
   - 等待编译完成（约 3-5 分钟）

3. **下载编译产物**
   - 在 workflow 页面底部找到 "Artifacts" 区域
   - 选择下载：
     - `TaskReminder-standalone-win-x64` - 独立版（推荐，无需 .NET）
     - `TaskReminder-framework-dependent-win-x64` - 依赖框架版
     - `TaskReminder-win-x64-ZIP` - ZIP 压缩包

### 方法 2：手动触发

1. **访问 Actions 页面**
   - 打开：https://github.com/YOUR_USERNAME/TaskReminder/actions/workflows/build.yml
   - 点击 "Run workflow" 按钮
   - 选择分支（默认 main）
   - 点击 "Run workflow"

2. **等待并下载**
   - 等待编译完成
   - 下载 Artifacts

---

## 📦 产物说明

### 1. 独立版（Standalone）
- **文件名**: `TaskReminder.exe`
- **大小**: 约 60-80 MB
- **依赖**: 无需安装 .NET
- **适用**: 所有 Windows 10/11 用户
- **推荐**: ✅ 首选此版本

### 2. 依赖框架版（Framework-dependent）
- **文件名**: `TaskReminder.exe`
- **大小**: 约 5-10 MB
- **依赖**: 需安装 .NET 8.0 Desktop Runtime
- **适用**: 已安装 .NET 8 的用户
- **下载**: .NET 8 Runtime: https://dotnet.microsoft.com/download/dotnet/8.0

### 3. ZIP 压缩包
- **文件名**: `TaskReminder-win-x64.zip`
- **内容**: 独立版所有文件
- **适用**: 方便分发和备份

---

## 🔧 自定义配置

### 修改触发条件

编辑 `.github/workflows/build.yml`：

```yaml
on:
  push:
    branches: [ main, develop ]  # 修改分支
  schedule:
    - cron: '0 2 * * *'  # 每天 UTC 2:00 自动编译
```

### 修改输出版本

```yaml
- name: Publish (Standalone)
  run: |
    dotnet publish TaskReminder.csproj `
      --runtime win-x64 `
      --self-contained true `
      -p:PublishSingleFile=true  # 单文件发布
```

### 添加测试

```yaml
- name: Run Tests
  run: dotnet test --no-build
```

---

## 📊 构建产物保留策略

- **默认保留**: 30 天
- **自动删除**: 超过 30 天的构建产物
- **永久保存**: 可手动下载后本地备份

---

## ⚠️ 注意事项

1. **GitHub 免费额度**
   - 公共仓库：无限制
   - 私有仓库：每月 2000 分钟免费额度
   - 每次编译约 3-5 分钟

2. **Windows 版本**
   - 使用 GitHub 的 `windows-latest` 运行器
   - 当前为 Windows Server 2022

3. **.NET 版本**
   - 固定使用 .NET 8.0.x
   - 自动获取最新补丁版本

4. **产物下载**
   - 需要登录 GitHub 账号
   - 产物保留 30 天，及时下载

---

## 🐛 故障排查

### 问题 1：编译失败
**报错**: `error NUxxxx: Unable to find package`
**解决**: 
- 检查网络连接
- 确认 NuGet 源配置正确
- 尝试重新运行 workflow

### 问题 2：产物下载失败
**现象**: 点击下载无反应
**解决**:
- 确认已登录 GitHub
- 检查浏览器是否拦截下载
- 尝试右键 → 另存为

### 问题 3：运行时报错
**报错**: `A fatal error was encountered. The library 'hostfxr.dll' was not found.`
**原因**: 下载了依赖框架版但未安装 .NET 8
**解决**: 
- 安装 .NET 8 Desktop Runtime
- 或下载独立版（Standalone）

---

## 📞 支持

如遇到问题，请：
1. 查看 GitHub Actions 日志
2. 检查编译错误信息
3. 提交 Issue 或联系维护者

---

**最后更新**: 2026-03-30
**维护者**: 猫工头 & 猫经理
