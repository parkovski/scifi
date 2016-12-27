public class Bomb : Item {
    void Start() {
        BaseStart();

        Destroy(gameObject, 10f);
    }

    void Update() {
        BaseUpdate();
    }
}