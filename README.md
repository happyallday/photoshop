# Photoshop Clone - WPF图像处理工具

基于WPF和.NET 8开发的现代图像处理应用，目标提供类似Photoshop的图片编辑功能，支持Windows平台。

## 技术栈

- **框架**: .NET 8 + WPF
- **架构**: MVVM (CommunityToolkit.Mvvm)
- **图像处理**: ImageSharp + OpenCvSharp4
- **渲染**: WriteableBitmap + ShaderEffect
- **依赖注入**: Microsoft.Extensions.DependencyInjection

## 核心功能

### 基础编辑
- 图像加载与保存 (PNG/JPG/BMP等多种格式)
- 裁剪、旋转、翻转等基础变换
- 亮度、对比度、饱和度调整

### 图层管理
- 多图层支持
- 图层可见性和透明度控制
- 图层顺序调整
- 多种混合模式

### 绘图工具
- 画笔、橡皮擦工具
- 形状工具（矩形、圆形、线条）
- 填充工具

### 滤镜效果
- 模糊、锐化滤镜
- 艺术效果滤镜
- 实时预览

### 专业功能
- 编辑历史与撤销重做
- 图层蒙版
- 调整图层
- 项目文件保存

## 项目架构

```
PhotoshopApp/
├── Core/                      # 核心业务逻辑
│   ├── ImageProcessing/       # 图像处理
│   ├── Layers/                # 图层管理  
│   ├── Effects/               # 滤镜效果
│   ├── History/               # 编辑历史
│   └── FileIO/                # 文件操作
├── UI/                        # 用户界面
│   ├── ViewModels/            # MVVM
│   ├── Views/                 # WPF Views
│   ├── Controls/              # 自定义控件
│   └── Converters/            # 数据转换
├── Services/                  # 服务层
└── Infrastructure/            # 基础设施
```

## 开发里程碑

- ✅ 1. **项目基础框架** - WPF应用框架和MVVM架构 *(已完成)*
- ✅ 2. **图像加载与渲染** - 基础图像显示和变换 *(已完成)*
- ✅ 3. **图层管理系统** - 多图层功能和图层面板 *(已完成)*
- 🚧 4. **基础图像编辑** - 裁剪、调色等基础工具 *(进行中)*
- ⏳ 5. **绘图工具系统** - 画笔、形状等绘图功能
- ⏳ 6. **滤镜效果系统** - 各类图像滤镜
- ⏳ 7. **编辑历史管理** - 撤销重做功能
- ⏳ 8. **高级图层功能** - 蒙版、调整图层等
- ⏳ 9. **文件导入导出** - 完整的文件操作
- ⏳ 10. **用户体验优化** - 性能和易用性提升

## 快速开始

### 环境要求
- .NET 8 SDK
- Windows 10/11
- Visual Studio 2022 或 Rider

### 构建运行
```bash
# 克隆项目
git clone https://github.com/happyallday/photoshop.git
cd photoshop

# 还原依赖
dotnet restore

# 构建项目
dotnet build

# 运行应用
dotnet run
```

详细的构建和运行说明请查看 [BUILD.md](BUILD.md)

## 开发指南

### 代码规范
- 遵循C#编码约定
- 使用MVVM模式分离UI和逻辑
- 保持接口清晰，便于测试和扩展

### Git提交规范
```
feat: 新功能
fix: 修复bug
refactor: 重构代码
docs: 文档更新
test: 测试相关
```

## 贡献指南

欢迎贡献代码！请遵循以下步骤：
1. Fork本项目
2. 创建功能分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'feat: Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 开启Pull Request

## 许可证

本项目采用 MIT 许可证

## 联系方式

- 项目主页: https://github.com/happyallday/photoshop
- 问题反馈: https://github.com/happyallday/photoshop/issues