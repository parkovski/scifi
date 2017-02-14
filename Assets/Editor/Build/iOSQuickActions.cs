using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace SciFi.Editor.Build {
    public class iOSQuickActions {
        [PostProcessBuild]
        public static void AddQuickActionsToPlist(BuildTarget buildTarget, string pathToBuiltProject) {
            if (buildTarget != BuildTarget.iOS) {
                return;
            }

            string plistPath = pathToBuiltProject + "/Info.plist";
            var plist = new PlistDocument();
            plist.ReadFromFile(plistPath);

            var rootDict = plist.root;

            var quickActions = rootDict.CreateArray("UIApplicationShortcutItems");
            var singlePlayer = quickActions.AddDict();
            singlePlayer.SetString("UIApplicationShortcutItemIconType", "UIApplicationShortcutIconTypePlay");
            singlePlayer.SetString("UIApplicationShortcutItemTitle", "Single Player");
            singlePlayer.SetString("UIApplicationShortcutItemType", "Singleplayer");

            var multiPlayer = quickActions.AddDict();
            multiPlayer.SetString("UIApplicationShortcutItemIconType", "UIApplicationShortcutIconTypeContact");
            multiPlayer.SetString("UIApplicationShortcutItemTitle", "Online Game");
            multiPlayer.SetString("UIApplicationShortcutItemType", "Multiplayer");

            plist.WriteToFile(plistPath);
        }
    }
}