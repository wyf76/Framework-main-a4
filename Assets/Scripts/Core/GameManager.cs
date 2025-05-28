using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        PREGAME,
        INWAVE,
        WAVEEND,
        COUNTDOWN,
        GAMEOVER
    }
    public GameState state;
    public int damageDealt;
    public int damageReceived;
    public int timeSpent;
    public int countdown;
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("GameManager");
                    _instance = obj.AddComponent<GameManager>();
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        enemies = new List<GameObject > ();
    }

    public GameObject player;
    public int currentWave = 1;
    public string selectedClass = "mage"; //default class
    public PlayerClass currentClass;


    public ProjectileManager projectileManager;
    public SpellIconManager spellIconManager;
    public EnemySpriteManager enemySpriteManager;
    public PlayerSpriteManager playerSpriteManager;
    public RelicIconManager relicIconManager;

    private List<GameObject> enemies;
    public int enemy_count { get { return enemies.Count; } }

    public void AddEnemy(GameObject enemy)
    {
        enemies.Add(enemy);
    }
    public void RemoveEnemy(GameObject enemy)
    {
        enemies.Remove(enemy);
    }

    public void NextWave()
    {
        state = GameState.COUNTDOWN;

        EnemySpawner spawner = UnityEngine.Object.FindAnyObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            CoroutineManager.Instance.Run(spawner.SpawnWave());
        }
        else
        {
            Debug.LogError("Could not find EnemySpawner.");
        }
    }

    public GameObject GetClosestEnemy(Vector3 point)
    {
        if (enemies == null || enemies.Count == 0) return null;
        if (enemies.Count == 1) return enemies[0];
        return enemies.Aggregate((a, b) => (a.transform.position - point).sqrMagnitude < (b.transform.position - point).sqrMagnitude ? a : b);
    }

    private GameManager()
    {
        damageDealt = 0;
        damageReceived = 0;
        timeSpent = 0;
        enemies = new List<GameObject>();
    }
    public void resetGame()
    {
        damageDealt = 0;
        damageReceived = 0;
        timeSpent = 0;
        currentWave = 1;
        enemies.Clear();
    }
    public void SetClass(string className)
    {
        selectedClass = className;
        currentClass = ClassManager.Classes[className];
    }
}
