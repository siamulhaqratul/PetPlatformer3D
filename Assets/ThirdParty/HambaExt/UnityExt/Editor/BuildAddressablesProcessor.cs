#if HAS_ADDRESSABLE_UNITYEXT
using System.Collections;
/// <summary>
/// The script gives you choice to whether to build addressable bundles when clicking the build button.
/// For custom build script, call PreExport method yourself.
/// For cloud build, put BuildAddressablesProcessor.PreExport as PreExport command.
/// Discussion: https://forum.unity.com/threads/how-to-trigger-build-player-content-when-build-unity-project.689602/
/// </summary>
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace UnityExt
{
    class BuildAddressablesProcessor
    {
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            if (UConfig.EnableAddressableSupport)
            {
                BuildPlayerWindow.RegisterBuildPlayerHandler((options) =>
                {
                    AddressableAssetSettings.CleanPlayerContent();
                    AddressableAssetSettings.BuildPlayerContent();
                    BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(options);
                });
            }
        }
    }
}
#endif