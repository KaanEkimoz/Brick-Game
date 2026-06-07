#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

// Ensures Info.plist has the App Tracking Transparency string required by LevelPlay/IronSource
// and any ad SDK on iOS 14.5+. Without it, App Store may reject the build.
public static class iOSBuildPostProcess
{
    [PostProcessBuild(999)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target != BuildTarget.iOS) return;

        string plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);
        PlistElementDict root = plist.root;

        // App Tracking Transparency — required when ads SDK is bundled
        root.SetString(
            "NSUserTrackingUsageDescription",
            "This identifier will be used to deliver personalized ads to you and limit how often you see the same ad."
        );

        // Optional but recommended for ad networks (SKAdNetwork already handled by LevelPlay postproc)
        // Mark the app as not exempt from US export encryption (uses only standard iOS encryption).
        root.SetBoolean("ITSAppUsesNonExemptEncryption", false);

        // Make sure plist allows arbitrary loads only if needed by ads; default policy stays strict.
        // LevelPlay already injects NSAppTransportSecurity exceptions; we don't override here.

        plist.WriteToFile(plistPath);
    }
}
#endif
