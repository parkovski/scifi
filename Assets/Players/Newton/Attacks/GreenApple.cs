namespace SciFi.Items {
    public class GreenApple : Projectile {
        void Start() {
            Destroy(gameObject, 2f);
        }
    }
}