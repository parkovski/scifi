using UnityEngine;
using System.Collections.Generic;

using SciFi.Items;

namespace SciFi.Players.Attacks {
    public class DynamiteFragment : Projectile {
        IEnumerable<GameObject> Children() {
            var i = 0;
            while (i < transform.childCount) {
                yield return transform.GetChild(i).gameObject;
                i++;
            }
        }

        void Start() {
            BaseStart();
            Destroy(gameObject, 1f);

            foreach (var child in Children()) {
                child.layer = Layers.projectiles;
                var rb = child.GetComponent<Rigidbody2D>();
                rb.AddForce(new Vector2(Random.Range(-200f, 200f), Random.Range(100f, 300f)));
            }
        }

        public void ChildCollide(GameObject child, Collision2D collision) {
            if (Attack.GetAttackHit(collision.gameObject.layer) == AttackHit.HitAndDamage) {
                child.layer = Layers.displayOnly;
                child.GetComponent<SpriteRenderer>().enabled = false;
                GameController.Instance.Hit(collision.gameObject, this, child, 8, 3f);
            }
        }

        public override AttackProperty Properties { get { return AttackProperty.Explosive; } }
    }
}