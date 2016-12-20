using UnityEngine;

public interface IPlayer {
    int Id { get; set; }
    string Name { get; set; }
    int Lives { get; set; }
    int Damage { get; set; }
    GameController GameController { get; set; }
    void TakeDamage(int amount);
}

class EmptyPlayer : IPlayer {
    public int Id { get; set; }
    public string Name { get; set; }
    public int Lives { get; set; }
    public int Damage { get; set; }
    public GameController GameController { get; set; }
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

    public int Id {
        get {
            return playerDelegate.Id;
        }
        set {
            playerDelegate.Id = value;
        }
    }

    public string Name {
        get {
            return playerDelegate.Name;
        }
        set {
            playerDelegate.Name = value;
        }
    }

    public int Lives {
        get {
            return playerDelegate.Lives;
        }
        set {
            playerDelegate.Lives = value;
        }
    }

    public int Damage {
        get {
            return playerDelegate.Damage;
        }
        set {
            playerDelegate.Damage = value;
        }
    }

    public GameController GameController {
        get {
            return playerDelegate.GameController;
        }
        set {
            playerDelegate.GameController = value;
        }
    }

    public void TakeDamage(int amount) {
        playerDelegate.TakeDamage(amount);
    }
}
