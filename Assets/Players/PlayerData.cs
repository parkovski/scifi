using UnityEngine;
using UnityEngine.Networking;

public class PlayerData : NetworkBehaviour {
    [SyncVar]
    public int id;
    [SyncVar]
    public string displayName;
    [SyncVar]
    public int lives;
    [SyncVar]
    public int damage;

    static int idCounter = 0;
    public override void OnStartServer() {
        id = ++idCounter;
    }
}