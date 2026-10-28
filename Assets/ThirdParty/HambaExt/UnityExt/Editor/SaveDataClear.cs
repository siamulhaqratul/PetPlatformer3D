using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityExt.Debugging;

namespace UnityExt
{
    public static class SaveDataClear
    {
        [MenuItem(itemName: "Tools/UnityExt/Clear Player Storage Save Data", isValidateFunction: false, priority: 1)]
        static void ClearSaveData()
        {
            var shouldDelete = EditorUtility.DisplayDialog("!Warning!", "Are you sure to delete player storage and PlayerPref save data?", "ok", "cancel");
            if (shouldDelete)
            {
                PlayerPrefs.DeleteAll();
                string saveFile = "";
                long saveSize = 0;
                UnityExt.DataSaving.StorageManEditorExternalCall.GetCurrentSaveDetails(ref saveFile, ref saveSize);
                GLog.Print("pre clear - save file: " + saveFile + " and size: " + saveSize + " KB");

                UnityExt.DataSaving.StorageManEditorExternalCall.Clear();
                PlayerPrefs.Save();
                GLog.Print("cleared all player storage save data!");

                UnityExt.DataSaving.StorageManEditorExternalCall.GetCurrentSaveDetails(ref saveFile, ref saveSize);
                GLog.Print("post clear - save file: " + saveFile + " and size: " + saveSize + " KB");
            }
        }
    }
}