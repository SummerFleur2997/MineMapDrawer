此目录存储有 MapDrawer C# CLI 版本的源代码。

**常规使用方法**

1. 从游戏目录 `Content\Maps\Mines` 下解包所有以 `mine` 开头的 png 文件；
2. 在 [`Program.cs`](./MapDrawer/Program.cs)  所在目录下创建 `assets` 文件夹，并将解包的 png 文件放入其中；
3. 使用 Release 构建，然后双击运行 exe，即可将图片绘制到 output 文件夹内。

**导出自定义二进制文件**

此工具亦可用于导出二进制文件及其 base64 编码，用于 [MediaWiki Widget](../mediawiki%20widget) 中，以下是操作方法：

1. 按照常规使用方法的 1, 2 两步执行构建操作；
2. 构建完成后右击资源管理器，在命令行中打开，然后输入以下命令：
    ```bash
    ./MapDrawer.exe export
    ```
3. 命令执行完毕后目录下会生成 `.maps`（gzip 压缩后的二进制）和 `maps_base64.txt`（base64 编码）两个文件，不会绘制图片。