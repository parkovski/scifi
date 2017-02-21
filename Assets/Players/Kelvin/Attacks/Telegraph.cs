using UnityEngine;

using SciFi.Items;

namespace SciFi.Players.Attacks {
    public class Telegraph : MonoBehaviour, IAttack {
        public Sprite openTelegraph;
        public Sprite closedTelegraph;

        public Sprite[] electricitySprites;

        [HideInInspector]
        public GameObject spawnedBy;

        SpriteRenderer electricity;
        int electricitySpriteIndex = 0;
        float electricitySwapTime;
        const float frameTime = .1f;

        AudioSource audioSource;
        SpriteRenderer telegraph;
        Collider2D rightCollider;
        Collider2D leftCollider;
        string sentence = "ABSOLUTE ZERO";
        int charIndex;
        int pulseIndex;
        float nextStateTime;
        MorseCodeState morseCodeState;

        enum MorseCodeState {
            PlayingPulse,
            /// The pause in between pulses
            ShortPause,
            /// The pause in between words
            LongPause,
            Done,
        }

        void Awake() {
            var colliders = GetComponents<Collider2D>();
            rightCollider = colliders[0];
            leftCollider = colliders[1];
            leftCollider.enabled = false;
        }

        void Start() {
            Item.IgnoreCollisions(gameObject, spawnedBy);
            electricity = gameObject.transform.Find("Electricity").GetComponent<SpriteRenderer>();
            electricitySwapTime = Time.time + frameTime;
            audioSource = GetComponent<AudioSource>();
            telegraph = GetComponent<SpriteRenderer>();
            charIndex = 0;
            pulseIndex = 0;
            morseCodeState = MorseCodeState.ShortPause;
            nextStateTime = Time.time;
            UpdateMorseCode();
        }

        public void SetDirection(Direction direction) {
            leftCollider.enabled = direction == Direction.Left;
            rightCollider.enabled = direction == Direction.Right;
        }

        void UpdateMorseCode() {
            if (morseCodeState == MorseCodeState.Done) {
                return;
            }

            if (Time.time > nextStateTime) {
                switch (morseCodeState) {
                case MorseCodeState.PlayingPulse:
                    audioSource.Stop();
                    telegraph.sprite = openTelegraph;
                    if (++pulseIndex >= MorseCode.Pulses(sentence[charIndex])) {
                        pulseIndex = 0;
                        if (++charIndex >= sentence.Length) {
                            morseCodeState = MorseCodeState.Done;
                        } else {
                            if (sentence[charIndex] == ' ') {
                                morseCodeState = MorseCodeState.LongPause;
                                nextStateTime += .25f;
                            } else {
                                morseCodeState = MorseCodeState.ShortPause;
                                nextStateTime += .1f;
                            }
                        }
                    } else {
                        morseCodeState = MorseCodeState.ShortPause;
                        nextStateTime += .1f;
                    }
                    break;
                case MorseCodeState.LongPause:
                    ++charIndex;
                    goto case MorseCodeState.ShortPause;
                case MorseCodeState.ShortPause:
                    morseCodeState = MorseCodeState.PlayingPulse;
                    nextStateTime += MorseCode.IsDash(sentence[charIndex], pulseIndex) ? .25f : .1f;
                    audioSource.time = 0;
                    audioSource.Play();
                    telegraph.sprite = closedTelegraph;
                    break;
                }
            }
        }

        void Update() {
            if (Time.time > electricitySwapTime) {
                electricitySwapTime = Time.time + frameTime;
                if (++electricitySpriteIndex >= electricitySprites.Length) {
                    electricitySpriteIndex = 0;
                }
                electricity.sprite = electricitySprites[electricitySpriteIndex];
            }
            UpdateMorseCode();
        }

        void OnTriggerEnter2D(Collider2D collider) {
            if (Attack.GetAttackHit(collider.gameObject.layer) == AttackHit.HitAndDamage) {
                GameController.Instance.Hit(collider.gameObject, this, gameObject, 5, 0.1f);
            }
        }

        public AttackType Type { get { return AttackType.Melee; } }
        public AttackProperty Properties { get { return AttackProperty.Electric; } }
    }
}