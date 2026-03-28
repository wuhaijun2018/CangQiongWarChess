# Phase 1 验证报告 — 核心框架

**项目**：《苍穹战棋》
**验证阶段**：Phase 1（核心框架）
**验证日期**：2026-03-28
**目标**：完成可运行的战棋战斗原型

---

## 一、项目结构验证 ✅ / ⬜

```
CangQiongWarChess/
├── .github/workflows/unity-ci.yml     ✅ GitHub Actions CI
├── Assets/Scripts/Battle/             ✅
│   └── BattleManager.cs                ✅
├── Assets/Scripts/Character/          ✅
│   ├── Unit.cs                         ✅
│   └── ClassSystem.cs                  ✅
├── Assets/Scripts/Systems/            ✅
│   └── FiveElements.cs                ✅
├── Assets/Scripts/Story/              ⬜ 待实现
├── Assets/Scripts/UI/                 ⬜ 待实现
├── Assets/Scripts/Steam/              ⬜ 待实现
├── Assets/Scenes/Battle/              ✅
│   └── BattleTest.scene               ✅
├── docs/                              ✅
│   ├── 开发计划与验证清单.md             ✅
│   └── 功能验证脚本.cs                  ✅
├── steam/                            ✅
│   └── app_build_cangqiong.vdf        ✅
└── README.md                          ✅
```

---

## 二、代码功能验证

### 2.1 五行相克系统（FiveElements.cs）✅

| 测试编号 | 测试内容 | 预期结果 | 状态 |
|---------|---------|---------|------|
| T-01 | `CalculateDamage(100, Fire, Metal)` | 120 (±1) | ⬜ |
| T-02 | `CalculateDamage(100, Metal, Fire)` | 80 (±1) | ⬜ |
| T-03 | `CalculateDamage(100, Fire, Fire)` | 100 (±1) | ⬜ |
| T-04 | `CalculateDamage(100, None, None)` | 100 (±1) | ⬜ |
| T-05 | `GetElementChart()` 完整打印 | 5×5 矩阵 | ⬜ |
| T-06 | 伤害修正 ±20% 边界 | 修正值正确 | ⬜ |

**验证方法**：Unity Editor → Test Runner → Run All

```
五行相克表（火>金>木>土>水>火）
        金     木     水     火     土
金      1.0   1.0   1.0   0.8   1.0
木      1.2   1.0   1.0   1.0   0.8
水      1.0   1.2   1.0   1.0   1.0
火      1.0   1.0   1.2   1.0   1.2
土      1.0   0.8   1.2   1.0   1.0
```

### 2.2 战斗管理器（BattleManager.cs）✅

| 测试编号 | 测试内容 | 预期结果 | 状态 |
|---------|---------|---------|------|
| T-10 | `StartBattle()` → `PlayerTurn` | 状态正确 | ⬜ |
| T-11 | `EndPlayerTurn()` → `EnemyTurn` | 状态切换 | ⬜ |
| T-12 | `EndEnemyTurn()` → `PlayerTurn` (回合+1) | 回合正确 | ⬜ |
| T-13 | `EndBattle(Victory)` → `BattleEnd` | 结果正确 | ⬜ |
| T-14 | 30 回合超时 → `Draw` | 超时处理 | ⬜ |
| T-15 | `OnStateChanged` 事件触发 | 事件正常 | ⬜ |

**状态机流程**：
```
StartBattle()
  → PlayerTurn
    → PlayerMoving
    → PlayerActing
    → (EndPlayerTurn)
  → EnemyTurn
    → EnemyActing
    → (EndEnemyTurn)
  → 循环 或 Victory/Defeat
  → BattleEnd
```

### 2.3 角色系统（Unit.cs）✅

| 测试编号 | 测试内容 | 预期结果 | 状态 |
|---------|---------|---------|------|
| T-20 | `TakeDamage(50, Fire, Metal)` | 50×1.2=60 伤害 | ⬜ |
| T-21 | `TakeDamage(50, Metal, Fire)` | 50×0.8=40 伤害 | ⬜ |
| T-22 | HP ≤ 0 → `OnDeath()` | 角色隐藏 | ⬜ |
| T-23 | `Heal(30)` → HP 恢复 | HP 正确 | ⬜ |
| T-24 | `ConsumeMP(20)` 足够 | MP 扣除 | ⬜ |
| T-25 | `ConsumeMP(200)` 不足 | 返回 false | ⬜ |

### 2.4 职业系统（ClassSystem.cs）✅

| 测试编号 | 测试内容 | 预期结果 | 状态 |
|---------|---------|---------|------|
| T-30 | 六大职业数据完整 | 6 种职业 | ⬜ |
| T-31 | 五行属性正确绑定 | 火+土等 | ⬜ |
| T-32 | 转职路径正确 | 剑士→剑豪/圣剑士→剑圣 | ⬜ |

---

## 三、CI/CD 验证

### 3.1 GitHub Actions CI ⬜

| 测试编号 | 测试内容 | 预期结果 | 状态 |
|---------|---------|---------|------|
| CI-01 | Push to `main` 触发 workflow | workflow 运行 | ⬜ |
| CI-02 | Windows 构建成功 | `.exe` 产出 | ⬜ |
| CI-03 | Linux 构建成功 | `.x86_64` 产出 | ⬜ |
| CI-04 | macOS 构建成功 | `.app` 产出 | ⬜ |
| CI-05 | Tag `v*.*.*` 触发 Release | GitHub Release | ⬜ |
| CI-06 | Artifact retention 7天 | 自动清理 | ⬜ |

**前置条件**：
- [ ] GitHub 仓库已创建
- [ ] GitHub Secrets 已配置（UNITY_EMAIL / UNITY_PASSWORD / UNITY_LICENSE 等）

---

## 四、待开发模块（Phase 1 未完成）

| 模块 | 文件 | 优先级 | 状态 |
|------|------|--------|------|
| TileMap 格子系统 | `TileMap.cs` | P0 | ⬜ |
| 移动范围算法 | `MoveRange.cs` | P0 | ⬜ |
| 攻击范围算法 | `AttackRange.cs` | P0 | ⬜ |
| 简单 AI | `AI_Simple.cs` | P1 | ⬜ |
| 战斗 UI | `BattleHUD.cs` | P1 | ⬜ |
| 角色数据配置 | `CharacterData.asset` | P2 | ⬜ |

---

## 五、验证总结

| 类别 | 已完成 | 待验证 | 合计 |
|------|--------|--------|------|
| 核心代码 | 4 个文件 | 0 | 4 |
| 单元测试 | 0 个 | 15 个 | 15 |
| CI Pipeline | 0 次运行 | 6 项 | 6 |

**Phase 1 启动验证完成时间目标**：2026-04-04（第一周结束前）
