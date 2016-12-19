using UnityEngine;

public interface IPlayer {
    void TakeDamage(int amount);
}

class EmptyPlayer : IPlayer {
    public void TakeDamage(int amount) {}
}

public class PlayerProxy : MonoBehaviour, IPlayer {
    IPlayer playerDelegate = new EmptyPlayer();
    public IPlayer PlayerDelegate {
        get {
            return playerDelegate;
        }
        set {
            playerDelegate = value;
        }
    }

    public void TakeDamage(int amount) {
        playerDelegate.TakeDamage(amount);
    }
}
