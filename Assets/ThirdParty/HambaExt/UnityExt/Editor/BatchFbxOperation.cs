using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityExt;
using System.Linq;
using System.Reflection;

namespace UnityExt 
{
    public class BatchFbxOperation : EditorWindow
    {
        [SerializeField] GameObject[] fbxModels;
        [SerializeField] bool processModel = false, processRig = false, processAnim = false, processMaterial = false;
        [SerializeField] Material material;
        #region Model Setting
        bool operateOnModelSetting = false;
        [SerializeField] float scaleFactor = 1.0f;
        [SerializeField]
        bool convertUnits = true, bakeAxisConversion = false, importBlendShapes = false, importVisibility = false,
            importCameras = false, importLights = false, preserveHierarchy = false, sortHierarchyByName = true;
        [SerializeField] ModelImporterMeshCompression meshCompression = ModelImporterMeshCompression.High;
        [SerializeField] bool meshReadWriteEnabled = false;
        [SerializeField] MeshOptimizationFlags optimizeMesh = MeshOptimizationFlags.Everything;
        [SerializeField] bool generateCollider = false, keepQuads = false, weldVertices = true;
        [SerializeField] ModelImporterIndexFormat indexFormat = ModelImporterIndexFormat.Auto;
        [SerializeField] ModelImporterNormals normals = ModelImporterNormals.Import;
        [SerializeField] ModelImporterNormalCalculationMode normalsMode = ModelImporterNormalCalculationMode.AreaAndAngleWeighted;
        [SerializeField] ModelImporterNormalSmoothingSource smoothnessSource = ModelImporterNormalSmoothingSource.PreferSmoothingGroups;
        [SerializeField] float smoothingAngle = 60f;
        [SerializeField] ModelImporterTangents tangents = ModelImporterTangents.None;
        [SerializeField] bool swapUV = false, generateLightmapUVs = false;
        SerializedProperty scaleFactor_prop, convertUnits_prop, bakeAxisConversion_prop, importBlendShapes_prop, importVisibility_prop,
            importCameras_prop, importLights_prop, preserveHierarchy_prop, sortHierarchyByName_prop, meshCompression_prop,
            meshReadWriteEnabled_prop, optimizeMesh_prop, generateCollider_prop, keepQuads_prop, weldVertices_prop, indexFormat_prop,
            normals_prop, normalsMode_prop, smoothnessSource_prop, smoothingAngle_prop, tangents_prop, swapUV_prop, generateLightmapUVs_prop;
        void InitModelProperties(BatchFbxOperation window)
        {
            window.scaleFactor_prop = window.serialObj.FindProperty(nameof(scaleFactor)); window.convertUnits_prop = window.serialObj.FindProperty(nameof(convertUnits));
            window.bakeAxisConversion_prop = window.serialObj.FindProperty(nameof(bakeAxisConversion)); window.importBlendShapes_prop = window.serialObj.FindProperty(nameof(importBlendShapes));
            window.importVisibility_prop = window.serialObj.FindProperty(nameof(importVisibility)); window.importCameras_prop = window.serialObj.FindProperty(nameof(importCameras));
            window.importLights_prop = window.serialObj.FindProperty(nameof(importLights)); window.preserveHierarchy_prop = window.serialObj.FindProperty(nameof(preserveHierarchy));
            window.sortHierarchyByName_prop = window.serialObj.FindProperty(nameof(sortHierarchyByName)); window.meshCompression_prop = window.serialObj.FindProperty(nameof(meshCompression));
            window.meshReadWriteEnabled_prop = window.serialObj.FindProperty(nameof(meshReadWriteEnabled)); window.optimizeMesh_prop = window.serialObj.FindProperty(nameof(optimizeMesh));
            window.generateCollider_prop = window.serialObj.FindProperty(nameof(generateCollider)); window.keepQuads_prop = window.serialObj.FindProperty(nameof(keepQuads));
            window.weldVertices_prop = window.serialObj.FindProperty(nameof(weldVertices)); window.indexFormat_prop = window.serialObj.FindProperty(nameof(indexFormat));
            window.normals_prop = window.serialObj.FindProperty(nameof(normals));
            window.normalsMode_prop = window.serialObj.FindProperty(nameof(normalsMode)); window.smoothnessSource_prop = window.serialObj.FindProperty(nameof(smoothnessSource));
            window.smoothingAngle_prop = window.serialObj.FindProperty(nameof(smoothingAngle)); window.tangents_prop = window.serialObj.FindProperty(nameof(tangents));
            window.swapUV_prop = window.serialObj.FindProperty(nameof(swapUV)); window.generateLightmapUVs_prop = window.serialObj.FindProperty(nameof(generateLightmapUVs));
        }
        void DrawModelSettings()
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(scaleFactor_prop, true); EditorGUILayout.PropertyField(convertUnits_prop, true); EditorGUILayout.PropertyField(bakeAxisConversion_prop, true);
            EditorGUILayout.PropertyField(importBlendShapes_prop, true); EditorGUILayout.PropertyField(importVisibility_prop, true); EditorGUILayout.PropertyField(importCameras_prop, true);
            EditorGUILayout.PropertyField(importLights_prop, true); EditorGUILayout.PropertyField(preserveHierarchy_prop, true); EditorGUILayout.PropertyField(sortHierarchyByName_prop, true);
            EditorGUILayout.PropertyField(meshCompression_prop, true); EditorGUILayout.PropertyField(meshReadWriteEnabled_prop, true); EditorGUILayout.PropertyField(optimizeMesh_prop, true);
            EditorGUILayout.PropertyField(generateCollider_prop, true); EditorGUILayout.PropertyField(keepQuads_prop, true); EditorGUILayout.PropertyField(weldVertices_prop, true);
            EditorGUILayout.PropertyField(indexFormat_prop, true); EditorGUILayout.PropertyField(normals_prop, true);
            EditorGUILayout.PropertyField(normalsMode_prop, true); EditorGUILayout.PropertyField(smoothnessSource_prop, true); EditorGUILayout.PropertyField(smoothingAngle_prop, true);
            EditorGUILayout.PropertyField(tangents_prop, true); EditorGUILayout.PropertyField(swapUV_prop, true); EditorGUILayout.PropertyField(generateLightmapUVs_prop, true);
            EditorGUI.indentLevel--;
        }
        void ApplyModelSettng(ModelImporter modelImporter)
        {
            modelImporter.globalScale = scaleFactor;
            modelImporter.useFileUnits = convertUnits;
            modelImporter.bakeAxisConversion = bakeAxisConversion;
            modelImporter.importBlendShapes = importBlendShapes;
            modelImporter.importVisibility = importVisibility;
            modelImporter.importCameras = importCameras;
            modelImporter.importLights = importLights;
            modelImporter.preserveHierarchy = preserveHierarchy;
            modelImporter.sortHierarchyByName = sortHierarchyByName;
            modelImporter.meshCompression = meshCompression;
            modelImporter.isReadable = meshReadWriteEnabled;
            modelImporter.meshOptimizationFlags = optimizeMesh;
            modelImporter.addCollider = generateCollider;
            modelImporter.keepQuads = keepQuads;
            modelImporter.weldVertices = weldVertices;
            modelImporter.indexFormat = indexFormat;
            modelImporter.importBlendShapeNormals = normals;
            modelImporter.importNormals = normals;
            modelImporter.normalCalculationMode = normalsMode;
            modelImporter.normalSmoothingSource = smoothnessSource;
            modelImporter.normalSmoothingAngle = smoothingAngle;
            modelImporter.importTangents = tangents;
            modelImporter.swapUVChannels = swapUV;
            modelImporter.generateSecondaryUV = generateLightmapUVs;
        }
        #endregion

