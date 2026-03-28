using UnityEngine;
using CangQiong.Systems;

namespace CangQiong.Character
{
    /// <summary>
    /// 职业类型
    /// </summary>
    public enum JobType
    {
        Swordsman,    // 剑士 — 平衡输出，火+土
        Blademaster,  // 刀客 — 爆发输出，火+木
        Spearman,     // 枪兵 — 防御肉盾，金+土
        Archer,       // 弓手 — 远程输出，金+木
        Mage,         // 法师 — 法术AOE，水+火
        Healer,       // 医者 — 治疗辅助，水+木
    }

    /// <summary>
    /// 职业系统数据
    /// 定义每个职业的基础属性和特点
    /// </summary>
    [CreateAssetMenu(fileName = "ClassData", menuName = "CangQiong/ClassData")]
    public class ClassData : ScriptableObject
    {
        public JobType jobType;
        public string jobNameCN;

        [Header("Primary Element")]
        public FiveElement PrimaryElement;
        public FiveElement SecondaryElement;

        [Header("Base Stats Growth (per level)")]
        public int hpGrowth = 10;
        public int mpGrowth = 5;
        public int atkGrowth = 3;
        public int matGrowth = 3;
        public int defGrowth = 2;
        public int mdfGrowth = 2;
        public int spdGrowth = 1;

        [Header("Combat Role")]
        [TextArea]
        public string description;
        public int baseAttackRange = 1;  // 基础攻击范围（格子数）
        public bool canCounterAttack = true;  // 是否可反击

        /// <summary>
        /// 获取职业定位描述
        /// </summary>
        public string GetRoleDescription()
        {
            return description;
        }

        /// <summary>
        /// 计算等级属性
        /// </summary>
        public void CalculateStats(int level, out int hp, out int mp, out int atk, out int mat, out int def, out int mdf, out int spd)
        {
            hp  = 100 + (level - 1) * hpGrowth;
            mp  = 30  + (level - 1) * mpGrowth;
            atk = 15  + (level - 1) * atkGrowth;
            mat = 15  + (level - 1) * matGrowth;
            def = 8   + (level - 1) * defGrowth;
            mdf = 8   + (level - 1) * mdfGrowth;
            spd = 8   + (level - 1) * spdGrowth;
        }
    }

    /// <summary>
    /// 转职路径定义
    /// </summary>
    public static class ClassEvolution
    {
        // 一级职业 → 二级职业
        public static readonly JobType[,] EvolutionTree = new JobType[,]
        {
            // 一级 → 二级 → 三级
            { JobType.Swordsman, JobType.Swordsman, JobType.Swordsman },  // 占位（不用）
        };

        public static string GetEvolutionPath(JobType currentJob)
        {
            return currentJob switch
            {
                JobType.Swordsman => "剑士 → 剑豪/圣剑士 → 剑圣",
                JobType.Blademaster => "刀客 → 狂战士/疾风 → 刀神",
                JobType.Spearman => "枪兵 → 铁卫/战盾 → 枪王",
                JobType.Archer => "弓手 → 神射手/游侠 → 箭圣",
                JobType.Mage => "法师 → 大法师/咒术师 → 法神",
                JobType.Healer => "医者 → 圣手/蛊师 → 医仙",
                _ => "无转职"
            };
        }
    }
}
