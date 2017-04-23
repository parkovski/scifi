using UnityEngine;
using SciFi.Util.Extensions;

namespace SciFi.Players.Attacks {
    public class CalcBookAttack : Attack {
        GameObject[] books;
        int activeBookIndex;
        int power;

        const float timeToChangeBooks = 0.5f;

        public CalcBookAttack(Player player, GameObject[] books)
            : base(player, true)
        {
            this.books = books;
            activeBookIndex = -1;
            foreach (var book in books) {
                ShowBook(book, false);
            }
        }

        void StartCharging(int index) {
            float animationTime = 0;
            GameObject activeBook;
            if (activeBookIndex != -1) {
                activeBook = books[activeBookIndex];
                animationTime = activeBook.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime;
                ShowBook(activeBook, false);
            }
            activeBook = books[index];
            var chargeAnim = player.eDirection == Direction.Right ? "CalcBookCharge" : "CalcBookChargeBackwards";
            ShowBook(activeBook, true);
            activeBookIndex = index;
            activeBook.GetComponent<Animator>().Play(chargeAnim, 0, animationTime);
        }

        void ShowBook(GameObject book, bool show) {
            book.GetComponent<CalcBook>().Show(show);
        }

        public override void OnBeginCharging(Direction direction) {
            power = 0;
            StartCharging(0);
        }

        public override void OnKeepCharging(float chargeTime, Direction direction) {
            if (chargeTime > timeToChangeBooks && power == 0) {
                ++power;
                StartCharging(1);
            } else if (chargeTime > 2*timeToChangeBooks && power == 1) {
                ++power;
                StartCharging(2);
            }
        }

        public override void OnEndCharging(float chargeTime, Direction direction) {
            books[activeBookIndex].GetComponent<CalcBook>().StartAttacking();
            if (direction == Direction.Left) {
                books[activeBookIndex].GetComponent<Animator>().SetTrigger("SwingBackwards");
            } else {
                books[activeBookIndex].GetComponent<Animator>().SetTrigger("Swing");
            }
        }

        public override void OnCancel() {
            ShowBook(books[activeBookIndex], false);
            activeBookIndex = -1;
        }
    }
}