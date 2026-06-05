#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace Ekimoz.Editor
{
    /// <summary>
    /// Patches the generated Xcode project's Info.plist with iOS-only keys that Unity's
    /// Player Settings UI does NOT expose in a portable way:
    ///
    ///   • NSUserTrackingUsageDescription — Apple-required ATT prompt string. Without it,
    ///     iOS 14.5+ refuses to even SHOW the tracking-permission dialog, so LevelPlay
    ///     can never request IDFA and every install collapses to limited-IDFA serve. App
    ///     Review also auto-rejects ad-monetised apps with no ATT description.
    ///
    ///   • ITSAppUsesNonExemptEncryption = NO — Skips the export-compliance questionnaire
    ///     in App Store Connect on every upload. We do not ship custom encryption, just
    ///     standard HTTPS, which qualifies for the exemption.
    ///
    /// Runs automatically after every iOS build. No manual Info.plist editing required.
    /// </summary>
    public static class iOSBuildPostProcessor
    {
        private const string AttDescription =
            "This identifier allows us to show more relevant ads and support the free version of Brick Game.";

        [PostProcessBuild(int.MaxValue)] // run last so our keys aren't overwritten by other post-processors
        public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target != BuildTarget.iOS) return;

            string plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
            if (!File.Exists(plistPath))
            {
                UnityEngine.Debug.LogWarning("[iOSBuildPostProcessor] Info.plist not found at " + plistPath);
                return;
            }

            var plist = new PlistDocument();
            plist.ReadFromFile(plistPath);
            var root = plist.root;

            root.SetString("NSUserTrackingUsageDescription", AttDescription);
            root.SetBoolean("ITSAppUsesNonExemptEncryption", false);

            // Belt-and-braces: status bar hidden, full-screen, portrait only (Unity usually
            // writes these from Player Settings, but enforce them so an accidental UI tweak
            // can't sneak a portrait-upside-down or landscape entry back in).
            root.SetBoolean("UIStatusBarHidden", true);
            root.SetBoolean("UIRequiresFullScreen", true);

            var orientations = root.CreateArray("UISupportedInterfaceOrientations");
            orientations.AddString("UIInterfaceOrientationPortrait");
            var orientationsiPad = root.CreateArray("UISupportedInterfaceOrientations~ipad");
            orientationsiPad.AddString("UIInterfaceOrientationPortrait");

            plist.WriteToFile(plistPath);
            UnityEngine.Debug.Log("[iOSBuildPostProcessor] Info.plist patched: ATT + exempt-encryption + portrait-only.");
        }
    }
}
#endif
