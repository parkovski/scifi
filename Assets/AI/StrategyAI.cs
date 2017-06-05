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

        struct StrategyInfo {
            public StrategyPicker strategyPicker;
            public Strategy strategy;
            public int lastControl;
        }

        StrategyInfo moveStrategyInfo;
        StrategyInfo attackStrategyInfo;
        StrategyInfo jumpStrategyInfo;
        StrategyInfo blockStrategyInfo;

        // Strategy params
        struct StrategyParams {
            public Player me;
            public Player opponent;
            public GameObject ground;
        }
        StrategyParams strategyParams;

        const int strategyListIndex = 0;

        void Start() {
            strategyParams = new StrategyParams {
                me = GetComponent<Player>(),
                opponent = GameController.Instance.GetPlayer(0),
                ground = GameObject.Find("FinalDest"),
            };

            var strategyList = GetStrategyList(strategyListIndex);

            moveStrategyInfo = new StrategyInfo {
                strategyPicker = new StrategyPicker(GetStrategiesOfType(strategyList, StrategyType.Movement)),
            };
            attackStrategyInfo = new StrategyInfo {
                strategyPicker = new StrategyPicker(GetStrategiesOfType(strategyList, StrategyType.Attack)),
            };
            jumpStrategyInfo = new StrategyInfo {
                strategyPicker = new StrategyPicker(GetStrategiesOfType(strategyList, StrategyType.Jump)),
            };
            blockStrategyInfo = new StrategyInfo {
                strategyPicker = new StrategyPicker(GetStrategiesOfType(strategyList, StrategyType.Block)),
            };
        }

        void Update() {
            if (Time.time > evaluateNextStrategyTime) {
                evaluateNextStrategyTime = Time.time + evaluateStrategyInteval;
                PickStrategy(ref moveStrategyInfo);
                PickStrategy(ref attackStrategyInfo);
                PickStrategy(ref jumpStrategyInfo);
                PickStrategy(ref blockStrategyInfo);
            }

            UseStrategy(ref moveStrategyInfo);
            UseStrategy(ref attackStrategyInfo);
            UseStrategy(ref jumpStrategyInfo);
            UseStrategy(ref blockStrategyInfo);
        }

        void PickStrategy(ref StrategyInfo info) {
            var newStrategy = info.strategyPicker.Pick();
            if (ReferenceEquals(newStrategy, info.strategy)) {
                return;
            }
            if (info.strategy != null) {
                info.strategy.OnDeactivate();
            }
            if (newStrategy != null) {
                newStrategy.OnActivate();
            }
            info.strategy = newStrategy;
        }

        void UseStrategy(ref StrategyInfo info) {
            if (info.strategy == null) {
                if (info.lastControl != Control.None) {
                    inputManager.Release(info.lastControl);
                    info.lastControl = Control.None;
                }
                return;
            }
            int control = info.strategy.GetControl();
            if (control != info.lastControl) {
                inputManager.Release(info.lastControl);
                inputManager.Press(control);
            }
            info.lastControl = control;
        }

        Strategy[] GetStrategyList(int list) {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Strategy)))
                .Where(t => t.GetCustomAttributes(false).Any(a => (a is StrategyListAttribute) && (((StrategyListAttribute)a).list == list)))
                .Select(t => InitializeStrategy(t))
                .ToArray();
        }

        static Strategy[] GetStrategiesOfType(Strategy[] list, StrategyType type) {
            return list
                .Where(t => t.GetType().GetCustomAttributes(false).Any(a => (a is StrategyTypeAttribute) && (((StrategyTypeAttribute)a).type == type)))
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