# Flash 小游戏启动器

一个 Windows 桌面小工具：把 SWF / EXE / 快捷方式丢进文件夹，就能像游戏库一样浏览和启动。

**文件夹即分类** —— 在 `games/` 下新建一个子文件夹就是一个新分类，不需要手动维护任何清单或配置文件。

## 功能

- 文件夹即分类，支持进入子文件夹、逐级返回
- 列表视图 / 图标网格视图切换
- 深色 / 浅色主题切换
- 为分类或单个游戏自定义图标（也会自动匹配 `icons/` 下同名的图片文件）
- 搜索过滤、右键菜单（打开所在文件夹、删除到回收站等）
- 选中项高亮、全屏、圆角窗口、跟随系统 DPI 缩放

## 使用方法

1. 下载 Release 中的 `Flash小游戏启动器.exe`
2. 放到任意目录（建议单独建一个文件夹）
3. 双击运行，首次运行会自动生成 `games/`、`icons/` 两个文件夹
4. 把游戏放进 `games/`：**新建子文件夹 = 新建分类**

支持的文件类型：`.swf`、`.exe`、`.lnk`、`.url`

## 从源码构建

需要 [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)（Windows 上构建）。

```bash
dotnet build -c Release

# 打包成单个 exe（自包含，目标机器无需安装 .NET 运行环境）
dotnet publish -c Release -r win-x64 --self-contained true \
  -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true \
  -p:EnableWindowsTargeting=true -p:DebugType=none -o ../pub
```

如果只想要一个小体积的 exe（目标机器需自行安装 .NET 10 桌面运行环境），把 `--self-contained true` 改成 `false`。

## 目录结构

```
Flash小游戏启动器.csproj   项目文件
MainForm.cs               主窗口：工具栏、右键菜单、扫描与刷新、图标
GamePanel.cs              游戏区：列表 / 网格绘制、滚动、选中
CatList.cs                左侧分类栏
AppTheme.cs               深色 / 浅色主题配色
Dpi.cs                    DPI 缩放
Gfx.cs                    圆角等绘制辅助
Properties/               程序集信息
app.ico / app.manifest    图标与清单
```

## 说明

- 本仓库**只包含启动器源码，不包含任何游戏文件**。
- 程序运行时会在 exe 同目录生成 `theme.txt`、`view.txt`、`sidebar.txt`、`icon_map.json` 作为状态记录，这些是本机数据，已在 `.gitignore` 中排除。

## 许可证

**允许个人非商业使用，禁止任何形式的商业用途。**

- 允许：个人免费下载、安装、运行；出于学习研究目的阅读源码、为自己使用而修改（修改版不得分发）
- 禁止：任何商业用途；再发布或分发本软件及其修改版本；移除版权声明

完整条款见 [LICENSE](LICENSE) 文件；如需商业授权，请通过 Issue 联系作者。
