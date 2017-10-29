using UnityEngine;
using System;
using System.IO;
using System.Text;

namespace SciFi.Util {
    public sealed class PlayDataLogger : IStateChangeListener, IDisposable {

        TextWriter outs;

        public PlayDataLogger(string filename) {
            outs = new StreamWriter(filename);
        }

        private PlayDataLogger() {}

        public static PlayDataLogger ToDebugLog() {
            var logger = new PlayDataLogger();
            logger.outs = new DebugLogAdapter();
            return logger;
        }

        public void Dispose() {
            outs.Dispose();
        }

        void WriteLine(string line) {
            outs.Write("{0:0.00}: ", Time.time);
            outs.WriteLine(line);
        }

        void WriteLine(string format, object extra) {
            outs.Write("{0:0.00}: ", Time.time);
            outs.WriteLine(format, extra);
        }

        void WriteLine(string format, params object[] extra) {
            outs.Write("{0:0.00}: ", Time.time);
            outs.WriteLine(format, extra);
        }

        public void GameStarted() {
            WriteLine("GameStarted");
        }

        public void GameEnded() {
            WriteLine("GameEnded");
        }

        public void ControlStateChanged(int control, bool active) {
            WriteLine("Control {0} = {1}", control, active);
        }

        public void ControlStateChanged(int control, float amount) {
            WriteLine("Control {0} = {1}", control, amount);
        }

        public void PlayerPositionChanged(int playerId, Vector2 position) {
            WriteLine("Player {0} = ({1:0.0##}, {2:0.0##})", playerId, position.x, position.y);
        }

        public void DamageChanged(int playerId, int newDamage) {
            WriteLine("Damage {0} = {1}", playerId, newDamage);
        }

        public void LifeChanged(int playerId, int newLives) {
            WriteLine("Lives {0} = {1}", playerId, newLives);
        }

        public void ObjectCreated(GameObject obj) {
            WriteLine("NewObject {0}", obj.name);
        }

        public void ObjectWillBeDestroyed(GameObject obj) {
            WriteLine("ObjectDestroyed {0}", obj.name);
        }
    }

    public class DebugLogAdapter : TextWriter {
        StringBuilder currentLine;

        public DebugLogAdapter() {
            currentLine = new StringBuilder();
        }

        private void ClearLine() {
            currentLine.Length = 0;
        }

        public override System.Text.Encoding Encoding => Encoding.UTF8;

        public override void Write(char c) {
            if (c == '\n') {
                Debug.Log(currentLine.ToString());
                ClearLine();
            } else {
                currentLine.Append(c);
            }
        }

        public override void Write(string s) {
            currentLine.Append(s);
        }

        public override void WriteLine(string s) {
            if (currentLine.Length > 0) {
                s = currentLine.ToString() + s;
                ClearLine();
            }
            Debug.Log(s);
        }
    }
}