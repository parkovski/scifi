using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using System.Linq;

public class DamageChangedEventArgs : EventArgs {
    public PlayerData player;
    public int deltaDamage;
}
public delegate void DamageChangedHandler(DamageChangedEventArgs args);

public class LifeChangedEventArgs : EventArgs {
    public PlayerData player;
    public int deltaLives;
}
public delegate void LifeChangedHandler(LifeChangedEventArgs args);

public class GameController : NetworkBehaviour {
    // Player characters
    public GameObject newton;

    // Map from character names to the properties set via the editor.
    Dictionary<string, GameObject> characters;

    // Active players, even if dead. Null if no game is running,
    // guaranteed not null if a game is running.
    GameObject[] activePlayersGo;
    Player[] activePlayers;

    [SyncEvent]
    public event DamageChangedHandler EventHealthChanged;

    public static GameController Instance { get; private set; }

    [Server]
    public void TakeDamage(GameObject playerObject, int amount) {
        var player = playerObject.GetComponent<PlayerData>();
        player.damage += amount;
        var args = new DamageChangedEventArgs {
            player = player,
            deltaDamage = amount,
        };
        EventHealthChanged(args);
    }

    void Awake() {
        Instance = this;
        characters = new Dictionary<string, GameObject>() {
            { "Newton", newton }
        };

        /*activePlayersGo = new[] {
            //Instantiate(newton, new Vector2(0, 0), Quaternion.identity)
        };
        activePlayers = activePlayersGo.Select(p => {
            var proxy = p.GetComponent<PlayerProxy>();
            proxy.GameController = this;
            return proxy.PlayerDelegate;
        }).ToArray();*/
    }
}