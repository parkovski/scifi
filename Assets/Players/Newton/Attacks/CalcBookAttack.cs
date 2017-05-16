using UnityEngine;
using System.Linq;

namespace SciFi.Players.Attacks {
    public class CalcBookAttack : Attack {
        CalcBook[] books;
        int activeBookIndex;
        int power;

        const float timeToChangeBooks = 0.5f;

        public CalcBookAttack(Player player, GameObject[] books)
            : base(player, true)
        {
            this.books = books.Select(b => b.GetComponent<CalcBook>()).ToArray();
            activeBookIndex = -1;
            for (int i = 0; i < books.Length; i++) {
                ShowBook(i, false);
            }
        }

        void StartCharging(int index) {
            float animationTime = 0;
            CalcBook activeBook;
            if (activeBookIndex != -1) {
                activeBook = books[activeBookIndex];
                animationTime = activeBook.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime;
                ShowBook(activeBookIndex, false);
            }
            activeBook = books[index];
            var chargeAnim = player.eDirection == Direction.Right ? "CalcBookCharge" : "CalcBookChargeBackwards";
            ShowBook(index, true);
            activeBookIndex = index;
            activeBook.GetComponent<Animator>().Play(chargeAnim, 0, animationTime);
        }

        void ShowBook(int index, bool show) {
            books[index].GetComponent<CalcBook>().Show(show);
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
            books[activeBookIndex].GetComponent<CalcBook>().Hide();
            activeBookIndex = -1;
        }
    }
}