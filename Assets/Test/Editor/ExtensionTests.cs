using NUnit.Framework;

using SciFi.Util.Extensions;

namespace SciFi.Test {
    public class ExtensionTests {
        [Test]
        public void ScaleTest() {
            Assert.AreEqual(5.Scale(0, 10, 0, 100), 50);
            Assert.AreEqual(10.Scale(5, 15, 45, 47), 46);

            Assert.AreEqual(1.1f.Scale(1, 2, 13, 23), 14, float.Epsilon);
        }
    }
}