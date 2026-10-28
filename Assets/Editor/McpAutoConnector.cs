using UnityEditor;
using MCPForUnity.Editor.Services;

namespace PetPlatformer.Editor
{
    [InitializeOnLoad]
    public static class McpAutoConnector
    {
        static McpAutoConnector()
        {
            EditorPrefs.SetBool("MCPForUnity.AutoStartOnLoad", true);
            EditorApplication.delayCall += () =>
            {
                try
                {
                    var bridge = new BridgeControlService();
                    if (!bridge.IsRunning)
                    {
                        UnityEngine.Debug.Log("[McpAutoConnector] Auto-starting MCP bridge...");
                        _ = bridge.StartAsync();
                    }
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogWarning("[McpAutoConnector] Failed to start MCP bridge: " + ex.Message);
                }
            };
        }
    }
}
