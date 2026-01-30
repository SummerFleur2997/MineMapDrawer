此目录存储有 MapDrawer MediaWiki Widget 版本的源代码，需要 TypeScript 环境。

**使用方法**

编译目录已默认被 `.gitignore` 排除，因此需要手动编译，若没有安装 TypeScript，请先安装：

```bash
npm install -D typescript
```

在 [tileSet.ts](./src/tileSet.ts) 文件中，默认使用的图片源位于 bwiki，若要使用其他 wiki 站点的源，需要更改 [wikiImageUrl](./src/tileSet.ts#L110) 字典以及 [fullUrl](./src/tileSet.ts#L153) 变量。

然后编译：
```bash
tsc
```

编译的文件会生成在 `./dist` 目录下，然后将 `./dist` 目录下的三个 js 文件上传至 MediaWiki 站点，最后新建一个 Widget，将 [index.html](./index.html) 中 `<body>` 部分的代码复制到 Widget 的 HTML 代码框中，修改 script 的引用路径，保存即可。