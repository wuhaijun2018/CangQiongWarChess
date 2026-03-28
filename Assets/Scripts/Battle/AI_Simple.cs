using UnityEngine;
using CangQiong.Character;

namespace CangQiong.Battle
{
    /// <summary>
    /// 战斗 AI 类型
    /// </summary>
    public enum AIType
    {
        Easy,     // 简单：随机攻击
        Normal,   // 普通：集火脆皮
        Hard,     // 困难：控制链配合
        Hell,     // 地狱：预判+语音配合
    }

    /// <summary>
    /// 战斗 AI 控制器
    /// 
    /// 根据 AI 类型决定行动策略：
    /// - Easy：随机选择一个可攻击目标
    /// - Normal：优先攻击血量最低的目标
    /// - Hard：优先攻击治疗/脆皮单位，并使用控制技能
    /// - Hell：预判玩家意图，配合多名敌人集火
    /// </summary>
    public class AI_Simple : MonoBehaviour
    {
        public AIType aiType = AIType.Normal;

        [Header("AI Config")]
        [Range(1, 5)]
        public int aiMoveDelayMs = 500;  // AI 行动前的"思考"延迟

        private System.Threading.Tasks.TaskCompletionSource<bool> aiTask;

        /// <summary>
        /// 执行 AI 回合
        /// 返回是否成功行动
        /// </summary>
        public async System.Threading.Tasks.Task ExecuteTurn(Unit[] enemies, TileMap tileMap)
        {
            Debug.Log($"[AI] AI 类型: {aiType}，开始行动");

            // 模拟 AI"思考"延迟
            await System.Threading.Tasks.Task.Delay(aiMoveDelayMs);

            // 找出所有存活的敌人单位
            var aliveEnemies = System.Linq.Enumerable.ToList(
                System.Linq.Enumerable.Where(enemies, e => e != null && !e.IsDead));

            if (aliveEnemies.Count == 0)
            {
                Debug.Log("[AI] 没有存活的敌人单位");
                return;
            }

            // 对每个敌人单位执行一次行动
            foreach (var enemy in aliveEnemies)
            {
                if (enemy.IsDead) continue;

                // 查找可攻击目标
                var target = SelectTarget(enemy, aliveEnemies, enemies, tileMap);
                if (target != null)
                {
                    await ExecuteAction(enemy, target);
                }
            }

            Debug.Log("[AI] AI 回合结束");
        }

        /// <summary>
        /// 根据 AI 类型选择目标
        /// </summary>
        private Unit SelectTarget(Unit aiUnit, System.Collections.Generic.List<Unit> allies, Unit[] allUnits, TileMap tileMap)
        {
            // 找出所有存活的我方单位
            var alivePlayers = System.Linq.Enumerable.ToList(
                System.Linq.Enumerable.Where(allUnits, u => u != null && !u.IsDead));

            if (alivePlayers.Count == 0) return null;

            return aiType switch
            {
                AIType.Easy => SelectTargetEasy(alivePlayers),
                AIType.Normal => SelectTargetNormal(alivePlayers),
                AIType.Hard => SelectTargetHard(alivePlayers),
                AIType.Hell => SelectTargetHell(aiUnit, alivePlayers, allies, tileMap),
                _ => alivePlayers[Random.Range(0, alivePlayers.Count)]
            };
        }

        /// <summary>
        /// Easy AI：随机目标
        /// </summary>
        private Unit SelectTargetEasy(System.Collections.Generic.List<Unit> targets)
        {
            return targets[Random.Range(0, targets.Count)];
        }

        /// <summary>
        /// Normal AI：集火脆皮（HP 最低）
        /// </summary>
        private Unit SelectTargetNormal(System.Collections.Generic.List<Unit> targets)
        {
            Unit best = null;
            int lowestHp = int.MaxValue;

            foreach (var t in targets)
            {
                if (t.CurrentHP < lowestHp)
                {
                    lowestHp = t.CurrentHP;
                    best = t;
                }
            }

            return best;
        }

        /// <summary>
        /// Hard AI：优先打治疗 > 脆皮 > 距离近的
        /// </summary>
        private Unit SelectTargetHard(System.Collections.Generic.List<Unit> targets)
        {
            // 优先打治疗职业（Healer）
            foreach (var t in targets)
            {
                if (t is Healer)  // 如果有职业系统，可以检查职业
                    return t;
            }

            // 然后打 HP 百分比最低的
            Unit best = null;
            float lowestRatio = float.MaxValue;

            foreach (var t in targets)
            {
                float ratio = (float)t.CurrentHP / t.MaxHP;
                if (ratio < lowestRatio)
                {
                    lowestRatio = ratio;
                    best = t;
                }
            }

            return best;
        }

        /// <summary>
        /// Hell AI：综合判断 — HP、距离、职业、威胁度
        /// </summary>
        private Unit SelectTargetHell(Unit aiUnit, System.Collections.Generic.List<Unit> targets, System.Collections.Generic.List<Unit> allies, TileMap tileMap)
        {
            Unit best = null;
            float highestScore = float.MinValue;

            foreach (var t in targets)
            {
                float score = EvaluateTarget(aiUnit, t, allies, tileMap);
                if (score > highestScore)
                {
                    highestScore = score;
                    best = t;
                }
            }

            return best;
        }

        /// <summary>
        /// 计算目标威胁度评分
        /// </summary>
        private float EvaluateTarget(Unit aiUnit, Unit target, System.Collections.Generic.List<Unit> allies, TileMap tileMap)
        {
            float score = 0f;

            // HP 越低分越高（优先击杀）
            score += (1f - (float)target.CurrentHP / target.MaxHP) * 30f;

            // 距离越近分越高
            if (tileMap != null)
            {
                int dist = tileMap.GetDistance(aiUnit.GridPosition, target.GridPosition);
                score += Mathf.Max(0, 20f - dist * 3f);
            }

            // 高威胁职业优先打
            if (target is Healer) score += 25f;
            if (target is Mage) score += 15f;
            if (target is Archer) score += 10f;

            // 如果目标血量很低，增加击杀优先级
            if ((float)target.CurrentHP / target.MaxHP < 0.3f)
                score += 20f;

            return score;
        }

        /// <summary>
        /// 执行 AI 行动
        /// </summary>
        private async System.Threading.Tasks.Task ExecuteAction(Unit aiUnit, Unit target)
        {
            Debug.Log($"[AI] {aiUnit.unitName} 选择攻击 {target.unitName}");

            // 计算伤害
            int damage = CalculateDamage(aiUnit, target);
            target.TakeDamage(damage, aiUnit.Element, isMagic: false);

            // 模拟攻击动画延迟
            await System.Threading.Tasks.Task.Delay(300);
        }

        /// <summary>
        /// 计算 AI 攻击伤害
        /// </summary>
        private int CalculateDamage(Unit attacker, Unit defender)
        {
            int baseDamage = attacker.ATK;
            float elementMod = Systems.FiveElementsSystem.GetElementModifier(attacker.Element, defender.Element);
            int def = defender.DEF;
            return Mathf.Max(1, Mathf.RoundToInt((baseDamage - def * 0.5f) * elementMod));
        }
    }

    // 辅助接口占位（正式实现时替换为实际职业类）
    public interface Healer { }
    public interface Mage { }
    public interface Archer { }
}
