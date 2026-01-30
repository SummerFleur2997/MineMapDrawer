此目录存储有 MapDrawer C# CLI 版本的源代码。

**常规使用方法**

1. 从游戏目录 `Content\Maps\Mines` 下解包所有以 `mine` 开头的 png 文件；
2. 下载 Release 版本，并解压到任意目录；
3. 在 `Program.exe`  所在目录下创建 `assets` 文件夹，并将解包的 png 文件放入其中；
4. 双击运行 exe，即可将图片绘制到 output 文件夹内，需要 NET 6 运行时。

**导出自定义二进制文件**

此工具亦可用于导出二进制文件及其 base64 编码，用于 [MediaWiki Widget](../mediawiki%20widget) 中，以下是操作方法：

下载 Release 版本并解压到任意目录，右击资源管理器，在命令行中打开，然后输入以下命令：
```bash
./MapDrawer.exe export
```

命令执行完毕后目录下会生成 `.maps`（gzip 压缩后的二进制）和 `maps_base64.txt`（base64 编码）两个文件，不会绘制图片。