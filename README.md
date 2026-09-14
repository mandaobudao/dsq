# Virtual Dimension 虚拟维度塔

Galaxy-shared storage dimension for Dyson Sphere Program. 戴森球计划全星系共享存储 Mod。

Turns the vanilla Interstellar Logistic Station (ILS) into a "Virtual Dimension Tower" — no new buildings, fully vanilla-compatible. 把原版星际物流运输站直接变成虚拟维度塔，不新增任何建筑。

- **星际供应** slot: surplus above half of the slot cap uploads into the shared dimension (local logistics have priority, supply never pulls back). 星际供应槽把超过槽位上限一半的部分存入维度（本地优先，只出不进）。
- **星际需求** slot: pulls from the dimension up to half of the slot cap — only stations you explicitly set to remote demand receive items. 星际需求槽从维度补货至上限一半；只有明确设为星际需求的站点才会收到维度物品。
- Cross-planet transfer costs only power — no ships involved. 跨星球传输只耗电，无需飞船。
- Local drone logistics work exactly like vanilla and combine freely. 本地小飞机物流与原版一致，可自由组合。
- **Alt+5** dimension inventory: search items and set per-item caps 0 ~ 10,000,000; resizable window. Alt+5 打开维度界面：搜索物品、设置每种物品上限（0–1000 万），窗口可拖拽调整大小。
- Save integration via DSPModSave. 通过 DSPModSave 随存档保存。

## Install 安装

Install from [Thunderstore](https://thunderstore.io/c/dyson-sphere-program/) with the Thunderstore Mod Manager, or manually: install BepInEx + CommonAPI + CommonAPI-DSPModSave, then copy `BepInEx/plugins/VirtualDimension` into the game's `BepInEx` folder.

推荐使用 Thunderstore Mod Manager 一键安装；手动安装请先装 BepInEx、CommonAPI、CommonAPI-DSPModSave，再把 `BepInEx/plugins/VirtualDimension` 复制到游戏 `BepInEx` 目录。

## Build 构建

Requirements: Windows, .NET Framework / Roslyn `csc.exe`, the game (Dyson Sphere Program) installed, and the mod dependencies installed in a Thunderstore profile.

```powershell
powershell -ExecutionPolicy Bypass -File build.ps1
```

The script compiles `src/VirtualDimension/*.cs` against the game's managed assemblies, regenerates `dist/icon.png`, packages `dist/VirtualDimension.zip`, and deploys to the local BepInEx profile. Adjust the game path inside `build.ps1` if your install location differs.

## License

MIT.
