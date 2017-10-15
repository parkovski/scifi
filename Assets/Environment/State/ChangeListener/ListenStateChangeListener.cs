using UnityEngine;
using UnityEngine.Networking;

using SciFi.Network;
using MessageType = SciFi.BroadcastStateChangeListener.MessageType;

namespace SciFi {
    public class ListenStateChangeListener {
        IStateChangeListener listener;

        public ListenStateChangeListener(IStateChangeListener listener) {
            this.listener = listener;
            NetworkController.clientConnectionToServer.RegisterHandler(NetworkMessages.StateChangeBroadcast, OnStateChanged);
        }

        void OnStateChanged(NetworkMessage msg) {
            var type = (MessageType)msg.reader.ReadByte();
            switch (type) {
            case MessageType.GameStarted:
                listener.GameStarted();
                break;
            case MessageType.GameEnded:
                listener.GameEnded();
                break;
            case MessageType.BoolControlStateChanged: {
                uint control = msg.reader.ReadUInt32();
                bool active = (control & (1 << 16)) != 0;
                control &= 0xFFFEFFFF;
                listener.ControlStateChanged((int)control, active);
                break;
            }
            case MessageType.AxisControlStateChanged: {
                int control = msg.reader.ReadInt32();
                float axis = msg.reader.ReadSingle();
                listener.ControlStateChanged(control, axis);
                break;
            }
            case MessageType.PlayerPositionChanged: {
                int player = msg.reader.ReadByte();
                Vector2 position = msg.reader.ReadVector2();
                listener.PlayerPositionChanged(player, position);
                break;
            }
            case MessageType.DamageChanged: {
                int player = msg.reader.ReadInt32();
                int damage = msg.reader.ReadInt32();
                listener.DamageChanged(player, damage);
                break;
            }
            case MessageType.LifeChanged: {
                int player = msg.reader.ReadInt32();
                int lives = msg.reader.ReadInt32();
                listener.LifeChanged(player, lives);
                break;
            }
            case MessageType.ObjectCreated: {
                NetworkInstanceId id = msg.reader.ReadNetworkId();
                listener.ObjectCreated(ClientScene.FindLocalObject(id));
                break;
            }
            case MessageType.ObjectWillBeDestroyed: {
                NetworkInstanceId id = msg.reader.ReadNetworkId();
                listener.ObjectWillBeDestroyed(ClientScene.FindLocalObject(id));
                break;
            }
            }
        }
    }
}