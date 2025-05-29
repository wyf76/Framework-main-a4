using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

public class GameManager
{
    public enum GameState { PREGAME, INWAVE, WAVEEND, COUNTDOWN, GAMEOVER }

    public GameState state;
    public int countdown;

    private static GameManager theInstance;
    public static GameManager Instance
    {
        get
        {
            if (theInstance == null)
                theInstance = new GameManager();
            return theInstance;
        }
    }

    public GameObject player;
    public ProjectileManager projectileManager;
    public SpellIconManager spellIconManager;
    public EnemySpriteManager enemySpriteManager;
    public PlayerSpriteManager playerSpriteManager;
    public RelicIconManager relicIconManager;
    private CharacterClassDefinition selectedClass; 

    private readonly List<GameObject> enemies = new List<GameObject>();
    public int enemy_count => enemies.Count;
    
    public List<RelicDefinition> ownedRelics = new List<RelicDefinition>();


    public List<Enemy> enemyDefs { get; private set; }
    public List<Level> levelDefs { get; private set; }

    public bool playerWon;
    public bool IsPlayerDead;

    public int totalEnemiesKilled;
    public float timeSurvived;
    public int totalDamageDealt;
    public int totalDamageTaken;
    public int wavesCompleted;

    private GameManager()
    {
        var eTxt = Resources.Load<TextAsset>("enemies");
        if (eTxt == null)
            Debug.LogError("GameManager: missing enemies.json");
        else
            enemyDefs = JsonConvert.DeserializeObject<List<Enemy>>(eTxt.text);

        var lTxt = Resources.Load<TextAsset>("levels");
        if (lTxt == null)
            Debug.LogError("GameManager: missing levels.json");
        else
            levelDefs = JsonConvert.DeserializeObject<List<Level>>(lTxt.text);

        ResetGame();
    }
    
    public void SetSelectedClass(CharacterClassDefinition def) {
        selectedClass = def;
    }

    public CharacterClassDefinition GetSelectedClass() {
        return selectedClass;
    }


    public void ResetGame()
    {
        state = GameState.PREGAME;
        countdown = 0;
        IsPlayerDead = false;
        playerWon = false;
        totalEnemiesKilled = 0;
        totalDamageDealt = 0;
        totalDamageTaken = 0;
        timeSurvived = 0f;
        wavesCompleted = 0;
        selectedClass = null; 

        ownedRelics.Clear();


        if (player != null)
            Object.Destroy(player);
        player = null;

        foreach (var e in enemies.ToList())
            if (e != null) Object.Destroy(e);
        enemies.Clear();
    }

    public void AddEnemy(GameObject enemy) => enemies.Add(enemy);

    public void RemoveEnemy(GameObject enemy)
    {
        if (enemies.Remove(enemy))
            totalEnemiesKilled++;
    }

    public GameObject GetClosestEnemy(Vector3 point)
    {
        if (enemies.Count == 0) return null;
        if (enemies.Count == 1) return enemies[0];
        return enemies.Aggregate((a, b) =>
            (a.transform.position - point).sqrMagnitude < (b.transform.position - point).sqrMagnitude ? a : b);
    }
}