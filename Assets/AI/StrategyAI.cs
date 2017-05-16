using UnityEngine;

using SciFi.AI.Strategies;
using SciFi.Players;

namespace SciFi.AI {
    public class StrategyAI : AIBase {
        const float evaluateStrategyInteval = .25f;
        float evaluateNextStrategyTime;

        StrategyPicker<Strategy> moveStrategyPicker;
        Strategy moveStrategy;
        int lastMoveControl = Control.None;

        void Start() {
            var me = GetComponent<Player>();
            var opponent = GameController.Instance.GetPlayer(0);
            var ground = GameObject.Find("FinalDest");
            moveStrategyPicker = new StrategyPicker<Strategy>(new Strategy[] {
                new StandStillStrategy(),
                new StayOnStageStrategy(me, ground),
                new FoFFightStrategy(me, opponent),
                new FoFFlightStrategy(me, opponent),
            });
        }

        void Update() {
            if (Time.time > evaluateNextStrategyTime) {
                evaluateNextStrategyTime = Time.time + evaluateStrategyInteval;
                moveStrategy = moveStrategyPicker.Pick();
            }

            int moveControl = moveStrategy.Step();
            if (moveControl != lastMoveControl) {
                inputManager.Release(lastMoveControl);
                inputManager.Press(moveControl);
            }
            lastMoveControl = moveControl;
        }
    }
}