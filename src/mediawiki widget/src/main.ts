let maps: map[] = new Array<map>;
let outputFileName: string;

/**
 * 获取图块集 Id
 */
function getTileId(): string
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

    return tileID;
}

/**
 * 获取地图索引
 */
function getMapIndex(): number
{
    const mapIndexInput = document.getElementById('mapIndex') as HTMLInputElement;
    return parseInt(mapIndexInput.value);
}

/**
 * 在 canvas 上绘制地图
 */
async function renderMaps(): Promise<void>
{
    // 获取 canvas 元素和 2D 上下文
    const canvas = document.getElementById("mapCanvas") as HTMLCanvasElement;
    if (!canvas)
        throw new Error("Canvas not found.");

    const ctx = canvas.getContext('2d') as CanvasRenderingContext2D;
    if (!ctx)
        throw new Error("Failed to get 2D context.");

    const tileId = getTileId();
    const mapIndex = getMapIndex();
    const map = maps[mapIndex - 1];
    let ts: tileSet;
    ts = await tileSet.createAsync(tileId);

    canvas.width = map.Width * 16;
    canvas.height = map.Height * 16;

    // 清空上一帧的内容，然后涂黑
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    ctx.fillStyle = 'black';
    ctx.fillRect(0, 0, canvas.width, canvas.height);
    console.log("Canvas cleared.");

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
    console.log(`Successfully draw the map: ${mapIndex} (${tileId}) \n`);
    outputFileName = `${tileId}_${mapIndex}.png`;
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
    autoRender();
}

function registerListener(id: string): void
{
    const el = document.getElementById(id);
    if (!el) return;

    const eventType = el.tagName === 'INPUT' && (el as HTMLInputElement).type === 'number'
        ? 'input'
        : 'change';

    el.addEventListener(eventType, autoRender);
}

function registerClickEvent(): void
{
    const dl = document.getElementById('download');
    if (!dl) return;

    dl.addEventListener('click', saveMap);
}

function autoRender(): void
{
    renderMaps().catch(err => console.error("Failed to render the map!", err));
}

function saveMap(): void
{
    const canvas = document.getElementById("mapCanvas") as HTMLCanvasElement;
    if (!canvas)
        throw new Error("Canvas not found.");
    const link = document.createElement('a');
    link.download = outputFileName;
    link.href = canvas.toDataURL("image/png");
    link.click();
}

decodeMaps().catch(err => console.error(err));

const controlIds = ['mapIndex', 'region', 'dark', 'dangerous'];
controlIds.forEach(registerListener);
registerClickEvent();
