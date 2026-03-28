using UnityEngine;
using System.Collections.Generic;

namespace CangQiong.Battle
{
    /// <summary>
    /// 地形类型（影响移动和战斗）
    /// </summary>
    public enum TerrainType
    {
        Plain,    // 平原 — 移动正常
        Mountain, // 山地 — 不可通行
        Water,    // 水域 — 不可通行
        Forest,   // 森林 — 移动+1，防御+10%
        Road,     // 道路 — 移动免费
        Wall,     // 城墙 — 不可通行
    }

    /// <summary>
    /// 格子数据
    /// </summary>
    [System.Serializable]
    public class TileData
    {
        public Vector2Int position;
        public TerrainType terrain = TerrainType.Plain;
        public bool isWalkable = true;
        public bool isAttackable = true;
        public int moveCost = 1;        // 移动费用
        public float defenseBonus = 0f; // 防御加成
        public Unit unit;                // 当前站在这个格子的单位
    }

    /// <summary>
    /// 战棋格子系统
    /// 
    /// 职责：
    /// - 管理 8×6 格地图数据
    /// - BFS 计算移动范围
    /// - 计算攻击范围
    /// - 地形效果
    /// </summary>
    public class TileMap : MonoBehaviour
    {
        public static TileMap Instance { get; private set; }

        [Header("Map Config")]
        public int mapWidth = 8;
        public int mapHeight = 6;
        public float tileSize = 1f;  // Unity 单位（米）

        [Header("Terrain")]
        public TerrainType[] terrainGrid;  // 扁平化地图数据 [y * width + x]

        [Header("Visuals")]
        public GameObject tilePrefab;      // 格子视觉预制件
        public Material plainMat;
        public Material mountainMat;
        public Material waterMat;
        public Material forestMat;
        public Material roadMat;

        private TileData[,] tiles;

        public event System.Action<Vector2Int> OnTileClicked;
        public event System.Action<Vector2Int> OnTileHovered;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            InitializeMap();
        }

