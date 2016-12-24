public class Bomb : Item {
    void Start() {
        Destroy(gameObject, 10f);
    }
}