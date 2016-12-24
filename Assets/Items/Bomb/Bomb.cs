public class Bomb : Item {
    void Start() {
        Destroy(gameObject, 5f);
    }
}