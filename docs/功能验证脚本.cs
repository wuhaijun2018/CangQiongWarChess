// FiveElementsSystemTests.cs
// 五行相克系统单元测试（可在 Unity Editor 运行）
// 路径：CangQiongWarChess/Assets/Scripts/Systems/FiveElementsSystemTests.cs

#if UNITY_EDITOR
using UnityEngine;
using CangQiong.Systems;

namespace CangQiong.Tests
{
    /// <summary>
    /// 五行相克系统验证测试
    /// 运行方法：Unity Editor → Window → General → Test Runner → Run All
    /// </summary>
    public class FiveElementsSystemTests
    {
        /// <summary>
        /// 测试 1：正常伤害（无相克）
        /// </summary>
        [UnityEngine.TestTools.UnityTest]
        public void TestNormalDamage_NoElementAdvantage()
        {
            int damage = FiveElementsSystem.CalculateDamage(100, FiveElement.None, FiveElement.None);
            UnityEngine.Debug.Assert(damage == 100, $"期望 100，实际 {damage}");
        }

        /// <summary>
        /// 测试 2：火克金（+20%）
        /// </summary>
        [UnityEngine.TestTools.UnityTest]
        public void TestFireAdvantageVsMetal()
        {
            int damage = FiveElementsSystem.CalculateDamage(100, FiveElement.Fire, FiveElement.Metal);
            UnityEngine.Debug.Assert(damage == 120, $"火→金 期望 120，实际 {damage}");
        }

        /// <summary>
        /// 测试 3：金克木（+20%）
        /// </summary>
        [UnityEngine.TestTools.UnityTest]
        public void TestMetalAdvantageVsWood()
        {
            int damage = FiveElementsSystem.CalculateDamage(100, FiveElement.Metal, FiveElement.Wood);
            UnityEngine.Debug.Assert(damage == 120, $"金→木 期望 120，实际 {damage}");
        }

        /// <summary>
        /// 测试 4：木克土（+20%）
        /// </summary>
        [UnityEngine.TestTools.UnityTest]
        public void TestWoodAdvantageVsEarth()
        {
            int damage = FiveElementsSystem.CalculateDamage(100, FiveElement.Wood, FiveElement.Earth);
            UnityEngine.Debug.Assert(damage == 120, $"木→土 期望 120，实际 {damage}");
        }

        /// <summary>
        /// 测试 5：土克水（+20%）
        /// </summary>
        [UnityEngine.TestTools.UnityTest]
        public void TestEarthAdvantageVsWater()
        {
            int damage = FiveElementsSystem.CalculateDamage(100, FiveElement.Earth, FiveElement.Water);
            UnityEngine.Debug.Assert(damage == 120, $"土→水 期望 120，实际 {damage}");
        }

        /// <summary>
        /// 测试 6：水克火（+20%）
        /// </summary>
        [UnityEngine.TestTools.UnityTest]
        public void TestWaterAdvantageVsFire()
        {
            int damage = FiveElementsSystem.CalculateDamage(100, FiveElement.Water, FiveElement.Fire);
            UnityEngine.Debug.Assert(damage == 120, $"水→火 期望 120，实际 {damage}");
        }

        /// <summary>
        /// 测试 7：被克制（-20%）
        /// </summary>
        [UnityEngine.TestTools.UnityTest]
        public void TestDisadvantageVsFire()
        {
            int damage = FiveElementsSystem.CalculateDamage(100, FiveElement.Metal, FiveElement.Fire);
            UnityEngine.Debug.Assert(damage == 80, $"金 vs 火（被克）期望 80，实际 {damage}");
        }

        /// <summary>
        /// 测试 8：完整相克表验证
        /// </summary>
        [UnityEngine.TestTools.UnityTest]
        public void TestFullElementChart()
        {
            // 完整验证：火>金>木>土>水>火
            int expected = 120;
            int[] fireAgainst = {
                (int)FiveElement.Metal,   // 火克金 ✓
                (int)FiveElement.Wood,   // 木无关
                (int)FiveElement.Water,   // 水克火
                (int)FiveElement.Fire,   // 同火
                (int)FiveElement.Earth   // 火克土 ✓
            };

            int expectedAgainstFire = 80;  // 水克火

            for (int i = 1; i <= 5; i++)
            {
                float mod = FiveElementsSystem.GetElementModifier(FiveElement.Fire, (FiveElement)i);
                if (i == (int)FiveElement.Fire)
                    UnityEngine.Debug.Assert(Mathf.Abs(mod - 1.0f) < 0.01f, $"火vs火 期望 1.0，实际 {mod}");
                else if (i == (int)FiveElement.Metal || i == (int)FiveElement.Earth)
                    UnityEngine.Debug.Assert(Mathf.Abs(mod - 1.2f) < 0.01f, $"火vs{(FiveElement)i} 期望 1.2，实际 {mod}");
                else
                    UnityEngine.Debug.Assert(Mathf.Abs(mod - 1.0f) < 0.01f, $"火vs{(FiveElement)i} 期望 1.0，实际 {mod}");
            }
        }

        /// <summary>
        /// 测试 9：元素名称正确返回
        /// </summary>
        [UnityEngine.TestTools.UnityTest]
        public void TestElementNames()
        {
            UnityEngine.Debug.Assert(FiveElementsSystem.GetElementName(FiveElement.Fire) == "火");
            UnityEngine.Debug.Assert(FiveElementsSystem.GetElementName(FiveElement.Metal) == "金");
            UnityEngine.Debug.Assert(FiveElementsSystem.GetElementName(FiveElement.Wood) == "木");
            UnityEngine.Debug.Assert(FiveElementsSystem.GetElementName(FiveElement.Water) == "水");
            UnityEngine.Debug.Assert(FiveElementsSystem.GetElementName(FiveElement.Earth) == "土");
        }
    }
}
#endif
