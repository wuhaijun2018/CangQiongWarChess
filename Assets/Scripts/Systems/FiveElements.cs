using UnityEngine;

namespace CangQiong.Systems
{
    /// <summary>
    /// 五行元素类型
    /// </summary>
    public enum FiveElement
    {
        None = 0,
        Metal = 1,   // 金
        Wood = 2,    // 木
        Water = 3,   // 水
        Fire = 4,    // 火
        Earth = 5,   // 土
    }

    /// <summary>
    /// 五行相克系统
    /// 
    /// 相克规则（火>金>木>土>水>火）
    /// 攻击方属性克制被攻击方时，伤害 +20%
    /// 被克制时，伤害 -20%
    /// 同属性或无克制关系时，伤害正常（1.0x）
    /// 
    /// 五行图：
    ///   火 → 金 → 木 → 土 → 水 → 火
    /// </summary>
    public static class FiveElementsSystem
    {
        // 相克矩阵：key = (攻击方, 防守方)，value = 伤害倍率
        // 火>金>木>土>水>火
        private static readonly float[,] ElementChart = new float[6, 6]
        {
            // None  Metal  Wood   Water  Fire   Earth
            { 1.0f,  1.0f,  1.0f,  1.0f,  1.0f,  1.0f },  // None
            { 1.0f,  1.0f,  1.0f,  1.0f,  0.8f,  1.2f },  // Metal (被火克, 克木)
            { 1.0f,  1.2f,  1.0f,  1.0f,  1.0f,  0.8f },  // Wood (被金属, 克土)
            { 1.0f,  1.0f,  1.2f,  1.0f,  1.0f,  0.8f },  // Water (被木克, 克火)
            { 1.0f,  0.8f,  1.0f,  1.0f,  1.0f,  1.2f },  // Fire (被水克, 克金)
            { 1.0f,  1.0f,  0.8f,  1.2f,  1.0f,  1.0f },  // Earth (被火克, 克水)
        };

        /// <summary>
        /// 获取元素相克修正倍率
        /// </summary>
        /// <param name="attackerElement">攻击方元素</param>
        /// <param name="defenderElement">防守方元素</param>
        /// <returns>伤害倍率（0.8 / 1.0 / 1.2）</returns>
        public static float GetElementModifier(FiveElement attackerElement, FiveElement defenderElement)
        {
            if (attackerElement == FiveElement.None || defenderElement == FiveElement.None)
                return 1.0f;

            int a = (int)attackerElement;
            int d = (int)defenderElement;
            return ElementChart[a, d];
        }

        /// <summary>
        /// 获取元素图标（用于 UI 显示）
        /// </summary>
        public static string GetElementIcon(FiveElement element)
        {
            return element switch
            {
                FiveElement.Metal => "⚔️",
                FiveElement.Wood => "🌿",
                FiveElement.Water => "💧",
                FiveElement.Fire => "🔥",
                FiveElement.Earth => "🪨",
                _ => "⚪"
            };
        }

        /// <summary>
        /// 获取元素中文名
        /// </summary>
        public static string GetElementName(FiveElement element)
        {
            return element switch
            {
                FiveElement.Metal => "金",
                FiveElement.Wood => "木",
                FiveElement.Water => "水",
                FiveElement.Fire => "火",
                FiveElement.Earth => "土",
                _ => "无"
            };
        }

        /// <summary>
        /// 计算最终伤害（基础伤害 × 五行修正）
        /// </summary>
        public static int CalculateDamage(int baseDamage, FiveElement attackerElement, FiveElement defenderElement)
        {
            float modifier = GetElementModifier(attackerElement, defenderElement);
            return Mathf.RoundToInt(baseDamage * modifier);
        }

        /// <summary>
        /// 调试：打印完整相克表
        /// </summary>
        public static void PrintElementChart()
        {
            Debug.Log("=== 五行相克表 ===");
            Debug.Log($"{"",8} {"金",8} {"木",8} {"水",8} {"火",8} {"土",8}");
            for (int i = 1; i <= 5; i++)
            {
                var row = $"{GetElementName((FiveElement)i),8}";
                for (int j = 1; j <= 5; j++)
                {
                    row += $"{ElementChart[i, j],8:F1}";
                }
                Debug.Log(row);
            }
            Debug.Log("火>金>木>土>水>火  (攻击方 → 被攻击方)");
        }
    }
}
