using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class EnemySpawnerController : MonoBehaviour
{
    public Image level_selector;
    public GameObject button;
    public GameObject enemy;
    public SpawnPoint[] SpawnPoints;
    public GameObject mainMenuPanel;
    public static event System.Action<int> OnWaveEnd;
    public static event System.Action<GameObject> OnEnemyKilled;

    public Level currentLevel { get; private set; }
    public int currentWave { get; private set; }
    public int lastWaveEnemyCount { get; private set; }
    private bool isEndless => currentLevel != null && currentLevel.waves <= 0;

    private bool waveInProgress = false;
    private CharacterClassDefinition selectedClass; // Store the selected class

    void Awake()
    {
        Debug.Log("[EnemySpawner] Awake() called - Game State: " + GameManager.Instance.state);
        
        // Ensure game is in PREGAME state
        if (GameManager.Instance.state != GameManager.GameState.PREGAME)
        {
            Debug.LogWarning("[EnemySpawner] Game not in PREGAME state! Resetting...");
            GameManager.Instance.ResetGame();
        }
    }

    private void TriggerWin()
    {
        GameManager.Instance.playerWon = true;
        GameManager.Instance.IsPlayerDead = false;
        GameManager.Instance.state = GameManager.GameState.GAMEOVER;
        Debug.Log(" You Win: all waves completed.");
    }

    void Start()
    {
        // Ensure the main menu panel is visible at start
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
            Debug.Log("[EnemySpawner] MainMenuPanel activated at start");
        }
        else
        {
            Debug.LogWarning("[EnemySpawner] MainMenuPanel is null! Make sure it's assigned in the Inspector");
        }
        
        // Ensure the level selector is visible at start
        if (level_selector != null && level_selector.gameObject != null)
        {
            level_selector.gameObject.SetActive(true);
            Debug.Log("[EnemySpawner] Level selector activated at start");
        }
        
        foreach (var lvl in GameManager.Instance.levelDefs)
        {
            GameObject selector = Instantiate(button, level_selector.transform);
            selector.transform.localPosition =
                new Vector3(0, 130 - 100 * GameManager.Instance.levelDefs.IndexOf(lvl));
            var controller = selector.GetComponent<MenuSelectorController>();
            controller.spawner = this;
            controller.mainMenuPanel = this.mainMenuPanel; // <-- ADD THIS LINE
            controller.SetLevel(lvl.name);
            selector.GetComponent<Button>().onClick.AddListener(controller.StartLevel);
        }
    }

    public void StartLevel(string levelname)
    {
        Debug.Log($"[EnemySpawner] StartLevel() called for '{levelname}'");
        level_selector.gameObject.SetActive(false);

        // 1) Fetch the selected class from GameManager
        selectedClass = GameManager.Instance.GetSelectedClass();
        if (selectedClass == null)
        {
            Debug.LogError("[EnemySpawner] No class selected! Falling back to first available.");
            selectedClass = GameDataLoader.Classes.Values.FirstOrDefault();
            if (selectedClass == null)
            {
                Debug.LogError("[EnemySpawner] Still no class found in GameDataLoader.Classes! Aborting StartLevel.");
                return;
            }
        }

        // 2) Find which LevelDef we want
        currentLevel = GameManager.Instance.levelDefs
            .Find(l => l.name == levelname);
        if (currentLevel == null)
        {
            Debug.LogError($"[EnemySpawner] StartLevel failed: level '{levelname}' not found in levelDefs.");
            return;
        }

        currentWave = 1;

        // 3) Ensure that GameManager.Instance.player is assigned
        if (GameManager.Instance.player == null)
        {
            Debug.LogError("[EnemySpawner] GameManager.Instance.player is null before getting PlayerController!");
            return;
        }

        // 4) Grab the PlayerController and call its StartLevel() first
        var playerController = GameManager.Instance.player.GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("[EnemySpawner] playerController is null in EnemySpawnerController.StartLevel!");
            return;
        }

        // This is where PlayerController will do its own setup (spawning, animations, etc.)
        playerController.StartLevel();

        // 5) Now that PlayerController.StartLevel() has run, we can safely swap the player's sprite.
        //    We do a Find on the child named "player sprite" to guarantee we hit the correct SpriteRenderer.
        if (GameManager.Instance.player != null && selectedClass != null)
        {
            SpriteRenderer playerSpriteRenderer = GameManager.Instance.player.transform
                                                    .Find("player sprite")
                                                    ?.GetComponent<SpriteRenderer>();

            if (playerSpriteRenderer != null)
            {
                Debug.Log("[EnemySpawner] Found SpriteRenderer on child: " 
                        + playerSpriteRenderer.gameObject.name);
                Debug.Log("[EnemySpawner] Sprite BEFORE change: " 
                        + (playerSpriteRenderer.sprite != null 
                            ? playerSpriteRenderer.sprite.name 
                            : "null"));

                Sprite newSprite = GameManager.Instance.playerSpriteManager.Get(selectedClass.sprite);
                if (newSprite != null)
                {
                    Debug.Log("[EnemySpawner] Assigning new sprite: " 
                            + newSprite.name 
                            + " (index " + selectedClass.sprite + ")");
                    playerSpriteRenderer.sprite = newSprite;

                    Debug.Log("[EnemySpawner] Sprite AFTER change: " 
                            + (playerSpriteRenderer.sprite != null 
                                ? playerSpriteRenderer.sprite.name 
                                : "null"));
                }
                else
                {
                    Debug.LogWarning("[EnemySpawner] PlayerSpriteManager.Get(" 
                                    + selectedClass.sprite 
                                    + ") returned null. Check your index.");
                }
            }
            else
            {
                Debug.LogWarning("[EnemySpawner] Could not find a child named 'player sprite' " +
                                "or it has no SpriteRenderer component!");
            }
        }
        else
        {
            Debug.LogWarning("[EnemySpawner] Skipping sprite swap: " +
                            "player or selectedClass is null. " +
                            "Player exists: " + (GameManager.Instance.player != null) + 
                            ", selectedClass exists: " + (selectedClass != null));
        }

        // 6) After sprite swap, do the usual wave setup
        ScalePlayerForWave(currentWave);

        StartCoroutine(SpawnWave());
    }


    public void NextWave()
    {
        if (!waveInProgress)
            StartCoroutine(SpawnWave());
    }

    public void SpawnEnemiesInRoom(string locationTag)
    {
        StartCoroutine(SpawnRoomEnemies(locationTag));
    }

    private IEnumerator SpawnRoomEnemies(string locationTag)
    {
        var roomSpawns = currentLevel.spawns
            .Where(s => s.location.ToUpper().Contains(locationTag.ToUpper()))
            .ToList();

        int totalSpawned = 0;
        foreach (var spawn in roomSpawns)
            yield return StartCoroutine(SpawnEnemies(spawn, c => totalSpawned += c));
    }

    IEnumerator SpawnWave()
    {
        if (waveInProgress) yield break;
        waveInProgress = true;

        ScalePlayerForWave(currentWave);
        GameManager.Instance.state = GameManager.GameState.COUNTDOWN;
        for (int i = 3; i > 0; i--)
        {
            GameManager.Instance.countdown = i;
            yield return new WaitForSeconds(1);
        }
        GameManager.Instance.countdown = 0;
        GameManager.Instance.state = GameManager.GameState.INWAVE;

        int totalSpawned = 0;
        foreach (var spawn in currentLevel.spawns)
            yield return StartCoroutine(SpawnEnemies(spawn, c => totalSpawned += c));
        lastWaveEnemyCount = totalSpawned;

        yield return new WaitWhile(() => GameManager.Instance.enemy_count > 0);

        if (!isEndless && currentWave >= currentLevel.waves)
        {
            TriggerWin();
            yield break;
        }

        GameManager.Instance.state = GameManager.GameState.WAVEEND;
        GameManager.Instance.wavesCompleted++;

        OnWaveEnd?.Invoke(currentWave); 

        currentWave++;
        waveInProgress = false;
    }

    IEnumerator SpawnEnemies(Spawn spawn, System.Action<int> onSpawnComplete = null)
    {
        var baseEnemy = GameManager.Instance.enemyDefs
                           .Find(e => e.name == spawn.enemy);
        if (baseEnemy == null) yield break;

        var vars = new Dictionary<string, int> {
            { "base", baseEnemy.hp },
            { "wave", currentWave }
        };

        int total = RPNEvaluator.SafeEvaluate(spawn.count, vars, 0);
        int hp = spawn.hp != null
            ? RPNEvaluator.SafeEvaluate(spawn.hp, vars, baseEnemy.hp)
            : baseEnemy.hp;
        float speed = spawn.speed != null
            ? RPNEvaluator.SafeEvaluate(spawn.speed, new() {
                  { "base", (int)baseEnemy.speed }, { "wave", currentWave }
              }, (int)baseEnemy.speed)
            : baseEnemy.speed;
        int damage = spawn.damage != null
            ? RPNEvaluator.SafeEvaluate(spawn.damage, new() {
                  { "base", baseEnemy.damage }, { "wave", currentWave }
              }, baseEnemy.damage)
            : baseEnemy.damage;
        float delay = spawn.delay != null
            ? RPNEvaluator.SafeEvaluate(spawn.delay, vars, 2)
            : 2f;

        speed = Mathf.Clamp(speed, 1f, 20f);
        var seq = (spawn.sequence != null && spawn.sequence.Count > 0)
                  ? spawn.sequence
                  : new List<int> { 1 };

        int spawned = 0, idx = 0;
        while (spawned < total)
        {
            int batch = seq[idx++ % seq.Count];
            for (int i = 0; i < batch && spawned < total; i++)
            {
                var pt = PickSpawnPoint(spawn.location);
                var ofs = GetNonOverlappingOffset(pt.transform.position);
                var pos = pt.transform.position + (Vector3)ofs;
                var go = Instantiate(enemy, pos, Quaternion.identity);
                go.GetComponent<SpriteRenderer>()
                  .sprite = GameManager.Instance.enemySpriteManager.Get(baseEnemy.sprite);

                var en = go.GetComponent<EnemyController>();
                en.enemyType = baseEnemy.name;
                en.hp = new Hittable(hp, Hittable.Team.MONSTERS, go);
                en.speed = (int)speed;
                en.damage = damage;
                en.spellKey = baseEnemy.spell;
                en.spellRange = baseEnemy.spell_range > 0 ? baseEnemy.spell_range : en.spellRange;

                en.hp.OnDeath += () => OnEnemyKilled?.Invoke(go);

                GameManager.Instance.AddEnemy(go);

                spawned++;
            }
            yield return new WaitForSeconds(delay);
        }

        onSpawnComplete?.Invoke(spawned);
    }

    private Vector2 GetNonOverlappingOffset(Vector3 center)
    {
        for (int i = 0; i < 10; i++)
        {
            var ofs = Random.insideUnitCircle * 3f;
            if (Physics2D.OverlapCircleAll(center + (Vector3)ofs, .75f).Length == 0)
                return ofs;
        }
        return Random.insideUnitCircle * 3f;
    }

    private SpawnPoint PickSpawnPoint(string loc)
    {
        if (string.IsNullOrEmpty(loc) || !loc.StartsWith("random"))
            return SpawnPoints[Random.Range(0, SpawnPoints.Length)];
        if (loc == "random")
            return SpawnPoints[Random.Range(0, SpawnPoints.Length)];

        var kind = loc.Split(' ')[1].Trim().ToUpperInvariant();
        var matches = SpawnPoints
            .Where(sp => sp.kind.ToString().ToUpperInvariant() == kind)
            .ToList();
        return matches.Count > 0
            ? matches[Random.Range(0, matches.Count)]
            : SpawnPoints[Random.Range(0, SpawnPoints.Length)];
    }

    private void ScalePlayerForWave(int wave)
    {
        if (selectedClass == null) {
            Debug.LogError("Cannot scale player stats, no class is selected!");
            return;
        }

        Debug.Log($"[EnemySpawner] ScalePlayerForWave({wave}) using class '{selectedClass}'");

        var v = new Dictionary<string, float> { { "wave", wave } };
        float rHP = RPNEvaluator.SafeEvaluateFloat(selectedClass.health, v);
        float rMana = RPNEvaluator.SafeEvaluateFloat(selectedClass.mana, v);
        float rRe = RPNEvaluator.SafeEvaluateFloat(selectedClass.mana_regeneration, v);
        float rPow = RPNEvaluator.SafeEvaluateFloat(selectedClass.spellpower, v);
        float rSpd = RPNEvaluator.SafeEvaluateFloat(selectedClass.speed, v);

        var pc = GameManager.Instance.player.GetComponent<PlayerController>();
        if (pc == null)
        {
            Debug.LogError("ScalePlayerForWave: no PlayerController!");
            return;
        }

        int newMaxHP = Mathf.RoundToInt(rHP);
        pc.hp.SetMaxHP(newMaxHP, true);

        pc.spellcaster.max_mana = Mathf.RoundToInt(rMana);
        pc.spellcaster.mana = pc.spellcaster.max_mana;
        pc.spellcaster.mana_reg = Mathf.RoundToInt(rRe);
        pc.spellcaster.spellPower = Mathf.RoundToInt(rPow);
        pc.speed = Mathf.RoundToInt(rSpd);

        pc.healthui.SetHealth(pc.hp);
        pc.manaui.SetSpellCaster(pc.spellcaster);

        Debug.Log($" → PlayerStats: HP={pc.hp.hp}/{pc.hp.max_hp}, Mana={rMana:F1}, Regen={rRe:F1}, " +
                  $"Power={rPow:F1}, Speed={rSpd:F1}");
    }
}