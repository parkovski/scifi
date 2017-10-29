using SciFi.Players.Attacks;

namespace SciFi {
    public interface IInteractable {
        void Interact(IAttackSource attack);
    }
}