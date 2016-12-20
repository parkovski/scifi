using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class DamageChangedEventArgs : EventArgs {
    public IPlayer Player { get; set; }
    public int DeltaDamage { get; set; }
}
public delegate void DamageChangedHandler(object sender, DamageChangedEventArgs args);

public class LifeChangedEventArgs : EventArgs {
    public IPlayer Player { get; set; }
    public int DeltaLives { get; set; }
}
public delegate void LifeChangedHandler(object sender, LifeChangedEventArgs args);

public class GameController : MonoBehaviour {
    // Player characters
    public GameObject newton;

    // Map from character names to the properties set via the editor.
    Dictionary<string, GameObject> characters;

    // Active players, even if dead. Null if no game is running,
    // guaranteed not null if a game is running.
    GameObject[] activePlayersGo;
    IPlayer[] activePlayers;

    public event DamageChangedHandler HealthChanged;

    public void TakeDamage(IPlayer player, int amount) {
        var args = new DamageChangedEventArgs {
            Player = player,
            DeltaDamage = amount,
        };
        HealthChanged(this, args);
    }

    void Start() {
        characters = new Dictionary<string, GameObject>() {
            { "Newton", newton }
        };

        activePlayersGo = new[] {
            Instantiate(newton, new Vector2(0, 0), Quaternion.identity)
        };
        activePlayers = activePlayersGo.Select(p => {
            var proxy = p.GetComponent<PlayerProxy>();
            proxy.GameController = this;
            return proxy.PlayerDelegate;
        }).ToArray();
    }

    //
}