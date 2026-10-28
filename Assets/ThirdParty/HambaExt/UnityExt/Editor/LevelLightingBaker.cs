using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

namespace UnityExt
{
    public class LevelLightingBaker : EditorWindow
    {
        static LevelLightingBaker window;

        [MenuItem("Hamba/Bake All Scene's Lights")]
        static void Init()
        {
            window = (LevelLightingBaker)EditorWindow.GetWindow(typeof(LevelLightingBaker));
        }

        void OnGUI()
        {
            if (GUILayout.Button(System.Environment.NewLine + "Bake Lights" + System.Environment.NewLine))
            {
                string path = EditorUtility.OpenFolderPanel("Load scenes", "Assets", "");
                if (string.IsNullOrEmpty(path) == false && string.IsNullOrWhiteSpace(path) == false)
                {
                    string[] files = Directory.GetFiles(path);
                    var levelPaths = new List<string>();
                    foreach (string file in files)
                    {
                        if (file.EndsWith(".unity"))
                        {
                            levelPaths.Add(AssetsRelativePath(file));
                        }
                    }
                    Lightmapping.BakeMultipleScenes(levelPaths.ToArray());
                }
            }
        }

        public static string AssetsRelativePath(string absolutePath)
        {
            if (absolutePath.StartsWith(Application.dataPath))
            {
                return "Assets" + absolutePath.Substring(Application.dataPath.Length);
            }
            else
            {
                throw new System.ArgumentException("Full path does not contain the current project's Assets folder", "absolutePath");
            }
        }
    }
}