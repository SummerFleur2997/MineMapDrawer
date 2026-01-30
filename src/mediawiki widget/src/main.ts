import { map, allMapData } from './map';
import { tileSet } from './tileSet';

let maps: map[] = new Array<map>;

async function renderMaps(): Promise<void>
{
    // 矿井区域
    const regionSelect = document.getElementById('region') as HTMLSelectElement;
    const regionValue = regionSelect.value;

    // 是否黑暗层
    const darkCheckbox = document.getElementById('dark') as HTMLInputElement;
    const isDark = darkCheckbox.checked;

    // 是否危险
    const dangerousCheckbox = document.getElementById('dangerous') as HTMLInputElement;
    const isDangerous = dangerousCheckbox.checked;

    // 根据条件拼接字符串
    let tileID = "mine";

    if (regionValue)
        tileID += `_${regionValue}`;

    if (isDark && regionValue !== "slime" && regionValue !== "dino" && regionValue !== "quarryshaft")
        tileID += "_dark";

    if (isDangerous && regionValue !== "dino" && regionValue !== "quarryshaft")
        tileID += "_dangerous";

    console.log(`Tile ID: ${tileID}`)

    // 获取 canvas 元素和 2D 上下文
    let canvas: HTMLCanvasElement;
    let ctx: CanvasRenderingContext2D;

    canvas = document.getElementById("mapCanvas") as HTMLCanvasElement;
    if (!canvas)
        throw new Error("Canvas not found.");

    let c = canvas.getContext('2d');
    if (!c)
        throw new Error("Failed to get 2D context.");
    ctx = c;
    console.log("Successfully got 2D context, ready to draw map.");

    const mapIndexInput = document.getElementById('mapIndex') as HTMLInputElement;
    const mapIndex = parseInt(mapIndexInput.value);
    const map = maps[mapIndex]
    let ts: tileSet;
    ts = await tileSet.createAsync(tileID);

    canvas.width = map.Width * 16;
    canvas.height = map.Height * 16;

    // 清空上一帧的内容（对应新建一张白纸）
    ctx.clearRect(0, 0, canvas.width, canvas.height);

    // 定义要绘制的图层顺序
    const layers = [map.BackLayerData, map.BuildingsLayerData, map.FrontLayerData];

    // 按顺序绘制每个图层
    for (const layerData of layers)
    {
        for (let y = 0; y < map.Height; y++)
        for (let x = 0; x < map.Width; x++)
        {
            const index = layerData[y][x];

            // 跳过无效索引
            if (index < 0 || index >= ts.Tiles.length)
                continue;

            // 计算目标位置
            const destX = x * 16;
            const destY = y * 16;

            // 调用 TileSet 的绘制方法
            ts.drawMap(ctx, index, destX, destY);
        }
    }
}

/**
 * 解码 base64 数据为 map 对象，然后填入 maps 数组中。
 */
async function decodeMaps(): Promise<void>
{
    // 解码 base64 数据
    const str = atob(allMapData);
    const buf = new Uint8Array(str.length);
    for (let i = 0; i < str.length; i++)
        buf[i] = str.charCodeAt(i);

    // 解压 gzip
    const decompressionStream = new DecompressionStream('gzip');
    const stream = new Response(buf).body?.pipeThrough(decompressionStream);
    if (!stream)
        throw new Error('Failed to create stream.');

    // 创建二进制 data view
    const buffer = await new Response(stream).arrayBuffer();
    const view = new DataView(buffer);

    // 校验文件头
    const header = view.getUint32(0);
    if (header !== 0x4D415053)
        throw new Error('Invalid map data.');

    // 读取地图数据
    for (let i = 0; i < 59; i++)
    {
        const offset = 8 + i * 8;
        const length = view.getUint16(offset + 2, true);
        const offsetValue = view.getUint32(offset + 4, true);
        const chunk = new DataView(buffer, offsetValue, length);
        maps[i] = new map(chunk);
    }
    console.log("Successfully load all data");
}

console.log("111")
decodeMaps().catch(err => console.error(err));

document.getElementById('start')?.addEventListener('click',
    () => {renderMaps().catch(console.error)} );
