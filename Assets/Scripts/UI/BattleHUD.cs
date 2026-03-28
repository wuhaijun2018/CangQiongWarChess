using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CangQiong.Battle;
using CangQiong.Systems;

namespace CangQiong.UI
{
    /// <summary>
    /// 战斗 HUD 控制器
    /// 
    /// 显示：
    /// - 当前回合（我方/敌方）
    /// - 角色 HP/MP 条
    /// - 操作提示
    /// - 战斗评价
    /// </summary>
    public class BattleHUD : MonoBehaviour
    {
        public static BattleHUD Instance { get; private set; }

        [Header("Turn Info")]
        public TextMeshProUGUI turnText;
        public TextMeshProUGUI phaseText;

        [Header("Player Unit Panel")]
        public Image[] playerHpBars;      // 玩家角色 HP 条
        public Image[] playerMpBars;      // 玩家角色 MP 条
        public TextMeshProUGUI[] playerHpTexts;
        public TextMeshProUGUI[] playerMpTexts;
        public TextMeshProUGUI[] playerNames;

        [Header("Action Buttons")]
        public Button attackButton;
        public Button skillButton;
        public Button itemButton;
        public Button waitButton;

        [Header("Info Panel")]
        public TextMeshProUGUI selectedUnitInfo;
        public TextMeshProUGUI logText;

        [Header("Result Panel")]
        public GameObject resultPanel;
        public TextMeshProUGUI resultText;
        public Button restartButton;

        private string[] battleLog = new string[3];
        private int logIndex = 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // 监听战斗状态变化
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.OnStateChanged += OnBattleStateChanged;
                BattleManager.Instance.OnBattleEnded += OnBattleEnded;
            }

            // 按钮事件绑定
            attackButton?.onClick.AddListener(OnAttackClicked);
            skillButton?.onClick.AddListener(OnSkillClicked);
            waitButton?.onClick.AddListener(OnWaitClicked);
            restartButton?.onClick.AddListener(OnRestartClicked);

            resultPanel?.SetActive(false);
            UpdateTurnUI(BattleState.Idle, 1);
        }

        /// <summary>
        /// 战斗状态变化回调
        /// </summary>
        private void OnBattleStateChanged(BattleState newState)
        {
            UpdateTurnUI(newState, BattleManager.Instance.CurrentTurn);
            UpdateActionButtons(newState);
        }

        /// <summary>
        /// 更新回合显示
        /// </summary>
        public void UpdateTurnUI(BattleState state, int turn)
        {
            string turnStr = state switch
            {
                BattleState.PlayerTurn => $"第 {turn} 回合 — 我方回合",
                BattleState.EnemyTurn => $"第 {turn} 回合 — 敌方回合",
                BattleState.BattleEnd => "战斗结束",
                _ => $"第 {turn} 回合"
            };

            if (turnText != null)
                turnText.text = turnStr;

            if (phaseText != null)
                phaseText.text = state.ToString();
        }

        /// <summary>
        /// 更新玩家角色状态显示
        /// </summary>
        public void UpdatePlayerUnits(Character.Unit[] players)
        {
            if (players == null) return;

            for (int i = 0; i < playerHpBars.Length && i < players.Length; i++)
            {
                var unit = players[i];
                if (unit == null) continue;

                float hpRatio = (float)unit.CurrentHP / unit.MaxHP;
                float mpRatio = (float)unit.CurrentMP / unit.MaxMP;

                if (playerHpBars[i] != null)
                    playerHpBars[i].fillAmount = Mathf.Lerp(playerHpBars[i].fillAmount, hpRatio, Time.deltaTime * 5);

                if (playerMpBars[i] != null)
                    playerMpBars[i].fillAmount = mpRatio;

                if (playerHpTexts[i] != null)
                    playerHpTexts[i].text = $"{unit.CurrentHP}/{unit.MaxHP}";

                if (playerMpTexts[i] != null)
                    playerMpTexts[i].text = $"{unit.CurrentMP}/{unit.MaxMP}";

                if (playerNames[i] != null)
                    playerNames[i].text = unit.unitName;
            }
        }

        /// <summary>
        /// 更新选中单位信息
        /// </summary>
        public void UpdateSelectedUnit(Character.Unit unit)
        {
            if (unit == null)
            {
                if (selectedUnitInfo != null)
                    selectedUnitInfo.text = "未选中单位";
                return;
            }

            string elemIcon = FiveElementsSystem.GetElementIcon(unit.Element);
            string info = $"{unit.unitName}\n" +
                         $"HP: {unit.CurrentHP}/{unit.MaxHP}\n" +
                         $"MP: {unit.CurrentMP}/{unit.MaxMP}\n" +
                         $"ATK: {unit.ATK}  DEF: {unit.DEF}\n" +
                         $"五行: {elemIcon}";

            if (selectedUnitInfo != null)
                selectedUnitInfo.text = info;
        }

        /// <summary>
        /// 写战斗日志
        /// </summary>
        public void AddBattleLog(string message)
        {
            battleLog[logIndex % battleLog.Length] = message;
            logIndex++;

            if (logText != null)
            {
                string logs = string.Join("\n", battleLog);
                logText.text = logs;
            }

            Debug.Log($"[BattleLog] {message}");
        }

        /// <summary>
        /// 更新按钮状态
        /// </summary>
        private void UpdateActionButtons(BattleState state)
        {
            bool canAct = state == BattleState.PlayerTurn
                       || state == BattleState.PlayerMoving
                       || state == BattleState.PlayerActing;

            if (attackButton != null) attackButton.interactable = canAct;
            if (skillButton != null) skillButton.interactable = canAct;
            if (itemButton != null) itemButton.interactable = canAct;
            if (waitButton != null) waitButton.interactable = canAct;
        }

        /// <summary>
        /// 攻击按钮回调
        /// </summary>
        private void OnAttackClicked()
        {
            AddBattleLog("选择了【攻击】");
            // TODO: 切换到选择目标状态
        }

        /// <summary>
        /// 技能按钮回调
        /// </summary>
        private void OnSkillClicked()
        {
            AddBattleLog("选择了【技能】");
            // TODO: 打开技能面板
        }

        /// <summary>
        /// 待机按钮回调
        /// </summary>
        private void OnWaitClicked()
        {
            AddBattleLog("选择了【待机】");
            BattleManager.Instance?.EndPlayerTurn();
        }

        /// <summary>
        /// 战斗结束回调
        /// </summary>
        private void OnBattleEnded(BattleResult result)
        {
            resultPanel?.SetActive(true);

            string resultStr = result switch
            {
                BattleResult.Victory => "🎉 胜利！",
                BattleResult.Defeat => "💀 失败...",
                BattleResult.Draw => "⚖️ 平局",
                _ => ""
            };

            if (resultText != null)
                resultText.text = resultStr;

            // 计算战斗评价
            EvaluateBattle(result);
        }

        /// <summary>
        /// 计算战斗评价（1-3星）
        /// </summary>
        public void EvaluateBattle(BattleResult result)
        {
            if (result != BattleResult.Victory)
            {
                AddBattleLog("评价：无星（失败）");
                return;
            }

            // 简单评价：根据剩余 HP 计算
            int stars = 3;
            AddBattleLog($"评价：{stars}星 ★★★");
        }

        /// <summary>
        /// 重新开始
        /// </summary>
        private void OnRestartClicked()
        {
            resultPanel?.SetActive(false);
            BattleManager.Instance?.StartBattle();
        }
    }
}
