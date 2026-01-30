/**
 * 图块集类：负责加载大图并计算所有子图块的坐标
 */
class tileSet
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

        ts.image = await resourceHelper.loadImageAsync(tileID);

        // 确保图片加载成功
        if (!ts.image.width || !ts.image.height)
            throw new Error(`Failed to load tileset image: ${tileID}`);

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
}

class resourceHelper
{
    /**
     * 图片缓存字典。
     */
    public static cachedImage: Record<string, HTMLImageElement> = {};

    /**
     * 图片 URL 字典，对应 wiki 上图集的 url
     */
    private static wikiImageUrl: Record<string, string> = {
        "mine": "a/a0/65r3e21i6s3lxm6k2tiazdj1sroadqb",
        "mine_dark": "5/5d/8ptcwesxswlvat5ozag8nb9finlwqcd",
        "mine_dangerous": "b/b2/c22ogfbjv54mrtkwhramiwmf8zsq793",
        "mine_dark_dangerous": "f/f5/47mc92hr3lvkz01piockwbin842aq74",

        "mine_frost": "6/61/4chirqhh0rlhwjaze9qe2ztfllv68cz",
        "mine_frost_dark": "4/4e/m4n1ureiceithkmq5wmgce7fkkywqso",
        "mine_frost_dangerous": "c/ca/ej53ccmr30ql66qx5hwa9ock7vm4g8i",
        "mine_frost_dark_dangerous": "4/44/p3ga1xxmweq9i89jjtefg7tjmusfoo5",

        "mine_lava": "c/c2/3y1peft41dck9db0ib97nb1iztmtk6b",
        "mine_lava_dark": "7/73/idm60ubxikf9vcgbxwb74qcwrzd6evx",
        "mine_lava_dangerous": "1/11/adbi1a8ddikfxoh7fiz6t8tc59rccsj",
        "mine_lava_dark_dangerous": "8/89/j91ewq4m292tgvrjelqjgz6jb23k4ar",

        "mine_desert": "d/d2/pi11x2mnbnajvojdk00woydp1xrsgrq",
        "mine_desert_dark": "6/64/lyk273uji6idvdmeaetzm4wodr1i4q7",
        "mine_desert_dangerous": "f/f9/jmkc65mkgpp5ikd3mv7b28gwf98tflq",
        "mine_desert_dark_dangerous": "e/e1/kf5jr54z8agrnof0j80wfahdcie744n",

        "mine_slime": "4/48/9my9b9wuhic2w1ya34nxythbeoy581g",
        "mine_slime_dangerous": "1/10/pml4ocqdr8c64espgqyt55msa8jwvlq",
        "mine_dino": "5/5e/8dw2h9saposuh7mekf2evo85fcyjcfv",
        "mine_quarryshaft": "6/62/l2jbeojwfm1em9i1pmrxi3nzwz3tu60",
    };

    /**
     * 获取图片，优先从缓存读取，未缓存则异步加载
     * @param imageId 图片标识符（对应 wikiImageUrl 的键）
     */
    public static async loadImageAsync(imageId: string): Promise<HTMLImageElement>
    {
        if (this.cachedImage[imageId])
        {
            console.log("Use cached image: " + imageId);
            return this.cachedImage[imageId];
        }

        const relativePath = this.wikiImageUrl[imageId];
        if (!relativePath)
            return Promise.reject(new Error(`Image ID not found: ${imageId}`));

        const fullUrl = `https://patchwiki.biligame.com/images/stardewvalley/${relativePath}.png` ;
        return new Promise((resolve, reject) =>
        {
            const img = new Image();
            img.onload = () =>
            {
                this.cachedImage[imageId] = img;
                console.log("Load image from wiki: " + imageId);
                resolve(img);
            };
            img.onerror = (err) => reject(err);
            img.src = fullUrl;
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