        #region Rig Setting
        bool operateOnRigSetting = false;
        [SerializeField] ModelImporterAnimationType animationType = ModelImporterAnimationType.None;
        [SerializeField] bool stripBones = true;
        [SerializeField] ModelImporterGenerateAnimations generation = ModelImporterGenerateAnimations.None;
        [SerializeField] ModelImporterSkinWeights skinWeights = ModelImporterSkinWeights.Custom;
        [SerializeField] int maxBonesPerVert = 1;
        [SerializeField] float minBoneWeight = 0.001f;
        [SerializeField] ModelImporterAvatarSetup avatarDefinition = ModelImporterAvatarSetup.CreateFromThisModel;
        [SerializeField] string rootNode;
        [SerializeField] Avatar sourceAvatar;
        [SerializeField] bool optimizeGameobjects = true;
        [SerializeField] string[] extraExposedTransformPath = null;
        SerializedProperty animationType_prop, stripBones_prop, generation_prop, skinWeights_prop, maxBonesPerVert_prop,
            minBoneWeight_prop, avatarDefinition_prop, rootNode_prop, sourceAvatar_prop, optimizeGameobjects_prop, extraExposedTransformPath_prop;
        void InitRigProperties(BatchFbxOperation window)
        {
            window.animationType_prop = window.serialObj.FindProperty(nameof(animationType)); window.stripBones_prop = window.serialObj.FindProperty(nameof(stripBones));
            window.generation_prop = window.serialObj.FindProperty(nameof(generation)); window.skinWeights_prop = window.serialObj.FindProperty(nameof(skinWeights));
            window.maxBonesPerVert_prop = window.serialObj.FindProperty(nameof(maxBonesPerVert)); window.minBoneWeight_prop = window.serialObj.FindProperty(nameof(minBoneWeight));
            window.avatarDefinition_prop = window.serialObj.FindProperty(nameof(avatarDefinition)); window.rootNode_prop = window.serialObj.FindProperty(nameof(rootNode));
            window.sourceAvatar_prop = window.serialObj.FindProperty(nameof(sourceAvatar)); window.optimizeGameobjects_prop = window.serialObj.FindProperty(nameof(optimizeGameobjects));
            window.extraExposedTransformPath_prop = window.serialObj.FindProperty(nameof(extraExposedTransformPath));
        }
        void DrawRigSettings()
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(animationType_prop, true);
            if (animationType == ModelImporterAnimationType.Legacy)
            {
                EditorGUILayout.PropertyField(generation_prop, true);
                if (generation == ModelImporterGenerateAnimations.InOriginalRoots || generation == ModelImporterGenerateAnimations.InNodes
                    || generation == ModelImporterGenerateAnimations.InRoot)
                {
                    EditorGUILayout.HelpBox("Generation setting is deprecated!", MessageType.Warning);
                }
            }

