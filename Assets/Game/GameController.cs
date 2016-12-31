using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public class DamageChangedEventArgs : EventArgs {
    public int playerId;
    public int newDamage;
}
public delegate void DamageChangedHandler(DamageChangedEventArgs args);

public class LifeChangedEventArgs : EventArgs {
    public int playerId;
    public int newLives;
}
public delegate void LifeChangedHandler(LifeChangedEventArgs args);

public static class Layers {
    public static int projectiles;
    public static int items;
    public static int players;

    public static void Init() {
        projectiles = LayerMask.NameToLayer("Projectiles");
        items = LayerMask.NameToLayer("Items");
        players = LayerMask.NameToLayer("Players");
    }
}

public class GameController : NetworkBehaviour {
    // Player characters
    public GameObject newton;
    public GameObject kelvin;

    // Map from character names to the properties set via the editor.
    Dictionary<string, GameObject> characters;

    // Items
    public GameObject bomb;
    public GameObject bow;
    public GameObject arrow;
    public GameObject fireArrow;
    public GameObject bombArrow;
    public GameObject rockArrow;

    // Active players, even if dead. Null if no game is running,
    // guaranteed not null if a game is running.
    GameObject[] activePlayersGo;
    Player[] activePlayers;

    [SyncEvent]
    public event DamageChangedHandler EventDamageChanged;
    /// Implies damage is set to 0.
    [SyncEvent]
    public event LifeChangedHandler EventLifeChanged;

    public static GameController Instance { get; private set; }

    [Server]
    public void RegisterNewPlayer(NetworkConnection conn, short controllerId, string name) {
        var playerObject = newton;
        characters.TryGetValue(name, out playerObject);
        playerObject = Instantiate(playerObject, Vector2.zero, Quaternion.identity);
        NetworkServer.AddPlayerForConnection(conn, playerObject, controllerId);

        activePlayersGo = activePlayersGo.Concat(new[] { playerObject }).ToArray();
        activePlayers = activePlayersGo.Select(p => p.GetComponent<Player>()).ToArray();
        var player = activePlayers[activePlayers.Length - 1];
        player.id = activePlayers.Length - 1;
        player.lives = 5;
        EventLifeChanged(new LifeChangedEventArgs {
            playerId = player.id,
            newLives = player.lives,
        });
    }

    [Command]
    public void CmdDie(GameObject playerObject) {
        var player = playerObject.GetComponent<Player>();
        --player.lives;
        player.damage = 0;
        player.RpcRespawn(new Vector3(0f, 7f));
        EventLifeChanged(new LifeChangedEventArgs {
            playerId = player.id,
            newLives = player.lives,
        });
    }

    [Server]
    public void TakeDamage(GameObject playerObject, int amount) {
        var player = playerObject.GetComponent<Player>();
        player.damage += amount;
        var args = new DamageChangedEventArgs {
            playerId = player.id,
            newDamage = player.damage,
        };
        EventDamageChanged(args);
    }

    [Server]
    public void Knockback(GameObject attackingObject, GameObject playerObject, float amount) {
        var player = playerObject.GetComponent<Player>();
        amount *= player.damage;
        var vector = playerObject.transform.position - attackingObject.transform.position;
        var force = transform.up * amount;
        if (vector.x < 0) {
            amount = -amount;
        }
        force += transform.right * amount;
        player.RpcKnockback(force);
    }

    void Awake() {
        Instance = this;
        characters = new Dictionary<string, GameObject>() {
            { "Newton", newton },
            { "Kelvin", kelvin },
        };
        activePlayersGo = new GameObject[0];
        Layers.Init();
    }

    float nextItemTime;
    void Update() {
        if (!isServer) {
            return;
        }

        if (Time.time > nextItemTime) {
            nextItemTime = Time.time + Random.Range(7f, 15f);
            SpawnItem();
        }
    }

    [Server]
    void SpawnItem() {
        var prefab = Random.Range(0f, 1f) > .5f ? bomb : bow;
        var item = Instantiate(prefab, new Vector2(Random.Range(-6f, 6f), 5f), Quaternion.identity);
        NetworkServer.Spawn(item);
    }
}