using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

using SciFi.Network;

namespace SciFi {
    public class BroadcastStateChangeListener : IStateChangeListener {
        public enum MessageType {
            GameStarted,
            GameEnded,
            BoolControlStateChanged,
            AxisControlStateChanged,
            PlayerPositionChanged,
            DamageChanged,
            LifeChanged,
            ObjectCreated,
            ObjectWillBeDestroyed,
        }

        NetworkWriter writer;
        List<NetworkConnection> connections;
        uint messageTypeMask;

        public BroadcastStateChangeListener() {
            writer = new NetworkWriter();
            connections = new List<NetworkConnection>();
        }

        public void ToggleMessage(MessageType type, bool active) {
            if (active) {
                messageTypeMask |= (1u << (int)type);
            } else {
                messageTypeMask &= ~(1u << (int)type);
            }
        }

        public void ToggleAllMessages(bool active) {
            if (active) {
                messageTypeMask = 0xFFFFFFFFu;
            } else {
                messageTypeMask = 0;
            }
        }

        public bool MessageActive(MessageType type) {
            return (messageTypeMask & (1u << (int)type)) != 0;
        }

        public void AddConnection(NetworkConnection conn) {
            connections.Add(conn);
        }

        void SendWriter() {
            for (var i = 0; i < connections.Count; i++) {
                connections[i].SendWriter(writer, 0);
            }
        }

        void Send(MessageType type) {
            if (!MessageActive(type)) {
                return;
            }
            writer.StartMessage(NetworkMessages.StateChangeBroadcast);
            writer.Write((byte)type);
            writer.FinishMessage();
            SendWriter();
        }

        void Send(MessageType type, int playerId, int param) {
            if (!MessageActive(type)) {
                return;
            }
            Debug.Assert(playerId >= 0 && playerId < 0xFF && param >= 0 && param < 0xFF);
            writer.StartMessage(NetworkMessages.StateChangeBroadcast);
            writer.Write((byte)type);
            writer.Write((byte)playerId);
            writer.Write((byte)param);
            writer.FinishMessage();
            SendWriter();
        }

        void Send(MessageType type, GameObject obj) {
            if (!MessageActive(type)) {
                return;
            }
            if (obj.GetComponent<NetworkBehaviour>() == null) {
                Debug.LogWarning("Cannot send message for objects without netId: " + obj.name);
                return;
            }
            writer.StartMessage(NetworkMessages.StateChangeBroadcast);
            writer.Write((byte)type);
            writer.Write(obj.GetComponent<NetworkBehaviour>().netId);
            writer.FinishMessage();
            SendWriter();
        }

        public void GameStarted() {
            Send(MessageType.GameStarted);
        }

        public void GameEnded() {
            Send(MessageType.GameEnded);
        }

        public void ControlStateChanged(int control, bool active) {
            if (!MessageActive(MessageType.BoolControlStateChanged)) {
                return;
            }
            writer.StartMessage(NetworkMessages.StateChangeBroadcast);
            writer.Write((byte)MessageType.BoolControlStateChanged);
            writer.Write(control | (active ? (1 << 16) : 0));
            writer.FinishMessage();
        }

        public void ControlStateChanged(int control, float amount) {
            if (!MessageActive(MessageType.AxisControlStateChanged)) {
                return;
            }
            writer.StartMessage(NetworkMessages.StateChangeBroadcast);
            writer.Write((byte)MessageType.AxisControlStateChanged);
            writer.Write(control);
            writer.Write(amount);
            writer.FinishMessage();
        }

        public void PlayerPositionChanged(int playerId, Vector2 position) {
            if (!MessageActive(MessageType.PlayerPositionChanged)) {
                return;
            }
            writer.StartMessage(NetworkMessages.StateChangeBroadcast);
            writer.Write((byte)MessageType.PlayerPositionChanged);
            writer.Write((byte)playerId);
            writer.Write(position);
            writer.FinishMessage();
        }

        public void DamageChanged(int playerId, int newDamage) {
            Send(MessageType.DamageChanged, playerId, newDamage);
        }

        public void LifeChanged(int playerId, int newLives) {
            Send(MessageType.LifeChanged, playerId, newLives);
        }

        public void ObjectCreated(GameObject obj) {
            Send(MessageType.ObjectCreated, obj);
        }

        public void ObjectWillBeDestroyed(GameObject obj) {
            Send(MessageType.ObjectWillBeDestroyed, obj);
        }
    }
}