            if (animationType == ModelImporterAnimationType.Human && avatarDefinition == ModelImporterAvatarSetup.NoAvatar)
            {
                avatarDefinition = ModelImporterAvatarSetup.CreateFromThisModel;
            }

            if (animationType == ModelImporterAnimationType.Generic && avatarDefinition == ModelImporterAvatarSetup.CreateFromThisModel)
            {
                EditorGUILayout.PropertyField(rootNode_prop, true);
            }

            if ((animationType == ModelImporterAnimationType.Generic || animationType == ModelImporterAnimationType.Human) &&
               avatarDefinition == ModelImporterAvatarSetup.CopyFromOther)
            {
                EditorGUILayout.PropertyField(sourceAvatar_prop, true);
            }

            if ((animationType == ModelImporterAnimationType.Generic || animationType == ModelImporterAnimationType.Human) &&
                avatarDefinition == ModelImporterAvatarSetup.CreateFromThisModel)
            {
                EditorGUILayout.PropertyField(optimizeGameobjects_prop, true);
                EditorGUILayout.PropertyField(extraExposedTransformPath_prop, true);
            }

            if (animationType != ModelImporterAnimationType.None)
            {
                EditorGUILayout.PropertyField(skinWeights_prop, true);
                if (skinWeights == ModelImporterSkinWeights.Custom)
                {
                    EditorGUILayout.PropertyField(maxBonesPerVert_prop, true);
                    if (maxBonesPerVert > 255) { maxBonesPerVert = 255; }
                    if (maxBonesPerVert < 1) { maxBonesPerVert = 1; }
                    EditorGUILayout.PropertyField(minBoneWeight_prop, true);
                    if (minBoneWeight < 0.001f) { minBoneWeight = 0.001f; }
                    if (minBoneWeight > 0.5f) { minBoneWeight = 0.5f; }
                }
                if ((QualitySettings.skinWeights == SkinWeights.OneBone || QualitySettings.skinWeights == SkinWeights.TwoBones) &&
                    (skinWeights == ModelImporterSkinWeights.Standard || maxBonesPerVert > 2))
                {
                    EditorGUILayout.HelpBox("Skin weight is less defined in quality setting than what is here. You will loose accuracy!", MessageType.Info);
                }
            }
            EditorGUILayout.PropertyField(stripBones_prop, true);
            EditorGUI.indentLevel--;
        }
        void ApplyRigSetting(ModelImporter modelImporter)
        {
            modelImporter.animationType = animationType;
            modelImporter.optimizeBones = stripBones;
            modelImporter.generateAnimations = generation;
            modelImporter.skinWeights = skinWeights;
            modelImporter.maxBonesPerVertex = maxBonesPerVert;
            modelImporter.minBoneWeight = minBoneWeight;
            modelImporter.motionNodeName = rootNode;
            if (animationType != ModelImporterAnimationType.None && animationType != ModelImporterAnimationType.Legacy)
            {
                modelImporter.sourceAvatar = sourceAvatar;
                modelImporter.avatarSetup = avatarDefinition;
            }
            modelImporter.optimizeGameObjects = optimizeGameobjects;
            modelImporter.extraExposedTransformPaths = extraExposedTransformPath;
        }
        #endregion

        #region Animation Setting
        bool operateOnAnimationSetting = false;
        [SerializeField] bool importAnimation = false, importConstraints = false;
        SerializedProperty importAnimation_prop, importConstraints_prop;
        void InitAnimationProperties(BatchFbxOperation window)
        {
            window.importAnimation_prop = window.serialObj.FindProperty(nameof(importAnimation));
            window.importConstraints_prop = window.serialObj.FindProperty(nameof(importConstraints));
        }
        void DrawAnimationSettings()
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(importConstraints_prop, true);
            EditorGUILayout.PropertyField(importAnimation_prop, true);
            EditorGUI.indentLevel--;
        }
        void ApplyAnimSetting(ModelImporter modelImporter)
        {
            modelImporter.importConstraints = importConstraints;
            modelImporter.importAnimation = importAnimation;
        }
        #endregion

        ScriptableObject scriptableObj;
        SerializedObject serialObj;
        SerializedProperty modelListProp;
        SerializedProperty materialProp;
        const string lastPathID = nameof(BatchFbxOperation) + "_ED_WINDOW_LAST_PATH";

        [MenuItem("Tools/UnityExt/Batch operations on FBX models")]
        public static void ShowWindow()
        {
            var window = EditorWindow.GetWindow<BatchFbxOperation>("Batch FBX Utility");
            window.scriptableObj = window;
            window.serialObj = new SerializedObject(window.scriptableObj);
            window.modelListProp = window.serialObj.FindProperty(nameof(fbxModels));
            window.materialProp = window.serialObj.FindProperty(nameof(material));
            window.InitModelProperties(window);
            window.InitRigProperties(window);
            window.InitAnimationProperties(window);
        }
        void OnGUI()
        {
            EditorGUILayout.HelpBox("Browse to set a folder for the models to process!", MessageType.Info);
            if (GUILayout.Button("Browse"))
            {
                var result = new List<GameObject>();
                var defaultPath = PlayerPrefs.GetString(lastPathID);
                var path = EditorUtility.OpenFolderPanel("Folder for models", defaultPath, "");
                if (path.ExIsValid())
                {
                    PlayerPrefs.SetString(lastPathID, path);

                    var filesInside = Directory.GetFiles(path, "*.fbx", SearchOption.AllDirectories);
                    foreach (var file in filesInside)
                    {
                        var relPath = file.ExAssetsRelativePath();
                        var model = AssetDatabase.LoadAssetAtPath<GameObject>(relPath);
                        if (model != null)
                        {
                            result.Add(model);
                        }
                    }
                }
                fbxModels = result.ToArray();
            }

            serialObj.Update();
            EditorGUILayout.PropertyField(modelListProp, true);
            processModel = EditorGUILayout.Toggle("Process Model? ", processModel);
            processRig = EditorGUILayout.Toggle("Process Rig? ", processRig);
            processAnim = EditorGUILayout.Toggle("Process Animation? ", processAnim);
            processMaterial = EditorGUILayout.Toggle("Process Material? ", processMaterial);

            if (processModel)
            {
                operateOnModelSetting = EditorGUILayout.Foldout(operateOnModelSetting, "Model");
                if (operateOnModelSetting)
                {
                    DrawModelSettings();
                }
            }

            if (processRig)
            {
                operateOnRigSetting = EditorGUILayout.Foldout(operateOnRigSetting, "Rig");
                if (operateOnRigSetting)
                {
                    DrawRigSettings();
                }
            }

            if (processAnim)
            {
                operateOnAnimationSetting = EditorGUILayout.Foldout(operateOnAnimationSetting, "Animation");
                if (operateOnAnimationSetting)
                {
                    DrawAnimationSettings();
                }
            }

            if (processMaterial)
            {
                EditorGUILayout.PropertyField(materialProp, true);
            }

            EditorGUILayout.Space(20f);
            if (GUILayout.Button(System.Environment.NewLine + "Process" + System.Environment.NewLine))
            {
                if (material == null)
                {
                    EditorUtility.DisplayDialog("!!Error!!", "Material is not set!", "Ok");
                }
                else
                {
                    fbxModels.ExForEachSafe((i) =>
                    {
                        ProcessModel(i, material);
                    });
                }
            }
            serialObj.ApplyModifiedProperties();
        }
        void ProcessModel(GameObject gameObject, Material material)
        {
            var assetPath = AssetDatabase.GetAssetPath(gameObject);
            if (!assetPath.ExIsValid() || gameObject == null) { return; }
            var modelImporter = (ModelImporter)AssetImporter.GetAtPath(assetPath);
            if (modelImporter == null) { return; }
            if (processMaterial)
            {
                modelImporter.materialLocation = ModelImporterMaterialLocation.InPrefab;
                modelImporter.materialName = ModelImporterMaterialName.BasedOnModelNameAndMaterialName;
                modelImporter.materialSearch = ModelImporterMaterialSearch.Local;

                var sourceMaterials = typeof(ModelImporter)
                        .GetProperty("sourceMaterials", BindingFlags.NonPublic | BindingFlags.Instance)?
                        .GetValue(modelImporter) as AssetImporter.SourceAssetIdentifier[];
                foreach (var identifier in sourceMaterials ?? Enumerable.Empty<AssetImporter.SourceAssetIdentifier>())
                {
                    modelImporter.AddRemap(identifier, material);
                }
            }
            if (processModel) { ApplyModelSettng(modelImporter); }
            if (processRig) { ApplyRigSetting(modelImporter); }
            if (processAnim) { ApplyAnimSetting(modelImporter); }
            modelImporter.SaveAndReimport();
        }
    }
}