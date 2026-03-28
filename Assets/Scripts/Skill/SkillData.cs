using UnityEngine;
using CangQiong.Systems;

namespace CangQiong.Skill
{
    /// <summary>
    /// 技能类型
    /// </summary>
    public enum SkillType
    {
        Passive,    // 被动技能
        Active,     // 主动技能
        Buff,       // Buff 技能
        Debuff,     // Debuff 技能
    }

    /// <summary>
    /// 技能目标类型
    /// </summary>
    public enum SkillTarget
    {
        Self,           // 自身
        SingleEnemy,    // 单体敌方
        SingleAlly,     // 单体友方
        AllEnemies,     // 全体敌方
        AllAllies,      // 全体友方
        Area,           // 范围（以目标为中心）
    }

    /// <summary>
    /// 技能数据
    /// </summary>
    [CreateAssetMenu(fileName = "SkillData", menuName = "CangQiong/SkillData")]
    public class SkillData : ScriptableObject
    {
        public string skillName;
        [TextArea]
        public string description;

        public SkillType type = SkillType.Active;
        public SkillTarget target;

        [Header("Cost")]
        public int mpCost;
        public int hpCost;     // 有些技能消耗 HP

        [Header("Combat Stats")]
        public int power;              // 技能威力（影响伤害）
        public bool isMagicDamage;     // 是否为魔法伤害
        public int range;              // 技能范围（格子数）
        public int hits = 1;           // 攻击段数（追击）

        [Header("Element")]
        public FiveElement element;    // 技能五行属性

        [Header("Effects")]
        public int healAmount;        // 治疗量
        public float statModifier = 1f;  // 属性倍率
        public int buffDuration;       // buff 持续回合
        public int moveRangeBonus;    // 移动范围加成

        [Header("Visual")]
        public string animationTrigger;  // Unity Animator Trigger
        public GameObject vfxPrefab;     // 技能特效预制件
        public AudioClip sfxClip;        // 技能音效

        /// <summary>
        /// 检查技能是否可用
        /// </summary>
        public bool CanUse(Character.Unit caster)
        {
            if (caster.CurrentMP < mpCost)
                return false;
            if (hpCost > 0 && caster.CurrentHP <= hpCost)
                return false;
            if (type == SkillType.Passive)
                return false;  // 被动技能自动生效
            return true;
        }

        /// <summary>
        /// 获取技能描述（带数值）
        /// </summary>
        public string GetDescription()
        {
            string desc = description;

            if (power > 0)
                desc += $"\n威力：{power}";
            if (mpCost > 0)
                desc += $"\n消耗：{mpCost} MP";
            if (healAmount > 0)
                desc += $"\n治疗：{healAmount} HP";
            if (range > 1)
                desc += $"\n范围：{range} 格";
            if (buffDuration > 0)
                desc += $"\n持续：{buffDuration} 回合";

            return desc;
        }
    }

    /// <summary>
    /// 技能管理器
    /// </summary>
    public class SkillManager : MonoBehaviour
    {
        public static SkillManager Instance { get; private set; }

        [Header("Skill Database")]
        public SkillData[] allSkills;  // 所有技能配置

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// 使用技能
        /// </summary>
        /// <returns>技能是否成功释放</returns>
        public bool UseSkill(SkillData skill, Character.Unit caster, Character.Unit target)
        {
            if (!skill.CanUse(caster))
            {
                Debug.Log($"[Skill] 技能 {skill.skillName} 无法使用（MP 不足）");
                return false;
            }

            // 消耗资源
            if (!caster.ConsumeMP(skill.mpCost))
                return false;

            if (skill.hpCost > 0)
                caster.CurrentHP = Mathf.Max(1, caster.CurrentHP - skill.hpCost);

            // 播放特效
            if (skill.vfxPrefab != null)
                SpawnVFX(skill, target);

            if (skill.sfxClip != null)
                AudioSource.PlayClipAtPoint(skill.sfxClip, target.transform.position);

            // 执行技能效果
            ExecuteSkillEffect(skill, caster, target);

            Debug.Log($"[Skill] {caster.unitName} 使用了 {skill.skillName}");
            return true;
        }

        /// <summary>
        /// 执行技能效果
        /// </summary>
        private void ExecuteSkillEffect(SkillData skill, Character.Unit caster, Character.Unit target)
        {
            switch (skill.type)
            {
                case SkillType.Active:
                    ApplyDamage(skill, caster, target);
                    break;

                case SkillType.Buff:
                    ApplyBuff(skill, caster);
                    break;

                case SkillType.Debuff:
                    ApplyDebuff(skill, caster, target);
                    break;
            }
        }

        /// <summary>
        /// 造成伤害
        /// </summary>
        private void ApplyDamage(SkillData skill, Character.Unit caster, Character.Unit target)
        {
            // 基础伤害 = 技能威力 × (ATK 或 MAT)
            int baseDamage = skill.isMagicDamage
                ? Mathf.RoundToInt(skill.power * (caster.MAT / 20f))
                : Mathf.RoundToInt(skill.power * (caster.ATK / 20f));

            // 五行修正
            int finalDamage = FiveElementsSystem.CalculateDamage(
                baseDamage,
                skill.element != FiveElement.None ? skill.element : caster.Element,
                target.Element
            );

            // 地形防御加成
            if (TileMap.Instance != null)
            {
                var tile = TileMap.Instance.GetTile(target.GridPosition);
                if (tile != null)
                    finalDamage = Mathf.RoundToInt(finalDamage * (1f - tile.defenseBonus));
            }

            // 多段攻击
            int totalDamage = 0;
            for (int i = 0; i < skill.hits; i++)
            {
                totalDamage += target.TakeDamage(finalDamage, caster.Element, skill.isMagicDamage);
            }

            Debug.Log($"[Skill] 造成 {totalDamage} 点伤害（{skill.hits} 段）");
        }

        /// <summary>
        /// 应用 Buff
        /// </summary>
        private void ApplyBuff(SkillData skill, Character.Unit caster)
        {
            // Buff 效果通过 BuffSystem 管理
            // 这里预留接口
            Debug.Log($"[Skill] {caster.unitName} 获得 Buff：{skill.skillName}");
        }

        /// <summary>
        /// 应用 Debuff
        /// </summary>
        private void ApplyDebuff(SkillData skill, Character.Unit caster, Character.Unit target)
        {
            // Debuff 效果通过 BuffSystem 管理
            Debug.Log($"[Skill] {target.unitName} 受到 Debuff：{skill.skillName}");
        }

        /// <summary>
        /// 生成技能特效
        /// </summary>
        private void SpawnVFX(SkillData skill, Character.Unit target)
        {
            if (skill.vfxPrefab == null) return;

            var vfx = Instantiate(skill.vfxPrefab, target.transform.position, Quaternion.identity);
            Destroy(vfx, 3f);  // 3 秒后销毁
        }

        /// <summary>
        /// 根据职业获取默认技能
        /// </summary>
        public SkillData[] GetDefaultSkillsForClass(Character.JobType job)
        {
            // 从 allSkills 中筛选该职业的默认技能
            // 正式实现会从配置数据中读取
            return System.Array.FindAll(allSkills, s => s != null);
        }
    }
}
