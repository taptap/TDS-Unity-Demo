using System.IO;
using UnityEditor;
# if UNITY_IOS
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
#endif
using UnityEngine;

namespace TapTap.Common.Editor
{
# if UNITY_IOS
    public static class TapCommonIOSProcessorFixer
    {
        // 娣诲姞鏍囩锛寀nity瀵煎嚭宸ョ▼鍚庤嚜鍔ㄦ墽琛岃鍑芥暟
        [PostProcessBuild(99)]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
        {
            if (buildTarget != BuildTarget.iOS) return;

            // 鑾峰緱宸ョ▼璺緞
            var projPath = TapCommonCompile.GetProjPath(path);
            var proj = TapCommonCompile.ParseProjPath(projPath);
            var unityFrameworkTarget = TapCommonCompile.GetUnityFrameworkTarget(proj);
            
            proj.AddFileToBuild(unityFrameworkTarget,
                proj.AddFile("usr/lib/libsqlite3.tbd", "libsqlite3.tbd", PBXSourceTree.Sdk));
        }
    }
#endif
}