        /// <summary>
        /// 初始化地图
        /// </summary>
        public void InitializeMap()
        {
            tiles = new TileData[mapWidth, mapHeight];

            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    int index = y * mapWidth + x;
                    TerrainType terrain = (terrainGrid != null && index < terrainGrid.Length)
                        ? terrainGrid[index]
                        : TerrainType.Plain;

                    tiles[x, y] = new TileData
                    {
                        position = new Vector2Int(x, y),
                        terrain = terrain,
                        isWalkable = CanWalkOn(terrain),
                        moveCost = GetMoveCost(terrain),
                        defenseBonus = GetDefenseBonus(terrain)
                    };
                }
            }

            Debug.Log($"[TileMap] 地图初始化完成：{mapWidth}×{mapHeight}");
        }

        /// <summary>
        /// 获取格子数据
        /// </summary>
        public TileData GetTile(int x, int y)
        {
            if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight)
                return null;
            return tiles[x, y];
        }

        public TileData GetTile(Vector2Int pos) => GetTile(pos.x, pos.y);

        /// <summary>
        /// 判断某地形是否可以行走
        /// </summary>
        public static bool CanWalkOn(TerrainType terrain)
        {
            return terrain != TerrainType.Mountain
                && terrain != TerrainType.Water
                && terrain != TerrainType.Wall;
        }

        /// <summary>
        /// 获取移动费用
        /// </summary>
        public static int GetMoveCost(TerrainType terrain)
        {
            return terrain switch
            {
                TerrainType.Road => 0,    // 道路免费
                TerrainType.Forest => 2,  // 森林移动消耗+1
                TerrainType.Plain => 1,   // 平原正常
                _ => 1
            };
        }

        /// <summary>
        /// 获取防御加成
        /// </summary>
        public static float GetDefenseBonus(TerrainType terrain)
        {
            return terrain switch
            {
                TerrainType.Mountain => 0.2f,  // 山地防御+20%
                TerrainType.Forest => 0.1f,    // 森林防御+10%
                TerrainType.Wall => 0.4f,      // 城墙防御+40%
                _ => 0f
            };
        }

        /// <summary>
        /// 世界坐标 → 格子坐标
        /// </summary>
        public Vector2Int WorldToGrid(Vector3 worldPos)
        {
            int x = Mathf.RoundToInt(worldPos.x / tileSize);
            int y = Mathf.RoundToInt(worldPos.z / tileSize);
            return new Vector2Int(x, y);
        }

        /// <summary>
        /// 格子坐标 → 世界坐标（格子中心点）
        /// </summary>
        public Vector3 GridToWorld(Vector2Int gridPos)
        {
            return new Vector3(
                gridPos.x * tileSize,
                0,
                gridPos.y * tileSize
            );
        }

        /// <summary>
        /// BFS 计算可移动范围
        /// </summary>
        /// <param name="startPos">起始格子</param>
        /// <param name="moveRange">基础移动力</param>
        /// <returns>可到达的格子集合</returns>
        public HashSet<Vector2Int> CalculateMoveRange(Vector2Int startPos, int moveRange)
        {
            var result = new HashSet<Vector2Int>();
            result.Add(startPos);

            var visited = new Dictionary<Vector2Int, int>();  // pos -> 最小花费
            visited[startPos] = 0;

            var queue = new Queue<Vector2Int>();
            queue.Enqueue(startPos);

            var directions = new Vector2Int[]
            {
                new Vector2Int(0, 1),   // 上
                new Vector2Int(0, -1),  // 下
                new Vector2Int(1, 0),   // 右
                new Vector2Int(-1, 0),  // 左
                new Vector2Int(1, 1),   // 右上
                new Vector2Int(1, -1),  // 右下
                new Vector2Int(-1, 1),  // 左上
                new Vector2Int(-1, -1), // 左下
            };

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                int currentCost = visited[current];

                foreach (var dir in directions)
                {
                    var next = current + dir;
                    if (!IsValidPosition(next))
                        continue;

                    var tile = GetTile(next);
                    if (tile == null || !tile.isWalkable || tile.unit != null)
                        continue;  // 不可通行或已有单位

                    int moveCost = tile.moveCost;
                    int totalCost = currentCost + moveCost;

                    if (totalCost <= moveRange)
                    {
                        if (!visited.ContainsKey(next) || visited[next] > totalCost)
                        {
                            visited[next] = totalCost;
                            result.Add(next);
                            queue.Enqueue(next);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 计算攻击范围（指定格子为中心，指定半径）
        /// </summary>
        /// <param name="center">中心格子</param>
        /// <param name="range">攻击范围（格子数）</param>
        /// <returns>可攻击的格子集合</returns>
        public HashSet<Vector2Int> CalculateAttackRange(Vector2Int center, int range)
        {
            var result = new HashSet<Vector2Int>();

            for (int x = center.x - range; x <= center.x + range; x++)
            {
                for (int y = center.y - range; y <= center.y + range; y++)
                {
                    var pos = new Vector2Int(x, y);
                    if (!IsValidPosition(pos))
                        continue;

                    var tile = GetTile(pos);
                    if (tile != null && tile.isAttackable)
                        result.Add(pos);
                }
            }

            return result;
        }

        /// <summary>
        /// 检查位置是否有效
        /// </summary>
        public bool IsValidPosition(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < mapWidth && pos.y >= 0 && pos.y < mapHeight;
        }

        /// <summary>
        /// 判断两个格子是否相邻（支持八方向）
        /// </summary>
        public bool IsAdjacent(Vector2Int a, Vector2Int b)
        {
            int dx = Mathf.Abs(a.x - b.x);
            int dy = Mathf.Abs(a.y - b.y);
            return dx <= 1 && dy <= 1 && (dx != 0 || dy != 0);
        }

        /// <summary>
        /// 获取两点间曼哈顿距离
        /// </summary>
        public int GetDistance(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        /// <summary>
        /// 在格子上放置单位
        /// </summary>
        public void PlaceUnit(Unit unit, Vector2Int pos)
        {
            if (unit == null) return;
            var tile = GetTile(pos);
            if (tile != null)
            {
                tile.unit = unit;
                unit.GridPosition = pos;
            }
        }

        /// <summary>
        /// 从格子上移除单位
        /// </summary>
        public void RemoveUnit(Unit unit)
        {
            if (unit == null) return;
            var tile = GetTile(unit.GridPosition);
            if (tile != null && tile.unit == unit)
                tile.unit = null;
        }

        /// <summary>
        /// 调试：打印地图
        /// </summary>
        public void PrintMap()
        {
            string mapStr = "=== 地图 ===\n";
            for (int y = mapHeight - 1; y >= 0; y--)
            {
                string row = $"{y}|";
                for (int x = 0; x < mapWidth; x++)
                {
                    var tile = tiles[x, y];
                    char c = tile?.unit != null ? 'U'
                           : tile?.terrain == TerrainType.Mountain ? 'M'
                           : tile?.terrain == TerrainType.Water ? 'W'
                           : tile?.terrain == TerrainType.Forest ? 'F'
                           : tile?.terrain == TerrainType.Road ? 'R'
                           : '.';
                    row += c;
                }
                mapStr += row + "\n";
            }
            mapStr += " +01234567\n";
            Debug.Log(mapStr);
        }
    }
}
