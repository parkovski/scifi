using UnityEngine;

namespace SciFi.Players.Attacks {
    public class CalcBookAttack : Attack {
        GameObject[] books;
        GameObject chargingBook;
        int power;

        const float timeToChangeBooks = 0.7f;

        public CalcBookAttack(Player player, GameObject[] books)
            : base(player, true)
        {
            this.books = books;
        }

        void SpawnChargingBook(GameObject book) {
            var offset = player.eDirection == Direction.Left
                ? new Vector3(-1f, .5f)
                : new Vector3(1f, .5f);
            var rotation = player.eDirection == Direction.Left
                ? Quaternion.Euler(0f, 0f, -20f)
                : Quaternion.Euler(0f, 0f, 20f);

            chargingBook = Object.Instantiate(
                book,
                player.gameObject.transform.position + offset,
                rotation,
                player.gameObject.transform
            );
            var behavior = chargingBook.GetComponent<CalcBook>();
            behavior.spawnedBy = player.gameObject;
        }

        public override void OnBeginCharging(Direction direction) {
            power = 0;
            SpawnChargingBook(books[0]);
        }

        public override void OnKeepCharging(float chargeTime, Direction direction) {
            if (chargeTime > timeToChangeBooks && power == 0) {
                ++power;
                Object.Destroy(chargingBook);
                SpawnChargingBook(books[1]);
            } else if (chargeTime > 2*timeToChangeBooks && power == 1) {
                ++power;
                Object.Destroy(chargingBook);
                SpawnChargingBook(books[2]);
            }
        }

        public override void OnEndCharging(float chargeTime, Direction direction) {
            chargingBook.GetComponent<CalcBook>().attacking = true;
            if (direction == Direction.Left) {
                chargingBook.GetComponent<Animator>().SetTrigger("SwingBackwards");
            } else {
                chargingBook.GetComponent<Animator>().SetTrigger("Swing");
            }
        }
    }
}