using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace Editor
{
    public static class BuildPostProcessor
    {
        [PostProcessBuild]
        public static void ChangeXcodePlist(BuildTarget buildTarget, string path)
        {
#if UNITY_IPHONE
            if (buildTarget != BuildTarget.iOS) return;

            var plistPath = path + "/Info.plist";
            var plist = new PlistDocument();

            plist.ReadFromFile(plistPath);

            var rootDict = plist.root;

            Debug.Log(">> Automation, plist ... <<");

            // example of changing a value:
            // rootDict.SetString("CFBundleVersion", "6.6.6");

            // example of adding a boolean key...
            // < key > ITSAppUsesNonExemptEncryption </ key > < false />
            rootDict.SetString("NSPhotoLibraryUsageDescription", "Enable to read/write on your device");
            rootDict.SetString("NSPhotoLibraryAddUsageDescription", "Enable to save MP4 on your device");

            File.WriteAllText(plistPath, plist.WriteToString());
#endif
        }

        /* [PostProcessBuildAttribute(1)]
        public static void OnPostProcessBuild(BuildTarget target, string path)
        {
            if (target == BuildTarget.iOS)
            {
                var project = new PBXProject();
                var sPath = PBXProject.GetPBXProjectPath(path);
                project.ReadFromFile(sPath);

                var g = project.GetUnityFrameworkTargetGuid();

                ModifyFrameworksSettings(project, g);

                // modify frameworks and settings as desired
                File.WriteAllText(sPath, project.WriteToString());
            }
        }

        private static void ModifyFrameworksSettings(PBXProject project, string g)
        {
            // add hella frameworks

            Debug.Log(">> Automation, Frameworks... <<");

            project.AddFrameworkToProject(g, "blah.framework", false);
            project.AddFrameworkToProject(g, "libz.tbd", false);

            // go insane with build settings

            Debug.Log(">> Automation, Settings... <<");

            project.AddBuildProperty(g,
                "LIBRARY_SEARCH_PATHS",
                "../blahblah/lib");

            project.AddBuildProperty(g,
                "OTHER_LDFLAGS",
                "-lsblah -lbz2");

            // note that, due to some Apple shoddyness, you usually need to turn this off
            // to allow the project to ARCHIVE correctly (ie, when sending to testflight):
            project.AddBuildProperty(g,
                "ENABLE_BITCODE",
                "false");
        } */
    }
}