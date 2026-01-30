/**
 * 图块集类：负责加载大图并计算所有子图块的坐标
 */
export class tileSet
{
    /**
     * 图块集的唯一标识 ID
     */
    public TileID: string;

    /**
     * 存储所有图块的坐标信息
     */
    public Tiles: Rectangle[] = [];

    /**
     * 加载后的 HTMLImageElement 对象
     */
    private image: HTMLImageElement | null = null;

    /**
     * 私有构造函数，请使用静态方法 createAsync 来实例化
     */
    private constructor(tileID: string)
    {
        this.TileID = tileID;
    }

    /**
     * 加载图片并初始化 TileSet
     * @param tileID 图块 ID (例如 "world_map")
     */
    public static async createAsync(tileID: string): Promise<tileSet>
    {
        const ts = new tileSet(tileID);
        const tileSize = 16;

        // 加载图片
        const fullPath = `./src/assets/${tileID}.png`;
        ts.image = await tileSet.loadImageAsync(fullPath);

        // 确保图片加载成功
        if (!ts.image.width || !ts.image.height)
            throw new Error(`Failed to load tileset image: ${fullPath}`);

        // 计算行列数并分割图块集
        const columns = Math.floor(ts.image.width / tileSize);
        const rows = Math.floor(ts.image.height / tileSize);

        for (let y = 0; y < rows; y++)
        for (let x = 0; x < columns; x++)
        {
            ts.Tiles.push({
                x: x * tileSize,
                y: y * tileSize,
                width: tileSize,
                height: tileSize
            });
        }

        return ts;
    }

    /**
     * 将指定索引的图块绘制到 Canvas 上
     *
     * @param ctx Canvas 渲染上下文
     * @param tileIndex 图块在 Tiles 数组中的索引
     * @param destX 目标 Canvas 的 X 坐标
     * @param destY 目标 Canvas 的 Y 坐标
     */
    public drawMap(ctx: CanvasRenderingContext2D, tileIndex: number, destX: number, destY: number): void
    {
        if (!this.image)
        {
            console.error("TileSet image not loaded!");
            return;
        }

        // 边界检查
        if (tileIndex < 0 || tileIndex >= this.Tiles.length)
        {
            console.warn(`TileIndex ${tileIndex} out of bounds.`);
            return;
        }

        // 透明层
        if (tileIndex == 0)
            return;

        const source = this.Tiles[tileIndex - 1];

        // 绘制图片
        ctx.drawImage(
            this.image,
            source.x, source.y, source.width, source.height, // 源矩形（在大图里的位置）
            destX, destY, source.width, source.height       // 目标矩形（在 Canvas 里的位置）
        );
    }

    /**
     * 私有辅助方法：封装图片加载逻辑为 Promise
     */
    private static loadImageAsync(src: string): Promise<HTMLImageElement>
    {
        return new Promise((resolve, reject) => {
            const img = new Image();
            img.onload = () => resolve(img);
            img.onerror = (err) => reject(err);
            img.src = src;
        });
    }
}

/**
 * 表示单个图块的元数据（位置信息）
 */
interface Rectangle
{
    x: number;
    y: number;
    width: number;
    height: number;
}
