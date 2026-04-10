# 构建和运行说明

## 环境要求

- **操作系统**: Windows 10/11
- **.NET SDK**: .NET 8.0 或更高版本
- **IDE**: Visual Studio 2022 或 JetBrains Rider

## 安装步骤

### 1. 安装 .NET 8 SDK

#### Windows:
1. 访问 [https://dotnet.microsoft.com/download/dotnet/8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
2. 下载并安装 .NET 8.0 SDK
3. 验证安装:
   ```bash
   dotnet --version
   ```

#### 使用脚本安装:
```bash
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh
bash dotnet-install.sh --channel 8.0
export PATH="$PATH:$HOME/.dotnet"
```

### 2. 克隆项目

```bash
git clone https://github.com/happyallday/photoshop.git
cd photoshop
```

## 构建项目

### 使用命令行:

```bash
# 还原依赖包
dotnet restore

# 构建解决方案
dotnet build

# 清理和重新构建
dotnet clean && dotnet build
```

### 使用 Visual Studio:

1. 打开 `PhotoshopApp.sln`
2. 选择 `Build` -> `Build Solution` (或按 `Ctrl+Shift+B`)
3. 等待构建完成

### 使用 Rider:

1. 打开 `PhotoshopApp.sln`
2. 点击 `Build` -> `Build Solution`
3. 确认构建成功

## 运行应用

### 使用命令行:

```bash
# 运行应用程序
dotnet run --project PhotoshopApp/PhotoshopApp.csproj

# 或者在解决方案根目录
dotnet run
```

### 使用 Visual Studio:

1. 选择 `PhotoshopApp` 作为启动项目
2. 点击 `Start Debugging` (或按 `F5`)
3. 或者点击 `Start Without Debugging` (或按 `Ctrl+F5`)

### 使用 Rider:

1. 右键点击 `PhotoshopApp` 项目
2. 选择 `Run 'PhotoshopApp'`
3. 或使用 `Run` -> `Run 'PhotoshopApp'` (Shift+F10)

## 开发调试

### 调试配置

#### Visual Studio:
1. 选择 `Debug` 配置
2. 设置断点
3. 按 `F5` 开始调试

#### Rider:
1. 选择 `Debug` 配置
2. 设置断点
3. 点击调试按钮

### 日志输出

应用程序的日志和控制台输出会显示在:
- Visual Studio: Output 窗口
- Rider: Run 窗口
- 命令行: 控制台输出

## 常见问题

### 问题1: 找不到 .NET SDK

**解决方案**:
```bash
# 检查已安装的SDK
dotnet --list-sdks

# 如果没有找到，重新安装 .NET 8 SDK
```

### 问题2: NuGet 包还原失败

**解决方案**:
```bash
# 清理 NuGet 缓存
dotnet nuget locals all --clear

# 重新还原
dotnet restore
```

### 问题3: WPF 窗口无法显示

**解决方案**:
- 确保正在 Windows 系统上运行
- 检查 .NET 8 Windows Desktop SDK 是否已安装
- 确认 `PhotoshopApp.csproj` 中的 `<UseWindowsForms>true</UseWindowsForms>` 或 `<UseWPF>true</UseWPF>`

### 问题4: 图像加载失败

**解决方案**:
- 确认已正确安装 ImageSharp 包
- 检查文件路径是否正确
- 确认图像文件格式受支持 (PNG, JPG, BMP, GIF等)

## 项目结构

```
PhotoshopApp/
├── PhotoshopApp.sln              # 解决方案文件
├── PhotoshopApp/                 # 主应用程序
│   ├── App.xaml                  # 应用程序入口
│   ├── Views/                    # 窗口和视图
│   ├── ViewModels/               # MVVM 视图模型
│   ├── Controls/                 # 自定义控件
│   ├── Services/                 # 服务实现
│   └── Commands/                 # 命令处理
└── PhotoshopApp.Core/           # 核心业务逻辑
    ├── Layers/                   # 图层管理
    ├── Effects/                  # 滤镜效果
    ├── History/                  # 编辑历史
    ├── ImageProcessing/          # 图像处理
    └── FileIO/                   # 文件操作
```

## 发布应用程序

### 发布为单文件应用:

```bash
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

### 发布为依赖框架的应用:

```bash
dotnet publish -c Release -r win-x64
```

生成的可执行文件位于:
`PhotoshopApp/bin/Release/net8.0-windows/win-x64/publish/`

## 性能调优

### 优化构建时间:
```bash
# 禁用增量构建以避免缓存问题
dotnet build --no-incremental

# 使用并行构建
dotnet build -m
```

### 优化运行时性能:
- 使用 `Release` 配置而非 `Debug`
- 考虑使用 `PublishTrimmed` 减小应用大小

## 下一步

查看 [README.md](README.md) 了解项目特性和使用说明。

## 技术支持

如有问题，请在 [GitHub Issues](https://github.com/happyallday/photoshop/issues) 中提交。