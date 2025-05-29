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

    private void TriggerWin()
    {
        GameManager.Instance.playerWon = true;
        GameManager.Instance.IsPlayerDead = false;
        GameManager.Instance.state = GameManager.GameState.GAMEOVER;
        Debug.Log(" You Win: all waves completed.");
    }

    void Start()
    {
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
        
        // Get the selected class from the GameManager
        selectedClass = GameManager.Instance.GetSelectedClass();
        if (selectedClass == null) {
            Debug.LogError("No class selected!");
            // Fallback to the first available class
            selectedClass = GameDataLoader.Classes.Values.First();
        }
    
        // --- ADD THE FOLLOWING DEBUG LOGS ---
        Debug.Log("Attempting to apply class. Selected Class is not null: " + (selectedClass != null));
        Debug.Log("Player object is not null: " + (GameManager.Instance.player != null));
        if(selectedClass != null)
        {
            Debug.Log("Attempting to apply sprite with index: " + selectedClass.sprite);
        }
        // --- END OF ADDED DEBUG LOGS ---
    if (GameManager.Instance.player != null && selectedClass != null)
    {
        Debug.Log("[EnemySpawner] Attempting to get SpriteRenderer from player's children...");
        // Make sure you are using GetComponentInChildren if the SpriteRenderer is on a child
        SpriteRenderer playerSpriteRenderer = GameManager.Instance.player.GetComponentInChildren<SpriteRenderer>(); 

        if (playerSpriteRenderer != null)
        {
            Debug.Log("[EnemySpawner] Found SpriteRenderer on child object: " + playerSpriteRenderer.gameObject.name);
            Debug.Log("[EnemySpawner] Sprite on child BEFORE change: " + (playerSpriteRenderer.sprite != null ? playerSpriteRenderer.sprite.name : "null"));

            Sprite newSprite = GameManager.Instance.playerSpriteManager.Get(selectedClass.sprite);
            if (newSprite != null)
            {
                Debug.Log("[EnemySpawner] Attempting to set new sprite: " + newSprite.name + " (from PlayerSpriteManager index " + selectedClass.sprite + ")");
                playerSpriteRenderer.sprite = newSprite; // The actual sprite assignment
                // Check immediately after assignment
                Debug.Log("[EnemySpawner] Sprite on child AFTER change: " + (playerSpriteRenderer.sprite != null ? playerSpriteRenderer.sprite.name : "null"));
                
                if (playerSpriteRenderer.sprite == newSprite) {
                    Debug.Log("[EnemySpawner] SUCCESS: Sprite assignment was successful in memory.");
                } else {
                    Debug.LogWarning("[EnemySpawner] WARNING: Sprite assignment in memory appears to have failed or was immediately overridden.");
                }
            }
            else
            {
                Debug.LogWarning("[EnemySpawner] PlayerSpriteManager.Get(" + selectedClass.sprite + ") returned a NULL sprite! Check PlayerSpriteManager setup and the sprite index for this class in classes.json.");
            }
        }
        else
        {
            Debug.LogWarning("[EnemySpawner] GetComponentInChildren<SpriteRenderer>() did NOT find a SpriteRenderer on the player object or any of its children!");
        }
    }
    else
    {
        Debug.LogWarning("[EnemySpawner] Player object or SelectedClass is null. Player found: " + (GameManager.Instance.player != null) + ", SelectedClass found: " + (selectedClass != null));
    }
        // if (GameManager.Instance.player != null && selectedClass != null)
        // {
        //     SpriteRenderer playerSpriteRenderer = GameManager.Instance.player.GetComponentInChildren<SpriteRenderer>();
        //     if (playerSpriteRenderer != null)
        //     {
        //         // Use the sprite index from the class definition to get the correct sprite
        //         playerSpriteRenderer.sprite = GameManager.Instance.playerSpriteManager.Get(selectedClass.sprite);
        //     }
        //     else
        //     {
        //         Debug.LogWarning("Player does not have a SpriteRenderer component to change the sprite!");
        //     }
        // }
        
        currentLevel = GameManager.Instance.levelDefs
            .Find(l => l.name == levelname);
        if (currentLevel == null)
        {
            Debug.LogError($"StartLevel failed: level '{levelname}' not found.");
            return;
        }

        currentWave = 1;

        if (GameManager.Instance.player == null)
        {
            Debug.LogError("GameManager.Instance.player is null before getting PlayerController!");
            return;
        }
        Debug.Log($"Getting PlayerController from GameManager.Instance.player: {GameManager.Instance.player.name}", GameManager.Instance.player);
        var playerController = GameManager.Instance.player.GetComponent<PlayerController>();

        if (playerController == null)
        {
            Debug.LogError("playerController is null in EnemySpawnerController.StartLevel!");
            return;
        }

        playerController.StartLevel();

        if (GameManager.Instance.player == null)
        {
            Debug.LogError("GameManager.Instance.player is null after calling StartLevel!");
            return;
        }
        Debug.Log($"After calling StartLevel, checking player again: {GameManager.Instance.player.name}", GameManager.Instance.player);

        ScalePlayerForWave(currentWave); 

        StartCoroutine(SpawnWave());
    }

    public void NextWave()
    {
        if (!waveInProgress)
            StartCoroutine(SpawnWave());
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
                en.hp = new Hittable(hp, Hittable.Team.MONSTERS, go);
                en.speed = (int)speed;

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