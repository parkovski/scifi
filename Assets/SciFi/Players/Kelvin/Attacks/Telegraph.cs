using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

using SciFi.Items;
using SciFi.Players.Modifiers;

namespace SciFi.Players.Attacks {
    public class Telegraph : MonoBehaviour, IAttackSource {
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
        const float dotLength = 0.1f;
        const float dashLength = 3 * dotLength;
        const float pauseBetweenPulses = dotLength;
        const float pauseBetweenLetters = 3 * pauseBetweenPulses;
        const float pauseBetweenWords = 7 * pauseBetweenPulses;

        List<GameObject> attackingObjects;
        List<ModifierStateChange> attackingModifierStateChanges;
        float nextDamageTime;
        const float damageInterval = 0.25f;
        int hits;

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
            attackingObjects = new List<GameObject>();
            attackingModifierStateChanges = new List<ModifierStateChange>();
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

        void OnDestroy() {
            foreach (var stateChange in attackingModifierStateChanges) {
                if (stateChange == null) {
                    continue;
                }
                stateChange.End();
            }
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
                                nextStateTime += pauseBetweenWords;
                            } else {
                                morseCodeState = MorseCodeState.ShortPause;
                                nextStateTime += pauseBetweenLetters;
                            }
                        }
                    } else {
                        morseCodeState = MorseCodeState.ShortPause;
                        nextStateTime += pauseBetweenPulses;
                    }
                    break;
                case MorseCodeState.LongPause:
                    ++charIndex;
                    goto case MorseCodeState.ShortPause;
                case MorseCodeState.ShortPause:
                    morseCodeState = MorseCodeState.PlayingPulse;
                    nextStateTime += MorseCode.IsDash(sentence[charIndex], pulseIndex) ? dashLength : dotLength;
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

            if (NetworkServer.active && nextDamageTime > 0f) {
                if (Time.time > nextDamageTime && hits < 4) {
                    nextDamageTime = Time.time + damageInterval;
                    var knockback = 0f;
                    if (++hits == 4) {
                        knockback = 10f;
                        foreach (var stateChange in attackingModifierStateChanges) {
                            if (stateChange == null) {
                                continue;
                            }
                            stateChange.End();
                        }
                    }
                    foreach (var obj in attackingObjects) {
                        GameController.Instance.Hit(obj, this, gameObject, 5, knockback);
                    }
                }
            }
        }

        void OnTriggerEnter2D(Collider2D collider) {
            if (!NetworkServer.active) {
                return;
            }

            if (Attack.GetAttackHit(collider.gameObject.layer) == AttackHit.HitAndDamage) {
                attackingObjects.Add(collider.gameObject);
                var player = collider.gameObject.GetComponent<Player>();
                if (player != null) {
                    var stateChange = new ModifierStateChange(player, ModId.CantMove, () => gameObject == null);
                    stateChange.Start();
                    attackingModifierStateChanges.Add(stateChange);
                } else {
                    attackingModifierStateChanges.Add(null);
                }
                if (nextDamageTime == 0f) {
                    nextDamageTime = Time.time;
                }
            }
        }

        public AttackType Type { get { return AttackType.Melee; } }
        public AttackProperty Properties { get { return AttackProperty.Electric; } }
        public Player Owner { get { return spawnedBy.GetComponent<Player>(); } }
    }
}