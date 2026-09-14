# Virtual Dimension 虚拟维度塔

Galaxy-shared storage dimension for Dyson Sphere Program. 全星系共享的虚拟维度存储空间。

Turns the vanilla Interstellar Logistic Station (ILS) into a "Virtual Dimension Tower" — no new buildings, fully vanilla-compatible. 把原版星际物流运输站直接变成虚拟维度塔，不新增任何建筑。

## Features 功能

- **Galaxy-shared dimension storage** — upload at any ILS, withdraw at any ILS, in any star system. 全星系共享存储，任意星球存取。
- **星际供应 (interstellar supply)** slot: items above half of the slot cap flow into the dimension — local logistics always have priority. Supply slots never pull from the dimension. 星际供应槽把超过槽位上限一半的部分存入维度（本地优先，只出不进）。
- **星际需求 (interstellar demand)** slot: pulls from the dimension up to half of the slot cap. Only stations you explicitly set to interstellar demand ever receive dimension items. 星际需求槽从维度补货至上限一半；只有明确设为星际需求的站点才会收到维度物品。
- Cross-planet transfer costs only power — the dimension link needs no ships or drones. 跨星球传输只耗电，无需飞船。
- Local drone logistics keep working exactly like vanilla and combine freely with the dimension (e.g. 本地需求 + 星际供应 = drones gather local goods, surplus uploads). 本地小飞机物流与原版一致，可自由组合。
- **Alt+5** opens the dimension inventory: browse stored items, search, and set per-item caps 0 ~ 10,000,000 (buildings default 50, items default 2,000,000). The window is resizable — drag the bottom-right corner. Alt+5 打开维度界面：查看库存、搜索、设置每种物品上限（建筑默认 50，物品默认 200 万），窗口可拖拽调整大小。
- Saves and loads with your game via DSPModSave. 通过 DSPModSave 随存档保存。

## Usage 使用

1. Build an ILS and keep it powered. 建造星际物流运输站并保持通电。
2. Set a slot to 星际供应 (interstellar supply): everything above half cap uploads into the dimension. 设为星际供应：超过一半的部分自动进入维度。
3. On another planet, set a slot to 星际需求 (interstellar demand): the dimension refills it up to half cap. 另一星球设为星际需求：自动从维度补货。
4. Press **Alt+5** anywhere to inspect and configure the dimension. 随时按 Alt+5 查看和设置维度空间。

## Installation 安装

- Recommended: install via the Thunderstore Mod Manager — dependencies are resolved automatically. 推荐用 Mod Manager 一键安装。
- Manual: install BepInEx, CommonAPI and CommonAPI-DSPModSave first, then copy the `BepInEx/plugins/VirtualDimension` folder into the game's `BepInEx` folder. 手动安装：先装 BepInEx、CommonAPI、CommonAPI-DSPModSave，再把 `BepInEx/plugins/VirtualDimension` 复制到游戏的 `BepInEx` 目录。

## Configuration 配置

| Setting | Default | Description |
| --- | --- | --- |
| `General / TransferPerTick` | `3600` | Items moved per tick (60 ticks/s) between a tower slot and the dimension. |
| `UI / WindowWidth` | `1000` | Saved width of the Alt+5 window. |
| `UI / WindowHeight` | `620` | Saved height of the Alt+5 window. |

## Requirements 环境

- Dyson Sphere Program 0.10.x (tested on 0.10.34)
- BepInEx 5.4.5+, CommonAPI 1.6.7, CommonAPI-DSPModSave 1.2.2
