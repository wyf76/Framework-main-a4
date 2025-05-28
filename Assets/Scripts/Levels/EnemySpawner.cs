using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EnemySpawner : MonoBehaviour
{
    #region Inspector
    [Header("UI References")]
    public Image level_selector;
    public GameObject button;

    [Header("Prefabs & Spawn Points")]
    public GameObject enemy;
    public SpawnPoint[] SpawnPoints;

    [Header("Runtime Data (do not edit)")]
    public Dictionary<string, Enemy> enemy_types;
    public Dictionary<string, Level> levels;
    public TextMeshProUGUI gameOver;
    #endregion

    private const float DEFAULT_DELAY = 1f;
    private int currentWave = 1;
    private string current_level;

    public int GetCurrentWave() => currentWave;

    #region MonoBehaviour
    private void Start()
    {
        CreateLevelButtons();
        LoadEnemyAndLevelData();
        StartCoroutine(LevelTimer());
    }
    #endregion

    #region Public API (called by other scripts/UI)
    public void StartLevel(string levelName)
    {
        level_selector.gameObject.SetActive(false);
        current_level = levelName;

        currentWave = 1;
        GameManager.Instance.currentWave = 1;

        GameManager.Instance.player.GetComponent<PlayerController>().StartLevel();
        StartCoroutine(SpawnWave());
    }

    public void NextWave()
    {
        if (GameManager.Instance.state == GameManager.GameState.GAMEOVER)
        {
            GameManager.Instance.resetGame();
            Debug.Log("restarted");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            GameManager.Instance.state = GameManager.GameState.PREGAME;
            return;
        }

        StartCoroutine(SpawnWave());
    }

    public int EvaluateRpn(string expression, int enemyBaseHp = 0)
    {
        if (string.IsNullOrWhiteSpace(expression)) return 0;

        static int DivSafe(int a, int b) => b == 0 ? 0 : a / b;
        static int ModSafe(int a, int b) => b == 0 ? 0 : a % b;

        var ops = new Dictionary<string, System.Func<int, int, int>>(5)
        {
            ["+"] = (a, b) => a + b,
            ["-"] = (a, b) => a - b,
            ["*"] = (a, b) => a * b,
            ["/"] = DivSafe,
            ["%"] = ModSafe
        };

        Stack<int> stack = new();
        foreach (string token in expression.Split(' '))
        {
            if (ops.TryGetValue(token, out var op))
            {
                if (stack.Count < 2) return 0;
                int b = stack.Pop();
                int a = stack.Pop();
                stack.Push(op(a, b));
            }
            else
            {
                switch (token)
                {
                    case "wave":
                        stack.Push(currentWave);
                        break;
                    case "base":
                        stack.Push(enemyBaseHp);
                        break;
                    default:
                        if (int.TryParse(token, out int value))
                            stack.Push(value);
                        break;
                }
            }
        }
        return stack.Count > 0 ? stack.Pop() : 0;
    }
    #endregion

    #region Coroutines
    public IEnumerator SpawnWave()
    {
        GameManager.Instance.state = GameManager.GameState.COUNTDOWN;
        GameManager.Instance.countdown = 3;

        for (int i = 3; i > 0; i--)
        {
            yield return new WaitForSeconds(1f);
            GameManager.Instance.countdown--;
        }

        GameManager.Instance.state = GameManager.GameState.INWAVE;
        GameManager.Instance.currentWave = currentWave;

        Level level = levels[current_level];
        Debug.Log($"Spawning wave {currentWave} from level {current_level}");

        foreach (Spawn spawn in level.spawns)
        {
            StartCoroutine(ManageWave(spawn));
        }

        yield return new WaitWhile(() => GameManager.Instance.enemy_count > 0);

        currentWave++;
        if (currentWave > level.waves)
        {
            GameManager.Instance.state = GameManager.GameState.GAMEOVER;
        }
        else if (GameManager.Instance.state is not (GameManager.GameState.GAMEOVER or GameManager.GameState.PREGAME))
        {
            GameManager.Instance.state = GameManager.GameState.WAVEEND;
            RewardScreenManager.Instance.ShowReward();
        }
    }

    private IEnumerator ManageWave(Spawn spawn)
    {
        int[] sequence = spawn.sequence?.Length > 0 ? spawn.sequence : new[] { 1 };
        int sequenceIndex = 0;

        int totalToSpawn = EvaluateRpn(spawn.count);
        Enemy enemyTemplate = enemy_types[spawn.enemy];
        int baseHp = enemyTemplate.hp;
        int modifiedHp = EvaluateRpn(spawn.hp, baseHp);
        int spawned = 0;

        int[] spawnSpaces = ParseSpawn(spawn.location);
        float delay = spawn.delay > 0 ? spawn.delay : DEFAULT_DELAY;

        while (spawned < totalToSpawn)
        {
            int batch = sequence[sequenceIndex];
            for (int i = 0; i < batch && spawned < totalToSpawn; i++)
            {
                yield return SpawnEnemy(spawn.enemy, modifiedHp, spawnSpaces);
                spawned++;
            }

            sequenceIndex = (sequenceIndex + 1) % sequence.Length;
            yield return new WaitForSeconds(delay);
        }
    }

    private IEnumerator SpawnEnemy(string enemyName, int hp, int[] spawnLocation)
    {
        Enemy stats = enemy_types[enemyName];
        SpawnPoint spawnPoint = SpawnPoints[Random.Range(spawnLocation[0], spawnLocation[0] + spawnLocation[1])];
        Vector2 offset = Random.insideUnitCircle * 1.8f;
        Vector3 position = spawnPoint.transform.position + new Vector3(offset.x, offset.y, 0);

        GameObject newEnemy = Instantiate(enemy, position, Quaternion.identity);
        newEnemy.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.enemySpriteManager.Get(stats.sprite);

        EnemyController controller = newEnemy.GetComponent<EnemyController>();
        controller.hp = new Hittable(hp, Hittable.Team.MONSTERS, newEnemy);
        controller.speed = stats.speed;
        controller.damage = stats.damage;

        GameManager.Instance.AddEnemy(newEnemy);
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator LevelTimer()
    {
        while (GameManager.Instance.state != GameManager.GameState.GAMEOVER)
        {
            if (GameManager.Instance.state != GameManager.GameState.WAVEEND)
            {
                GameManager.Instance.timeSpent += 1;
                yield return new WaitForSeconds(1f);
            }
            else
            {
                yield return new WaitWhile(() => GameManager.Instance.state == GameManager.GameState.WAVEEND);
            }
        }
    }
    #endregion

    #region Helpers
    private void CreateLevelButtons()
    {
        (string name, float yOffset)[] levelsInfo =
        {
            ("Easy",  40f),
            ("Medium",  0f),
            ("Endless", -40f)
        };

        foreach (var (levelName, y) in levelsInfo)
        {
            GameObject selector = Instantiate(button, level_selector.transform);
            selector.transform.localPosition = new Vector3(0, y, 0);

            MenuSelectorController controller = selector.GetComponent<MenuSelectorController>();
            controller.spawner = this;
            controller.SetLevel(levelName);
        }
    }

    private void LoadEnemyAndLevelData()
    {
        enemy_types = new Dictionary<string, Enemy>();
        TextAsset enemyText = Resources.Load<TextAsset>("enemies");
        foreach (JToken token in JToken.Parse(enemyText.text))
        {
            Enemy en = token.ToObject<Enemy>();
            enemy_types[en.name] = en;
        }

        levels = new Dictionary<string, Level>();
        TextAsset levelText = Resources.Load<TextAsset>("levels");
        foreach (JToken token in JToken.Parse(levelText.text))
        {
            Level lvl = token.ToObject<Level>();
            levels[lvl.name] = lvl;
        }
    }

    private static int[] ParseSpawn(string spawnString) => spawnString switch
    {
        "random green" => new[] { 0, 3 },
        "random bone"  => new[] { 3, 1 },
        "random red"   => new[] { 4, 3 },
        "random"       => new[] { 0, 7 },
        _               => new[] { 0, 7 },
    };
    #endregion
}