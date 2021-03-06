using UnityEngine;
using UnityEngine.Networking;

using SciFi.Players.Attacks;
using SciFi.UI;
using SciFi.Util;
using SciFi.Util.Extensions;

namespace SciFi.Players {
    public class daVinci : Player {
        public GameObject boneArmPrefab;
        public GameObject flyingMachinePrefab;
        public GameObject paintbrushPrefab;
        public GameObject paintDropPrefab;
        int paintDropPrefabIndex;
        GameObject boneArm;
        GameObject paintbrush;

        private CompoundSpriteFlip playerFlip;
        private CompoundSpriteFlip boneArmFlip;

        protected override void OnInitialize() {
            boneArm = Instantiate(boneArmPrefab, transform.position + GetBoneArmOffset(defaultDirection), Quaternion.identity);
            paintbrush = Instantiate(paintbrushPrefab, transform.position + GetPaintbrushOffset(defaultDirection), Quaternion.identity);
            paintDropPrefabIndex = GameController.PrefabToIndex(paintDropPrefab);

            eAttacks[0] = new NetworkAttack(new PaintbrushAttack(this, paintbrush.GetComponent<Paintbrush>()));
            eAttacks[1] = new NetworkAttack(new BoneArmAttack(this, boneArm.GetComponent<BoneArm>()));
            eAttacks[2] = new FlyingMachineAttack(this);

            playerFlip = new CompoundSpriteFlip(gameObject, defaultDirection);
            boneArmFlip = new CompoundSpriteFlip(boneArm, defaultDirection.Opposite());
            boneArmFlip.Flip(defaultDirection);
        }

        new void Update() {
            base.Update();
            if (boneArm == null || paintbrush == null) {
                return;
            }
            boneArm.transform.position = transform.position + GetBoneArmOffset(eDirection);
            paintbrush.transform.position = transform.position + GetPaintbrushOffset(eDirection);
        }

        Vector3 GetBoneArmOffset(Direction direction) {
            return new Vector3(.7f, .2f).FlipDirection(direction);
        }

        Vector3 GetPaintbrushOffset(Direction direction) {
            return new Vector3(.35f, .3f).FlipDirection(direction);
        }

        void FixedUpdate() {
            BaseInput();
        }

        void OnCollisionEnter2D(Collision2D collision) {
            BaseCollisionEnter2D(collision);
        }

        void OnCollisionExit2D(Collision2D collision) {
            BaseCollisionExit2D(collision);
        }

        protected override void OnChangeDirection() {
            playerFlip.Flip(eDirection);
            boneArmFlip.Flip(eDirection);
        }

        [Command]
        public void CmdSpawnFlyingMachine(float chargeTime) {
            var fmObj = Object.Instantiate(flyingMachinePrefab, transform.position, Quaternion.identity);
            var fm = fmObj.GetComponent<FlyingMachine>();
            fm.power = Mathf.Clamp((int)(chargeTime * 7.5f), 1, 10);
            fm.dx = 1.5f.FlipDirection(eDirection);
            NetworkServer.Spawn(fmObj);
            fm.Enable(netId, GetItemNetId(), false);
        }

        [Command]
        public void CmdSpawnPaintDrops(Color color) {
            var n = Random.Range(2, 5);
            var baseVelocity = new Vector2(7f, 2.5f).FlipDirection(eDirection);
            var positionOffset = new Vector3(.4f, .2f, 0f).FlipDirection(eDirection);
            var sharedHitSet = new HitSet();
            for (int i = 0; i < n; i++) {
                var scale = Random.Range(PaintDrop.minScale, PaintDrop.maxScale);
                var velocity = baseVelocity + new Vector2(Random.Range(-2f, 2f), Random.Range(-1.5f, 1.5f));
                var paintDrop = SpawnPooledProjectileScaled(
                    paintDropPrefabIndex,
                    paintbrush.transform.position + positionOffset,
                    new Vector3(scale, scale, 1),
                    Quaternion.identity,
                    velocity,
                    0,
                    false
                );
                var pd = paintDrop.GetComponent<PaintDrop>();
                pd.SetColor(color);
                pd.sSharedHitSet = sharedHitSet;
            }
        }
    }
}