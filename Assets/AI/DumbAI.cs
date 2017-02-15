using UnityEngine;
using System.Collections;

namespace SciFi.AI {
    public class DumbAI : AIBase {
        bool movingLeft = false;
        bool movingRight = false;

        float nextAttackTime;

        void Start() {
            nextAttackTime = Time.time + 3f;
        }

        void FixedUpdate() {
            if (movingLeft) {
                if (transform.position.x < -3f) {
                    movingLeft = false;
                    movingRight = true;
                    inputManager.Release(Control.Left);
                } else if (!inputManager.IsControlActive(Control.Left)) {
                    inputManager.Press(Control.Left);
                }
            } else if (movingRight) {
                if (transform.position.x > 3f) {
                    movingRight = false;
                    movingLeft = true;
                    inputManager.Release(Control.Right);
                } else if (!inputManager.IsControlActive(Control.Right)) {
                    inputManager.Press(Control.Right);
                }
            } else {
                if (transform.position.x < 0) {
                    movingRight = true;
                } else {
                    movingLeft = true;
                }
            }

            if (Time.time > nextAttackTime) {
                StartCoroutine(Attack());
                nextAttackTime = Time.time + Random.Range(0.5f, 3f);
            }
        }

        IEnumerator Attack() {
            int button;
            int random = Random.Range(0, 3);
            switch (random) {
            case 0:
                button = Control.Attack1;
                break;
            case 1:
                button = Control.Attack2;
                break;
            case 2:
                button = Control.Attack3;
                break;
            default:
                yield break;
            }

            inputManager.Press(button);
            yield return new WaitForSeconds(0.25f);
            inputManager.Release(button);
        }
    }
}