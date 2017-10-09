using System.Collections.Generic;
using System.IO;

namespace SciFi.Util {
    public interface IDataPointProvider {
        string tag { get; }
        float GetCurrentPoint();
    }

    public class DataLogger {
        List<IDataPointProvider> providers;
        ManualSampler timer;
        TextWriter writer;

        public DataLogger(float interval, TextWriter writer) {
            providers = new List<IDataPointProvider>();
            timer = new ManualSampler(interval, LogPoints);
            this.writer = writer;
        }

        public void Tick() {
            timer.Run();
        }

        private void LogPoints() {
            foreach (var p in providers) {
                writer.WriteLine(
                    "{{\"tag\":\"{0}\",\"value\":{1}}}",
                    p.tag,
                    p.GetCurrentPoint()
                );
            }
        }
    }
}