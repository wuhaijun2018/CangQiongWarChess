# 《苍穹战棋》

**策略战棋 RPG + 恋爱养成 | Steam 平台**

---

## 游戏简介

《苍穹战棋》是一款以五行相克为核心的战棋策略 RPG，玩家扮演第三代五行之体「云霄」，在四章主线中经历江湖羁绊、爱恨情仇，最终揭开苍穹深渊的秘密。

## 平台

- **Steam**（PC / macOS / Linux）
- 完全免费

## 开发环境

| 工具 | 版本 |
|------|------|
| Unity Editor | **2022.3 LTS** |
| C# | .NET Standard 2.1 |
| Unity Hub | 3.x+ |

## 项目结构

```
Assets/
├── Scripts/
│   ├── Battle/       # 战斗系统（回合制/AI/技能）
│   ├── Character/    # 角色系统（职业/转职/装备）
│   ├── Story/        # 剧情系统（对话/选项/分支）
│   ├── Skill/        # 技能系统（五行/buff/追击）
│   ├── UI/           # UI 逻辑（菜单/HUD/立绘）
│   └── Steam/        # Steam 集成（成就/云存档）
├── Art/              # 美术资源（LFS 管理）
├── Audio/            # 音频资源（BGM/SFX/配音）
└── Scenes/           # Unity 场景
```

## 快速开始

### 1. 克隆项目

```bash
git clone https://github.com/YOUR_USERNAME/CangQiongWarChess.git
cd CangQiongWarChess
git lfs install
git lfs pull
```

### 2. 打开 Unity

```bash
# 通过 Unity Hub 打开项目目录
open -a "Unity Hub" .
# 或直接
unity-editor -projectPath .
```

### 3. CI/CD

推送到 `main` 分支并打 tag 触发 Steam 构建：

```bash
git commit -m "feat: your change"
git tag v0.1.0
git push origin main --tags
```

GitHub Actions 会自动：
1. 构建 Windows / Linux / macOS 三个平台
2. 上传构建产物到 GitHub Release
3. （需要 secrets 配置）steamcmd 上传到 Steam 实验分支

## 开发阶段

详见 [docs/开发计划与验证清单.md](docs/开发计划与验证清单.md)

## 技术栈

- Unity 2022.3 LTS（GameCI）
- GitHub Actions（CI/CD）
- Steamworks API（成就/云存档）
- Aseprite（像素图）
- FL Studio（音乐）

## License

 proprietary - All rights reserved
