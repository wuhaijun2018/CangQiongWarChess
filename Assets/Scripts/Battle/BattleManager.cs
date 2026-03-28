using UnityEngine;

namespace CangQiong.Battle
{
    /// <summary>
    /// 战斗状态枚举
    /// </summary>
    public enum BattleState
    {
        Idle,
        PlayerTurn,      // 我方回合
        PlayerMoving,    // 玩家移动中
        PlayerActing,    // 玩家执行动作（攻击/技能）
        EnemyTurn,       // 敌方回合
        EnemyActing,     // 敌方行动
        Evaluating,      // 战斗评价结算
        BattleEnd,       // 战斗结束
    }

    /// <summary>
    /// 战斗结果
    /// </summary>
    public enum BattleResult
    {
        None,
        Victory,
        Defeat,
        Draw,
    }

    /// <summary>
    /// 战斗管理器 - 回合制状态机核心
    /// 
    /// 流程：
    /// StartBattle() → PlayerTurn → (PlayerMoving → PlayerActing) → EnemyTurn → EnemyActing → 循环
    /// → Victory/Defeat → EndBattle()
    /// </summary>
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        [Header("Battle Config")]
        [SerializeField] private int mapWidth = 8;
        [SerializeField] private int mapHeight = 6;
        [SerializeField] private int maxTurns = 30;  // 超时强制结束

        [Header("State")]
        public BattleState CurrentState { get; private set; } = BattleState.Idle;
        public BattleResult Result { get; private set; } = BattleResult.None;
        public int CurrentTurn { get; private set; } = 1;
        public int TurnCount { get; private set; } = 0;

        public event System.Action<BattleState> OnStateChanged;
        public event System.Action<BattleResult> OnBattleEnded;

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
        /// 开始战斗
        /// </summary>
        public void StartBattle()
        {
            CurrentState = BattleState.PlayerTurn;
            CurrentTurn = 1;
            TurnCount = 0;
            Result = BattleResult.None;
            OnStateChanged?.Invoke(CurrentState);
            Debug.Log($"[Battle] Battle Started! Turn {CurrentTurn}");
        }

        /// <summary>
        /// 状态切换（供外界调用）
        /// </summary>
        public void SetState(BattleState newState)
        {
            var oldState = CurrentState;
            CurrentState = newState;
            OnStateChanged?.Invoke(newState);
            Debug.Log($"[Battle] {oldState} → {newState}");
        }

        /// <summary>
        /// 玩家结束当前回合
        /// </summary>
        public void EndPlayerTurn()
        {
            if (CurrentState != BattleState.PlayerActing && CurrentState != BattleState.PlayerMoving)
                return;

            SetState(BattleState.EnemyTurn);
            TurnCount++;
        }

        /// <summary>
        /// 敌方回合结束，进入下一轮
        /// </summary>
        public void EndEnemyTurn()
        {
            if (CurrentState != BattleState.EnemyActing)
                return;

            CurrentTurn++;
            if (CurrentTurn > maxTurns)
            {
                EndBattle(BattleResult.Draw);
                return;
            }

            SetState(BattleState.PlayerTurn);
        }

        /// <summary>
        /// 结束战斗
        /// </summary>
        public void EndBattle(BattleResult result)
        {
            Result = result;
            SetState(BattleState.BattleEnd);
            OnBattleEnded?.Invoke(result);
            Debug.Log($"[Battle] Battle Ended: {result}");
        }

        /// <summary>
        /// 检查胜利条件（子类/数据驱动覆盖）
        /// </summary>
        public virtual bool CheckVictoryCondition()
        {
            return false; // 由关卡数据或 AI 触发
        }

        /// <summary>
        /// 检查失败条件
        /// </summary>
        public virtual bool CheckDefeatCondition()
        {
            return false;
        }
    }
}
