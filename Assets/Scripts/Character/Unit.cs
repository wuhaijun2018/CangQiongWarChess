using UnityEngine;
using CangQiong.Systems;

namespace CangQiong.Character
{
    /// <summary>
    /// 角色单位基类
    /// </summary>
    public class Unit : MonoBehaviour
    {
        [Header("Base Info")]
        public string unitName = "单位";
        public int Level = 1;

        [Header("Element")]
        public FiveElement Element = FiveElement.None;
        public FiveElement SecondaryElement = FiveElement.None;

        [Header("Base Stats")]
        public int MaxHP = 100;
        public int MaxMP = 50;
        public int ATK = 20;   // 物理攻击
        public int MAT = 20;   // 法术攻击
        public int DEF = 10;   // 物理防御
        public int MDF = 10;   // 法术防御
        public int SPD = 10;   // 速度（影响行动顺序）

        [Header("Current State")]
        public int CurrentHP { get; private set; }
        public int CurrentMP { get; private set; }

        [Header("Position")]
        public Vector2Int GridPosition;  // 地图格子坐标

        protected virtual void Awake()
        {
            CurrentHP = MaxHP;
            CurrentMP = MaxMP;
        }

        /// <summary>
        /// 造成伤害（内部使用五行修正）
        /// </summary>
        public virtual int TakeDamage(int damage, FiveElement attackerElement, bool isMagic = false)
        {
            int def = isMagic ? MDF : DEF;
            
            // 五行相克修正
            float elementMod = FiveElementsSystem.GetElementModifier(attackerElement, Element);
            
            // 最终伤害 = 攻击 - 防御（最低为1）
            int finalDamage = Mathf.Max(1, Mathf.RoundToInt((damage - def) * elementMod));
            
            CurrentHP = Mathf.Max(0, CurrentHP - finalDamage);
            Debug.Log($"[{unitName}] 受到 {finalDamage} 点伤害 ({(isMagic ? "魔法" : "物理")} / 五行修正 {elementMod:F2})");

            if (CurrentHP <= 0)
            {
                OnDeath();
            }

            return finalDamage;
        }

        /// <summary>
        /// 恢复 HP
        /// </summary>
        public void Heal(int amount)
        {
            CurrentHP = Mathf.Min(MaxHP, CurrentHP + amount);
            Debug.Log($"[{unitName}] 恢复 {amount} HP ({CurrentHP}/{MaxHP})");
        }

        /// <summary>
        /// 消耗 MP
        /// </summary>
        public bool ConsumeMP(int cost)
        {
            if (CurrentMP >= cost)
            {
                CurrentMP -= cost;
                return true;
            }
            Debug.Log($"[{unitName}] MP 不足 ({CurrentMP}/{cost})");
            return false;
        }

        /// <summary>
        /// 移动到指定格子
        /// </summary>
        public void MoveTo(Vector2Int newPos)
        {
            GridPosition = newPos;
            Debug.Log($"[{unitName}] 移动到 ({newPos.x}, {newPos.y})");
        }

        /// <summary>
        /// 死亡时调用
        /// </summary>
        protected virtual void OnDeath()
        {
            Debug.Log($"[{unitName}] 阵亡！");
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 是否已阵亡
        /// </summary>
        public bool IsDead => CurrentHP <= 0;

        /// <summary>
        /// 获取角色状态摘要（调试用）
        /// </summary>
        public string GetStatus()
        {
            string elemStr = FiveElementsSystem.GetElementIcon(Element);
            if (SecondaryElement != FiveElement.None)
                elemStr += FiveElementsSystem.GetElementIcon(SecondaryElement);
            return $"[{unitName}] HP:{CurrentHP}/{MaxHP} MP:{CurrentMP}/{MaxMP} {elemStr}";
        }
    }
}
