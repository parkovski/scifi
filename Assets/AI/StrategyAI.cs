using UnityEngine;
using System;
using System.Reflection;
using System.Linq;

using SciFi.AI.Strategies;
using SciFi.Players;

namespace SciFi.AI {
    public class StrategyAI : AIBase {
        const float evaluateStrategyInteval = .25f;
        float evaluateNextStrategyTime;

        StrategyPicker<Strategy> moveStrategyPicker;
        Strategy moveStrategy;
        int lastMoveControl = Control.None;

        // Strategy params
        struct StrategyParams {
            public Player me;
            public Player opponent;
            public GameObject ground;
        }
        StrategyParams strategyParams;

        void Start() {
            strategyParams = new StrategyParams {
                me = GetComponent<Player>(),
                opponent = GameController.Instance.GetPlayer(0),
                ground = GameObject.Find("FinalDest"),
            };

            moveStrategyPicker = new StrategyPicker<Strategy>(GetStrategiesOfType(0, StrategyType.Movement));
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

        Strategy[] GetStrategiesOfType(int list, StrategyType type) {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Strategy)))
                .Where(t => {
                    var attrs = t.GetCustomAttributes(false);
                    return attrs.Any(a => (a is StrategyListAttribute) && (((StrategyListAttribute)a).list == list))
                        && attrs.Any(a => (a is StrategyTypeAttribute) && (((StrategyTypeAttribute)a).type == type));
                })
                .Select(t => InitializeStrategy(t))
                .ToArray();
        }

        Strategy InitializeStrategy(Type type) {
            object[] paramValues;
            var constructor = type.GetConstructors().FirstOrDefault(c =>
                c.GetParameters().All(p =>
                    p.GetCustomAttributes(false).Any(a => a is StrategyParamAttribute)
                )
            );
            if (constructor == null) {
                constructor = type.GetConstructor(Type.EmptyTypes);
                paramValues = null;
            } else {
                paramValues = constructor.GetParameters()
                    .Select(p => (StrategyParamAttribute)p.GetCustomAttributes(false).Single(a => a is StrategyParamAttribute))
                    .Select(a => {
                        object val;
                        switch (a.type) {
                        case StrategyParamType.Me:
                            val = strategyParams.me;
                            break;
                        case StrategyParamType.Opponent:
                            val = strategyParams.opponent;
                            break;
                        case StrategyParamType.Ground:
                            val = strategyParams.ground;
                            break;
                        default:
                            val = null;
                            break;
                        }
                        return val;
                    })
                    .ToArray();
            }

            return (Strategy)constructor.Invoke(paramValues);
        }
    }
}