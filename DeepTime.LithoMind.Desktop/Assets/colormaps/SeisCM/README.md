# SeisCM 色标剥离文件

来源：<https://github.com/lijunzh/SeisCM>

许可证：MIT。使用时请保留 `LICENSE.SeisCM.MIT.txt` 中的版权与许可声明。

## 文件说明

- `seiscm_colormaps.json`：已剥离出的通用 JSON 色标文件，包含 `bwr`、`seismic`、`phase`、`frequency` 四个色标。
  - `segments`：原始 Matplotlib `LinearSegmentedColormap` 分段定义。
  - `samples256`：按 0~1 均匀采样生成的 256 个 `#RRGGBBAA` 颜色，适合 C#/Avalonia/Skia 直接查表使用。
- `seiscm_original.py`：原始 Python 色标定义文件，便于核对来源。
- `LICENSE.SeisCM.MIT.txt`：原仓库 MIT 许可证。

## 推荐用途

- `seismic`：常规 SEG-Y 振幅剖面显示。
- `bwr`：正负振幅叠加，中点默认透明。
- `phase`：瞬时相位/相位属性。
- `frequency`：瞬时频率/频率属性。
