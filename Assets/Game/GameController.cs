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

public class GameController : NetworkBehaviour {
    // Player characters
    public GameObject newton;
    public GameObject kelvin;

    // Map from character names to the properties set via the editor.
    Dictionary<string, GameObject> characters;

    // Items
    public GameObject bomb;

    // Active players, even if dead. Null if no game is running,
    // guaranteed not null if a game is running.
    GameObject[] activePlayersGo;
    Player[] activePlayers;
    PlayerData[] activePlayersData;

    [SyncEvent]
    public event DamageChangedHandler EventHealthChanged;

    public static GameController Instance { get; private set; }

    public PlayerData GetPlayerData(int id) {
        return activePlayersData[id];
    }

    // TODO: This class should create players, won't need this when that's working.
    public void RegisterNewPlayer(GameObject playerObject) {
        activePlayersGo = activePlayersGo.Concat(new[] { playerObject } ).ToArray();
        activePlayersData = activePlayersGo.Select(p => p.GetComponent<PlayerData>()).ToArray();
        activePlayersData[activePlayersData.Length - 1].id = activePlayersData.Length - 1;
    }

    [Server]
    public void TakeDamage(GameObject playerObject, int amount) {
        var player = playerObject.GetComponent<PlayerData>();
        player.damage += amount;
        var args = new DamageChangedEventArgs {
            playerId = player.id,
            newDamage = player.damage,
        };
        EventHealthChanged(args);
    }

    [Server]
    public void Knockback(GameObject attackingObject, GameObject playerObject, float amount) {
        var data = playerObject.GetComponent<PlayerData>();
        amount *= data.damage;
        var vector = playerObject.transform.position - attackingObject.transform.position;
        var force = transform.up * amount;
        if (vector.x < 0) {
            amount = -amount;
        }
        force += transform.right * amount;
        data.player.RpcKnockback(force);
    }

    void Awake() {
        Instance = this;
        characters = new Dictionary<string, GameObject>() {
            { "Newton", newton },
            { "Kelvin", kelvin },
        };
        activePlayersGo = new GameObject[0];
    }

    float nextItemTime;
    void Update() {
        if (Time.time > nextItemTime) {
            nextItemTime = Time.time + Random.Range(7f, 15f);
            CmdSpawnItem();
        }
    }

    [Command]
    void CmdSpawnItem() {
        Instantiate(bomb, new Vector2(Random.Range(-6f, 6f), 5f), Quaternion.identity);
        NetworkServer.Spawn(bomb);
    }
}