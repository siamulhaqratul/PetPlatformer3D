using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityExt
{
    /// <summary>Extension methods for working with Unity <see cref="Transform"/> and <see cref="RectTransform"/> objects (UI tweens, hierarchy traversal, rect utilities, scaling, and enumeration helpers).</summary>
    public static class ExTransform
    {
        // ========================= PUBLIC METHODS ========================

        /// <summary>Shows a Transform (non-UI) by scaling from zero to one.</summary>
        /// <param name="transform">Target transform.</param>
        /// <param name="OnComplete">Optional callback invoked after tween completes.</param>
        /// <returns>The created tween.</returns>
        public static Tween ExShow(this Transform transform, Action OnComplete = null)
        {
            transform.gameObject.SetActive(true);
            transform.localScale = Vector3.zero;
            return transform.DOScale(1.0f, UConfig.DefaultTweenTime).OnComplete(() =>
            {
                OnComplete?.Invoke();
            });
        }
        /// <summary>Shows a RectTransform (UI) by scaling from zero to one.</summary>
        /// <param name="transform">Target rect transform.</param>
        /// <param name="OnComplete">Optional callback invoked after tween completes.</param>
        /// <returns>The created tween.</returns>
        public static Tween ExShow(this RectTransform transform, Action OnComplete = null)
        {
            transform.gameObject.SetActive(true);
            transform.localScale = Vector3.zero;
            return transform.DOScale(1.0f, UConfig.DefaultTweenTime).OnComplete(() =>
            {
                OnComplete?.Invoke();
            });
        }
        /// <summary>Hides a Transform (non-UI) by scaling to zero and disabling the GameObject.</summary>
        /// <param name="transform">Target transform.</param>
        /// <param name="OnComplete">Optional callback invoked after tween completes (before deactivation).</param>
        /// <returns>The created tween.</returns>
        public static Tween ExHide(this Transform transform, Action OnComplete = null)
        {
            transform.gameObject.SetActive(true);
            return transform.DOScale(0.0f, UConfig.DefaultTweenTime).OnComplete(() =>
            {
                OnComplete?.Invoke();
                transform.gameObject.SetActive(false);
            });
        }
        /// <summary>Hides a RectTransform (UI) by scaling to zero and disabling the GameObject.</summary>
        /// <param name="transform">Target rect transform.</param>
        /// <param name="OnComplete">Optional callback invoked after tween completes (before deactivation).</param>
        /// <returns>The created tween.</returns>
        public static Tween ExHide(this RectTransform transform, Action OnComplete = null)
        {
            transform.gameObject.SetActive(true);
            return transform.DOScale(0.0f, UConfig.DefaultTweenTime).OnComplete(() =>
            {
                OnComplete?.Invoke();
                transform.gameObject.SetActive(false);
            });
        }
        /// <summary>Opens or closes a UI panel with state tracking using a reference flag.</summary>
        /// <param name="panelUI">Panel transform.</param>
        /// <param name="OpenFlag">Reference flag storing current open state; updated inside the method.</param>
        /// <param name="interaction">Interaction type (open or close).</param>
        /// <param name="smoothTween">Whether to animate the transition.</param>
        /// <param name="OnComplete">Optional callback on completion.</param>
        /// <param name="overrideTweenAmount">Whether to override default tween scale amount.</param>
        /// <param name="offset">Override scale offset if overriding.</param>
        /// <param name="tweenTime">Override tween time if overriding.</param>
        /// <returns>The created tween if animated; otherwise null.</returns>
        public static Tween ExSetUIPanel(this Transform panelUI, ref bool OpenFlag, InteractionType interaction,
            bool smoothTween = true, System.Action OnComplete = null, bool overrideTweenAmount = false, float offset = 0.8f, float tweenTime = 0.4f)
        {
            Tween result = null;
            if (smoothTween)
            {
                if ((OpenFlag && interaction == InteractionType.Open) || (!OpenFlag && interaction == InteractionType.Close)) { return result; }
                OpenFlag = interaction == InteractionType.Open;
                panelUI.gameObject.SetActive(true);
                var amount = overrideTweenAmount ? offset : UConfig.PanelOpenCloseOffset;
                var within = overrideTweenAmount ? tweenTime : UConfig.DefaultTweenTime;
                panelUI.localScale = interaction == InteractionType.Open ? Vector3.one * amount : Vector3.one;
                result = panelUI.DOScale(interaction == InteractionType.Open ? 1.0f : amount, within).OnComplete(() =>
                {
                    OnComplete?.Invoke();
                    if (interaction == InteractionType.Close)
                    {
                        panelUI.gameObject.SetActive(false);
                    }
                });
            }
            else
            {
                OpenFlag = interaction == InteractionType.Open;
                panelUI.localScale = interaction == InteractionType.Open ? Vector3.one : Vector3.zero;
                panelUI.gameObject.SetActive(interaction == InteractionType.Open);
                OnComplete?.Invoke();
            }
            return result;
        }
        /// <summary>Opens or closes a UI panel (no external state flag).</summary>
        /// <param name="panelUI">Panel transform.</param>
        /// <param name="interaction">Interaction type (open or close).</param>
        /// <param name="smoothTween">Whether to animate the transition.</param>
        /// <param name="OnComplete">Optional callback on completion.</param>
        /// <param name="overrideTweenAmount">Whether to override default tween scale amount.</param>
        /// <param name="offset">Override scale offset if overriding.</param>
        /// <param name="tweenTime">Override tween time if overriding.</param>
        /// <returns>The created tween if animated; otherwise null.</returns>
        public static Tween ExSetUIPanel(this Transform panelUI, InteractionType interaction,
            bool smoothTween = true, System.Action OnComplete = null, bool overrideTweenAmount = false, float offset = 0.8f, float tweenTime = 0.4f)
        {
            Tween result = null;
            if (smoothTween)
            {
                panelUI.gameObject.SetActive(true);
                var amount = overrideTweenAmount ? offset : UConfig.PanelOpenCloseOffset;
                var within = overrideTweenAmount ? tweenTime : UConfig.DefaultTweenTime;
                panelUI.localScale = interaction == InteractionType.Open ? Vector3.one * amount : Vector3.one;
                result = panelUI.DOScale(interaction == InteractionType.Open ? 1.0f : amount, within).OnComplete(() =>
                {
                    OnComplete?.Invoke();
                    if (interaction == InteractionType.Close)
                    {
                        panelUI.gameObject.SetActive(false);
                    }
                });
            }
            else
            {
                panelUI.localScale = interaction == InteractionType.Open ? Vector3.one : Vector3.zero;
                panelUI.gameObject.SetActive(interaction == InteractionType.Open);
                OnComplete?.Invoke();
            }
            return result;
        }
        /// <summary>Builds a string representing the transform hierarchy chain upward (current to root).</summary>
        /// <param name="transform">Starting transform.</param>
        /// <returns>Hierarchy string ("->Child->Parent...").</returns>
        public static string ExGetHierarchy(this Transform transform)
        {
            var str = "";
            var ch = transform;
            while (ch != null)
            {
                str += "->" + ch.name;
                ch = ch.parent;
            }
            return str;
        }
        /// <summary>Creates an array of world positions from the provided transform list.</summary>
        /// <param name="tList">Source transform list.</param>
        /// <returns>Array of positions.</returns>
        public static Vector3[] GetPosListFrom(this List<Transform> tList)
        {
            var res = new List<Vector3>();
            tList.ExForEachSafe((i) =>
            {
                res.Add(i.position);
            });
            return res.ToArray();
        }
        /// <summary>Gets all immediate (non-recursive) children of a transform.</summary>
        /// <param name="tr">Parent transform.</param>
        /// <returns>List of immediate child transforms.</returns>
        public static List<Transform> ExGetAllImmediateChilds(this Transform tr)
        {
            var result = new List<Transform>();
            for (int i = 0; i < tr.childCount; i++)
            {
                result.Add(tr.GetChild(i));
            }
            return result;
        }
        /// <summary>Applies all local rect/layout properties from a source RectTransform to a target.</summary>
        /// <param name="target">Target RectTransform to modify.</param>
        /// <param name="source">Source RectTransform to copy from.</param>
        public static void ApplyRectInfoFrom(this RectTransform target, RectTransform source)
        {
            target.localPosition = source.localPosition;
            target.localRotation = source.localRotation;
            target.localScale = source.localScale;
            target.anchorMin = source.anchorMin;
            target.anchorMax = source.anchorMax;
            target.anchoredPosition = source.anchoredPosition;
            target.sizeDelta = source.sizeDelta;
            target.pivot = source.pivot;
        }
        /// <summary>Sets a single 2D anchored position axis on a RectTransform.</summary>
        /// <param name="rectTransform">Target RectTransform.</param>
        /// <param name="axis">Axis to set.</param>
        /// <param name="amount">Value to apply to axis.</param>
        public static void ExSetLocalRectTransform(this RectTransform rectTransform, SetTransformAxis axis, float amount)
        {
            var pos = rectTransform.anchoredPosition;
            switch (axis)
            {
                case SetTransformAxis.X:
                    pos.x = amount;
                    break;
                case SetTransformAxis.Y:
                    pos.y = amount;
                    break;
                default:
                    break;
            }
            rectTransform.anchoredPosition = pos;
        }
        /// <summary>Sets a single 3D anchored position axis on a RectTransform.</summary>
        /// <param name="rectTransform">Target RectTransform.</param>
        /// <param name="axis">Axis to set.</param>
        /// <param name="amount">Value to apply to axis.</param>
        public static void ExSetRectTransform(this RectTransform rectTransform, SetTransformAxis axis, float amount)
        {
            var pos = rectTransform.anchoredPosition3D;
            switch (axis)
            {
                case SetTransformAxis.X:
                    pos.x = amount;
                    break;
                case SetTransformAxis.Y:
                    pos.y = amount;
                    break;
                default:
                    break;
            }
            rectTransform.anchoredPosition3D = pos;
        }
        /// <summary>Resizes a RectTransform to match the device safe area (only valid for full-screen canvases).</summary>
        /// <param name="rectTransform">RectTransform to adjust.</param>
        /// <param name="canvas">Parent canvas used for size reference.</param>
        public static void ResizeToSafeArea(this RectTransform rectTransform, Canvas canvas)
        {
            if (rectTransform == null)
            {
                Debug.LogError("recttransform can not be null!");
            }

            Rect safe = Screen.safeArea;
            Rect cnvRect = canvas.pixelRect;

            Vector2 anchorMin = safe.position;
            Vector2 anchorMax = safe.position + safe.size;
            anchorMin.x /= cnvRect.width;
            anchorMin.y /= cnvRect.height;
            anchorMax.x /= cnvRect.width;
            anchorMax.y /= cnvRect.height;

#if UNITY_EDITOR
            rectTransform.anchorMin = new Vector2(0f, 0.05f);
            rectTransform.anchorMax = new Vector2(1f, 0.95f);
#else
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
#endif
        }


        //https://gist.github.com/AdamCarballo/5fd4f021f0332f1bd40f8b1721d1c32f
        /// <summary>Converts a world position to a canvas (RectTransform) anchored position.</summary>
        /// <param name="worldObj">World object transform.</param>
        /// <param name="canvas">Target canvas RectTransform.</param>
        /// <param name="cam">Optional camera (defaults to main).</param>
        /// <returns>Canvas space position.</returns>
        public static Vector3 ExWorldToCanvasPosition(this Transform worldObj, RectTransform canvas, Camera cam = null)
        {
            if (!cam)
            {
                cam = Camera.main;
            }
            Vector2 ViewportPosition = cam.WorldToViewportPoint(worldObj.position);
            Vector2 WorldObject_CanvasPosition = new Vector2(
            (ViewportPosition.x * canvas.sizeDelta.x) - (canvas.sizeDelta.x * 0.5f),
            (ViewportPosition.y * canvas.sizeDelta.y) - (canvas.sizeDelta.y * 0.5f));

            return WorldObject_CanvasPosition;
        }

        /// <summary>Resets local position, rotation (Euler) and scale to defaults.</summary>
        /// <param name="transform">Target transform.</param>
        public static void ExResetLocal(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localEulerAngles = Vector3.zero;
            transform.localScale = Vector3.one;
        }
        /// <summary>Resets local transforms (position/rotation/scale) for each transform in a list.</summary>
        /// <param name="transforms">List of transforms to reset.</param>
        public static void ExResetLocal(this List<Transform> transforms)
        {
            transforms.ExForEachSafe((t) =>
            {
                t.ExResetLocal();
            });
        }

        /// <summary>Sets the global (lossy) scale of a transform by adjusting local scale.</summary>
        /// <param name="transform">Target transform.</param>
        /// <param name="globalScale">Desired global scale.</param>
        public static void ExSetGlobalScale(this Transform transform, Vector3 globalScale)
        {
            transform.localScale = Vector3.one;
            var currentGlobalScale = transform.lossyScale;
            transform.localScale = new Vector3(globalScale.x / currentGlobalScale.x, globalScale.y / currentGlobalScale.y, globalScale.z / currentGlobalScale.z);
        }
        /// <summary>Destroys all child GameObjects inside a holder transform.</summary>
        /// <param name="holder">Parent transform.</param>
        /// <param name="editorExecution">If true, executes in editor context (UNITY_EDITOR only).</param>
        /// <param name="editorDeleteNow">If true and in editor, uses DestroyImmediate immediately; otherwise delayed.</param>
        public static void ExDestroyEverythingInside(this Transform holder, bool editorExecution = false, bool editorDeleteNow = false)
        {
            var delList = new List<GameObject>();
            if (holder.childCount > 0)
            {
                for (int i = 0; i < holder.childCount; i++)
                {
                    delList.Add(holder.GetChild(i).gameObject);
                }
            }
            if (delList != null && delList.Count > 0)
            {
                for (int i = 0; i < delList.Count; i++)
                {
                    var d = delList[i];
                    if (d == null) { continue; }
                    if (editorExecution)
                    {
#if UNITY_EDITOR
                        if (editorDeleteNow) { GameObject.DestroyImmediate(d); }
                        else
                        {
                            UnityEditor.EditorApplication.delayCall += () =>
                            {
                                GameObject.DestroyImmediate(d);
                            };
                        }
#endif
                    }
                    else
                    {
                        GameObject.Destroy(d);
                    }
                }
            }
        }
        /// <summary>Gets all immediate child transforms (non-recursive).</summary>
        /// <param name="transform">Parent transform.</param>
        /// <returns>List of immediate child transforms.</returns>
        public static List<Transform> ExGetImmediateChilds(this Transform transform)
        {
            var result = new List<Transform>();
            var count = transform.childCount;
            if (count == 0) { return result; }
            for (int i = 0; i < count; i++)
            {
                result.Add(transform.GetChild(i));
            }
            return result;
        }

        #region Foreach
        /// <summary>Invokes delegate for each immediate child transform.</summary>
        /// <param name="transform">Parent transform.</param>
        /// <param name="Code">Callback invoked per child.</param>
        public static void ExForEachImmediateChilds(this Transform transform, OnDoAnything<Transform> Code)
        {
            var count = transform.childCount;
            if (count == 0) { return; }
            for (int i = 0; i < count; i++)
            {
                Code?.Invoke(transform.GetChild(i));
            }
        }
        /// <summary>Asynchronously invokes delegate for each immediate child transform.</summary>
        /// <typeparam name="T">Unused generic (signature disambiguation).</typeparam>
        /// <param name="transform">Parent transform.</param>
        /// <param name="Code">Async callback per child.</param>
        public static async Task ExForEachImmediateChilds<T>(this Transform transform, Func<Transform, Task> Code)
        {
            var count = transform.childCount;
            if (count == 0) { return; }
            for (int i = 0; i < count; i++)
            {
                await Code?.Invoke(transform.GetChild(i));
            }
        }
        /// <summary>Invokes delegate for each parent in the chain upward.</summary>
        /// <param name="transform">Starting transform.</param>
        /// <param name="Code">Callback per parent.</param>
        public static void ExForEachParentOnChain(this Transform transform, OnDoAnything<Transform> Code)
        {
            var tr = transform.parent;
            while (tr != null)
            {
                Code?.Invoke(tr);
                tr = tr.parent;
            }
        }
        /// <summary>Asynchronously invokes delegate for each parent in the chain upward.</summary>
        /// <typeparam name="T">Unused generic (signature disambiguation).</typeparam>
        /// <param name="transform">Starting transform.</param>
        /// <param name="Code">Async callback per parent.</param>
        public static async Task ExForEachParentOnChain<T>(this Transform transform, Func<Transform, Task> Code)
        {
            var tr = transform.parent;
            while (tr != null)
            {
                await Code?.Invoke(tr);
                tr = tr.parent;
            }
        }
        /// <summary>Recursively invokes delegate for all descendants (depth-first).</summary>
        /// <param name="transform">Root transform.</param>
        /// <param name="Code">Callback per descendant.</param>
        public static void ExForEachChilds(this Transform transform, OnDoAnything<Transform> Code)
        {
            CH(transform);
            void CH(Transform tr)
            {
                var len = tr.childCount;
                for (int i = 0; i < len; i++)
                {
                    var chtr = tr.GetChild(i);
                    Code?.Invoke(chtr);
                    CH(chtr);
                }
            }
        }
        /// <summary>Asynchronously and recursively invokes delegate for all descendants (depth-first).</summary>
        /// <typeparam name="T">Unused generic (signature disambiguation).</typeparam>
        /// <param name="transform">Root transform.</param>
        /// <param name="Code">Async callback per descendant.</param>
        public static async Task ExForEachChilds<T>(this Transform transform, Func<Transform, Task> Code)
        {
            await CH(transform);
            async Task CH(Transform tr)
            {
                var len = tr.childCount;
                for (int i = 0; i < len; i++)
                {
                    var chtr = tr.GetChild(i);
                    await Code?.Invoke(chtr);
                    await CH(chtr);
                }
            }
        }
        #endregion

        #region Foreach With ID
        /// <summary>Invokes delegate for each immediate child providing index.</summary>
        /// <typeparam name="T">Unused generic (signature disambiguation).</typeparam>
        /// <param name="transform">Parent transform.</param>
        /// <param name="Code">Callback with child and index.</param>
        public static void ExForEachImmediateChilds<T>(this Transform transform, OnDoAnything<Transform, int> Code)
        {
            var count = transform.childCount;
            if (count == 0) { return; }
            for (int i = 0; i < count; i++)
            {
                Code?.Invoke(transform.GetChild(i), i);
            }
        }
        /// <summary>Asynchronously invokes delegate for each immediate child providing index.</summary>
        /// <typeparam name="T">Unused generic.</typeparam>
        /// <param name="transform">Parent transform.</param>
        /// <param name="Code">Async callback with child and index.</param>
        public static async Task ExForEachImmediateChilds<T>(this Transform transform, Func<Transform, int, Task> Code)
        {
            var count = transform.childCount;
            if (count == 0) { return; }
            for (int i = 0; i < count; i++)
            {
                await Code?.Invoke(transform.GetChild(i), i);
            }
        }
        /// <summary>Invokes delegate for each parent in the chain providing ascending counter.</summary>
        /// <param name="transform">Starting transform.</param>
        /// <param name="Code">Callback with parent and running index (1-based).</param>
        public static void ExForEachParentOnChain(this Transform transform, OnDoAnything<Transform, int> Code)
        {
            var tr = transform.parent;
            var counter = 0;
            while (tr != null)
            {
                counter++;
                Code?.Invoke(tr, counter);
                tr = tr.parent;
            }
        }
        /// <summary>Asynchronously invokes delegate for each parent in the chain providing ascending counter.</summary>
        /// <typeparam name="T">Unused generic.</typeparam>
        /// <param name="transform">Starting transform.</param>
        /// <param name="Code">Async callback with parent and running index.</param>
        public static async Task ExForEachParentOnChain<T>(this Transform transform, Func<Transform, int, Task> Code)
        {
            var tr = transform.parent;
            var counter = 0;
            while (tr != null)
            {
                counter++;
                await Code?.Invoke(tr, counter);
                tr = tr.parent;
            }
        }
        /// <summary>Recursively invokes delegate for all descendants providing running and sibling indices.</summary>
        /// <param name="transform">Root transform.</param>
        /// <param name="Code">Callback (child, running index, sibling index).</param>
        public static void ExForEachChilds(this Transform transform, OnDoAnything<Transform, int, int> Code)
        {
            int counter = 0;
            CH(transform);
            void CH(Transform tr)
            {
                var len = tr.childCount;
                for (int i = 0; i < len; i++)
                {
                    var chtr = tr.GetChild(i);
                    counter++;
                    Code?.Invoke(chtr, counter, i);
                    CH(chtr);
                }
            }
        }
        /// <summary>Asynchronously and recursively invokes delegate for all descendants with running and sibling indices.</summary>
        /// <typeparam name="T">Unused generic.</typeparam>
        /// <param name="transform">Root transform.</param>
        /// <param name="Code">Async callback (child, running index, sibling index).</param>
        public static async Task ExForEachChilds<T>(this Transform transform, Func<Transform, int, int, Task> Code)
        {
            int counter = 0;
            await CH(transform);
            async Task CH(Transform tr)
            {
                var len = tr.childCount;
                for (int i = 0; i < len; i++)
                {
                    var chtr = tr.GetChild(i);
                    counter++;
                    await Code?.Invoke(chtr, counter, i);
                    await CH(chtr);
                }
            }
        }
        #endregion

        #region Foreach with break
        /// <summary>Invokes delegate for each immediate child until break predicate returns true.</summary>
        /// <param name="transform">Parent transform.</param>
        /// <param name="Code">Callback per child.</param>
        /// <param name="whenBreak">Predicate to break iteration.</param>
        public static void ExForEachImmediateChilds(this Transform transform, OnDoAnything<Transform> Code, WhenToDoFunc<Transform> whenBreak)
        {
            var count = transform.childCount;
            if (count == 0) { return; }
            for (int i = 0; i < count; i++)
            {
                var t = transform.GetChild(i);
                if (whenBreak.Invoke(t)) { break; }
                Code?.Invoke(t);
            }
        }
        /// <summary>Asynchronously invokes delegate for each immediate child until break predicate returns true.</summary>
        /// <typeparam name="T">Unused generic.</typeparam>
        /// <param name="transform">Parent transform.</param>
        /// <param name="Code">Async callback per child.</param>
        /// <param name="whenBreak">Predicate to break iteration.</param>
        public static async Task ExForEachImmediateChilds<T>(this Transform transform, Func<Transform, Task> Code, WhenToDoFunc<Transform> whenBreak)
        {
            var count = transform.childCount;
            if (count == 0) { return; }
            for (int i = 0; i < count; i++)
            {
                var t = transform.GetChild(i);
                if (whenBreak.Invoke(t)) { break; }
                await Code?.Invoke(t);
            }
        }
        /// <summary>Invokes delegate for each parent until break predicate returns true.</summary>
        /// <param name="transform">Starting transform.</param>
        /// <param name="Code">Callback per parent.</param>
        /// <param name="whenBreak">Predicate to break iteration.</param>
        public static void ExForEachParentOnChain(this Transform transform, OnDoAnything<Transform> Code, WhenToDoFunc<Transform> whenBreak)
        {
            var tr = transform.parent;
            while (tr != null)
            {
                if (whenBreak.Invoke(tr)) { break; }
                Code?.Invoke(tr);
                tr = tr.parent;
            }
        }
        /// <summary>Asynchronously invokes delegate for each parent until break predicate returns true.</summary>
        /// <typeparam name="T">Unused generic.</typeparam>
        /// <param name="transform">Starting transform.</param>
        /// <param name="Code">Async callback per parent.</param>
        /// <param name="whenBreak">Predicate to break iteration.</param>
        public static async Task ExForEachParentOnChain<T>(this Transform transform, Func<Transform, Task> Code, WhenToDoFunc<Transform> whenBreak)
        {
            var tr = transform.parent;
            while (tr != null)
            {
                if (whenBreak.Invoke(tr)) { break; }
                await Code?.Invoke(tr);
                tr = tr.parent;
            }
        }
        /// <summary>Recursively invokes delegate for all descendants until break predicate returns true at each level.</summary>
        /// <param name="transform">Root transform.</param>
        /// <param name="Code">Callback per descendant.</param>
        /// <param name="whenBreak">Predicate to break iteration for a branch.</param>
        public static void ExForEachChilds(this Transform transform, OnDoAnything<Transform> Code, WhenToDoFunc<Transform> whenBreak)
        {
            CH(transform);
            void CH(Transform tr)
            {
                var len = tr.childCount;
                for (int i = 0; i < len; i++)
                {
                    var chtr = tr.GetChild(i);
                    if (whenBreak.Invoke(chtr)) { break; }
                    Code?.Invoke(chtr);
                    CH(chtr);
                }
            }
        }
        /// <summary>Asynchronously and recursively invokes delegate for all descendants until break predicate returns true at each level.</summary>
        /// <typeparam name="T">Unused generic.</typeparam>
        /// <param name="transform">Root transform.</param>
        /// <param name="Code">Async callback per descendant.</param>
        /// <param name="whenBreak">Predicate to break iteration for a branch.</param>
        public static async Task ExForEachChilds<T>(this Transform transform, Func<Transform, Task> Code, WhenToDoFunc<Transform> whenBreak)
        {
            await CH(transform);
            async Task CH(Transform tr)
            {
                var len = tr.childCount;
                for (int i = 0; i < len; i++)
                {
                    var chtr = tr.GetChild(i);
                    if (whenBreak.Invoke(chtr)) { break; }
                    await Code?.Invoke(chtr);
                    await CH(chtr);
                }
            }
        }
        #endregion

        #region Foreach With ID with break
        /// <summary>Invokes delegate for each immediate child (with index) until break predicate matches.</summary>
        /// <typeparam name="T">Unused generic.</typeparam>
        /// <param name="transform">Parent transform.</param>
        /// <param name="Code">Callback per child with index.</param>
        /// <param name="whenBreak">Predicate deciding when to break.</param>
        public static void ExForEachImmediateChilds<T>(this Transform transform, OnDoAnything<Transform, int> Code, WhenToDoFunc<Transform, int> whenBreak)
        {
            var count = transform.childCount;
            if (count == 0) { return; }
            for (int i = 0; i < count; i++)
            {
                var tr = transform.GetChild(i);
                if (whenBreak.Invoke(tr, i)) { break; }
                Code?.Invoke(tr, i);
            }
        }
        /// <summary>Asynchronously invokes delegate for each immediate child (with index) until break predicate matches.</summary>
        /// <typeparam name="T">Unused generic.</typeparam>
        /// <param name="transform">Parent transform.</param>
        /// <param name="Code">Async callback per child with index.</param>
        /// <param name="whenBreak">Predicate deciding when to break.</param>
        public static async Task ExForEachImmediateChilds<T>(this Transform transform, Func<Transform, int, Task> Code, WhenToDoFunc<Transform, int> whenBreak)
        {
            var count = transform.childCount;
            if (count == 0) { return; }
            for (int i = 0; i < count; i++)
            {
                var tr = transform.GetChild(i);
                if (whenBreak.Invoke(tr, i)) { break; }
                await Code?.Invoke(tr, i);
            }
        }
        /// <summary>Invokes delegate for each parent (with running counter) until break predicate matches.</summary>
        /// <param name="transform">Starting transform.</param>
        /// <param name="Code">Callback per parent with counter.</param>
        /// <param name="whenBreak">Predicate deciding when to break.</param>
        public static void ExForEachParentOnChain(this Transform transform, OnDoAnything<Transform, int> Code, WhenToDoFunc<Transform, int> whenBreak)
        {
            var tr = transform.parent;
            var counter = 0;
            while (tr != null)
            {
                counter++;
                if (whenBreak.Invoke(tr, counter)) { break; }
                Code?.Invoke(tr, counter);
                tr = tr.parent;
            }
        }
        /// <summary>Asynchronously invokes delegate for each parent (with running counter) until break predicate matches.</summary>
        /// <typeparam name="T">Unused generic.</typeparam>
        /// <param name="transform">Starting transform.</param>
        /// <param name="Code">Async callback per parent with counter.</param>
        /// <param name="whenBreak">Predicate deciding when to break.</param>
        public static async Task ExForEachParentOnChain<T>(this Transform transform, Func<Transform, int, Task> Code, WhenToDoFunc<Transform, int> whenBreak)
        {
            var tr = transform.parent;
            var counter = 0;
            while (tr != null)
            {
                counter++;
                if (whenBreak.Invoke(tr, counter)) { break; }
                await Code?.Invoke(tr, counter);
                tr = tr.parent;
            }
        }
        /// <summary>Recursively invokes delegate for all descendants with counters until break predicate matches.</summary>
        /// <param name="transform">Root transform.</param>
        /// <param name="Code">Callback (child, running counter, sibling index).</param>
        /// <param name="whenBreak">Predicate deciding when to break.</param>
        public static void ExForEachChilds(this Transform transform, OnDoAnything<Transform, int, int> Code, WhenToDoFunc<Transform, int, int> whenBreak)
        {
            int counter = 0;
            CH(transform);
            void CH(Transform tr)
            {
                var len = tr.childCount;
                for (int i = 0; i < len; i++)
                {
                    var chtr = tr.GetChild(i);
                    counter++;
                    if (whenBreak.Invoke(tr, counter, i)) { break; }
                    Code?.Invoke(chtr, counter, i);
                    CH(chtr);
                }
            }
        }
        /// <summary>Asynchronously and recursively invokes delegate for all descendants with counters until break predicate matches.</summary>
        /// <typeparam name="T">Unused generic.</typeparam>
        /// <param name="transform">Root transform.</param>
        /// <param name="Code">Async callback (child, running counter, sibling index).</param>
        /// <param name="whenBreak">Predicate deciding when to break.</param>
        public static async Task ExForEachChilds<T>(this Transform transform, Func<Transform, int, int, Task> Code, WhenToDoFunc<Transform, int, int> whenBreak)
        {
            int counter = 0;
            await CH(transform);
            async Task CH(Transform tr)
            {
                var len = tr.childCount;
                for (int i = 0; i < len; i++)
                {
                    var chtr = tr.GetChild(i);
                    counter++;
                    if (whenBreak.Invoke(tr, counter, i)) { break; }
                    await Code?.Invoke(chtr, counter, i);
                    await CH(chtr);
                }
            }
        }
        #endregion

        #region Foreach
        /// <summary>Invokes delegate for each immediate child in reverse order.</summary>
        /// <param name="transform">Parent transform.</param>
        /// <param name="Code">Callback per child.</param>
        public static void ExForEachReverseImmediateChilds(this Transform transform, OnDoAnything<Transform> Code)
        {
            var count = transform.childCount;
            if (count == 0) { return; }
            for (int i = count - 1; i >= 0; i--)
            {
                Code?.Invoke(transform.GetChild(i));
            }
        }
        /// <summary>Asynchronously invokes delegate for each immediate child in reverse order.</summary>
        /// <typeparam name="T">Unused generic.</typeparam>
        /// <param name="transform">Parent transform.</param>
        /// <param name="Code">Async callback per child.</param>
        public static async Task ExForEachReverseImmediateChilds<T>(this Transform transform, Func<Transform, Task> Code)
        {
            var count = transform.childCount;
            if (count == 0) { return; }
            for (int i = count - 1; i >= 0; i--)
            {
                await Code?.Invoke(transform.GetChild(i));
            }
        }
        /// <summary>Recursively invokes delegate for all descendants in reverse sibling order.</summary>
        /// <param name="transform">Root transform.</param>
        /// <param name="Code">Callback per descendant.</param>
        public static void ExForEachReverseChilds(this Transform transform, OnDoAnything<Transform> Code)
        {
            CH(transform);
            void CH(Transform tr)
            {
                var len = tr.childCount;
                for (int i = len - 1; i >= 0; i--)
                {
                    var chtr = tr.GetChild(i);
                    Code?.Invoke(chtr);
                    CH(chtr);
                }
            }
        }
        /// <summary>Asynchronously and recursively invokes delegate for all descendants in reverse sibling order.</summary>
        /// <typeparam name="T">Unused generic.</typeparam>
        /// <param name="transform">Root transform.</param>
        /// <param name="Code">Async callback per descendant.</param>
        public static async Task ExForEachReverseChilds<T>(this Transform transform, Func<Transform, Task> Code)
        {
            await CH(transform);
            async Task CH(Transform tr)
            {
                var len = tr.childCount;
                for (int i = len - 1; i >= 0; i--)
                {
                    var chtr = tr.GetChild(i);
                    await Code?.Invoke(chtr);
                    await CH(chtr);
                }
            }
        }
        #endregion

        #region Foreach Reverse With ID
        /// <summary>Invokes delegate for each immediate child in reverse order (with index).</summary>
        /// <typeparam name="T">Unused generic.</typeparam>
        /// <param name="transform">Parent transform.</param>
        /// <param name="Code">Callback per child with index.</param>
        public static void ExForEachReverseImmediateChilds<T>(this Transform transform, OnDoAnything<Transform, int> Code)
        {
            var count = transform.childCount;
            if (count == 0) { return; }
            for (int i = count - 1; i >= 0; i--)
            {
                Code?.Invoke(transform.GetChild(i), i);
            }
        }
        /// <summary>Asynchronously invokes delegate for each immediate child in reverse order (with index).</summary>
        /// <typeparam name="T">Unused generic.</typeparam>
        /// <param name="transform">Parent transform.</param>
        /// <param name="Code">Async callback per child with index.</param>
        public static async Task ExForEachReverseImmediateChilds<T>(this Transform transform, Func<Transform, int, Task> Code)
        {
            var count = transform.childCount;
            if (count == 0) { return; }
            for (int i = count - 1; i >= 0; i--)
            {
                await Code?.Invoke(transform.GetChild(i), i);
            }
        }
        /// <summary>Recursively invokes delegate for all descendants in reverse sibling order providing counters.</summary>
        /// <param name="transform">Root transform.</param>
        /// <param name="Code">Callback (child, running counter, sibling index).</param>
        public static void ExForEachReverseChilds(this Transform transform, OnDoAnything<Transform, int, int> Code)
        {
            int counter = 0;
            CH(transform);
            void CH(Transform tr)
            {
                var len = tr.childCount;
                for (int i = len - 1; i >= 0; i--)
                {
                    var chtr = tr.GetChild(i);
                    counter++;
                    Code?.Invoke(chtr, counter, i);
                    CH(chtr);
                }
            }
        }
        /// <summary>Asynchronously and recursively invokes delegate for all descendants in reverse order providing counters.</summary>
        /// <typeparam name="T">Unused generic.</typeparam>
        /// <param name="transform">Root transform.</param>
        /// <param name="Code">Async callback (child, running counter, sibling index).</param>
        public static async Task ExForEachReverseChilds<T>(this Transform transform, Func<Transform, int, int, Task> Code)
        {
            int counter = 0;
            await CH(transform);
            async Task CH(Transform tr)
            {
                var len = tr.childCount;
                for (int i = len - 1; i >= 0; i--)
                {
                    var chtr = tr.GetChild(i);
                    counter++;
                    await Code?.Invoke(chtr, counter, i);
                    await CH(chtr);
                }
            }
        }
        #endregion

        #region Foreach Reverse with break
        /// <summary>Invokes delegate for each immediate child in reverse order until break predicate matches.</summary>
        /// <param name="transform">Parent transform.</param>
        /// <param name="Code">Callback per child.</param>
        /// <param name="whenBreak">Predicate deciding when to break.</param>
        public static void ExForEachReverseImmediateChilds(this Transform transform, OnDoAnything<Transform> Code, WhenToDoFunc<Transform> whenBreak)
        {
            var count = transform.childCount;
            if (count == 0) { return; }
            for (int i = count - 1; i >= 0; i--)
            {
                var t = transform.GetChild(i);
                if (whenBreak.Invoke(t)) { break; }
                Code?.Invoke(t);
            }
        }
        /// <summary>Asynchronously invokes delegate for each immediate child in reverse order until break predicate matches.</summary>
        /// <typeparam name="T">Unused generic.</typeparam>
        /// <param name="transform">Parent transform.</param>
        /// <param name="Code">Async callback per child.</param>
        /// <param name="whenBreak">Predicate deciding when to break.</param>
        public static async Task ExForEachReverseImmediateChilds<T>(this Transform transform, Func<Transform, Task> Code, WhenToDoFunc<Transform> whenBreak)
        {
            var count = transform.childCount;
            if (count == 0) { return; }
            for (int i = count - 1; i >= 0; i--)
            {
                var t = transform.GetChild(i);
                if (whenBreak.Invoke(t)) { break; }
                await Code?.Invoke(t);
            }
        }
        /// <summary>Recursively invokes delegate for descendants in reverse order until break predicate matches at each branch.</summary>
        /// <param name="transform">Root transform.</param>
        /// <param name="Code">Callback per descendant.</param>
        /// <param name="whenBreak">Predicate deciding when to break per branch.</param>
        public static void ExForEachReverseChilds(this Transform transform, OnDoAnything<Transform> Code, WhenToDoFunc<Transform> whenBreak)
        {
            CH(transform);
            void CH(Transform tr)
            {
                var len = tr.childCount;
                for (int i = len - 1; i >= 0; i--)
                {
                    var chtr = tr.GetChild(i);
                    if (whenBreak.Invoke(chtr)) { break; }
                    Code?.Invoke(chtr);
                    CH(chtr);
                }
            }
        }
        /// <summary>Asynchronously and recursively invokes delegate for descendants in reverse order until break predicate matches at each branch.</summary>
        /// <typeparam name="T">Unused generic.</typeparam>
        /// <param name="transform">Root transform.</param>
        /// <param name="Code">Async callback per descendant.</param>
        /// <param name="whenBreak">Predicate deciding when to break per branch.</param>
        public static async Task ExForEachReverseChilds<T>(this Transform transform, Func<Transform, Task> Code, WhenToDoFunc<Transform> whenBreak)
        {
            await CH(transform);
            async Task CH(Transform tr)
            {
                var len = tr.childCount;
                for (int i = len - 1; i >= 0; i--)
                {
                    var chtr = tr.GetChild(i);
                    if (whenBreak.Invoke(chtr)) { break; }
                    await Code?.Invoke(chtr);
                    await CH(chtr);
                }
            }
        }
        #endregion

        #region Foreach Reverse With ID with break
        /// <summary>Invokes delegate for each immediate child in reverse order (with index) until break predicate matches.</summary>
        /// <typeparam name="T">Unused generic.</typeparam>
        /// <param name="transform">Parent transform.</param>
        /// <param name="Code">Callback per child with index.</param>
        /// <param name="whenBreak">Predicate deciding when to break.</param>
        public static void ExForEachReverseImmediateChilds<T>(this Transform transform, OnDoAnything<Transform, int> Code, WhenToDoFunc<Transform, int> whenBreak)
        {
            var count = transform.childCount;
            if (count == 0) { return; }
            for (int i = count - 1; i >= 0; i--)
            {
                var tr = transform.GetChild(i);
                if (whenBreak.Invoke(tr, i)) { break; }
                Code?.Invoke(tr, i);
            }
        }
        /// <summary>Asynchronously invokes delegate for each immediate child in reverse order (with index) until break predicate matches.</summary>
        /// <typeparam name="T">Unused generic.</typeparam>
        /// <param name="transform">Parent transform.</param>
        /// <param name="Code">Async callback per child with index.</param>
        /// <param name="whenBreak">Predicate deciding when to break.</param>
        public static async Task ExForEachReverseImmediateChilds<T>(this Transform transform, Func<Transform, int, Task> Code, WhenToDoFunc<Transform, int> whenBreak)
        {
            var count = transform.childCount;
            if (count == 0) { return; }
            for (int i = count - 1; i >= 0; i--)
            {
                var tr = transform.GetChild(i);
                if (whenBreak.Invoke(tr, i)) { break; }
                await Code?.Invoke(tr, i);
            }
        }
        /// <summary>Recursively invokes delegate for descendants in reverse order with counters until break predicate matches.</summary>
        /// <param name="transform">Root transform.</param>
        /// <param name="Code">Callback (child, running counter, sibling index).</param>
        /// <param name="whenBreak">Predicate deciding when to break per branch.</param>
        public static void ExForEachReverseChilds(this Transform transform, OnDoAnything<Transform, int, int> Code, WhenToDoFunc<Transform, int, int> whenBreak)
        {
            int counter = 0;
            CH(transform);
            void CH(Transform tr)
            {
                var len = tr.childCount;
                for (int i = len - 1; i >= 0; i--)
                {
                    var chtr = tr.GetChild(i);
                    counter++;
                    if (whenBreak.Invoke(tr, counter, i)) { break; }
                    Code?.Invoke(chtr, counter, i);
                    CH(chtr);
                }
            }
        }
        /// <summary>Asynchronously and recursively invokes delegate for descendants in reverse order with counters until break predicate matches.</summary>
        /// <typeparam name="T">Unused generic.</typeparam>
        /// <param name="transform">Root transform.</param>
        /// <param name="Code">Async callback (child, running counter, sibling index).</param>
        /// <param name="whenBreak">Predicate deciding when to break per branch.</param>
        public static async Task ExForEachReverseChilds<T>(this Transform transform, Func<Transform, int, int, Task> Code, WhenToDoFunc<Transform, int, int> whenBreak)
        {
            int counter = 0;
            await CH(transform);
            async Task CH(Transform tr)
            {
                var len = tr.childCount;
                for (int i = len - 1; i >= 0; i--)
                {
                    var chtr = tr.GetChild(i);
                    counter++;
                    if (whenBreak.Invoke(tr, counter, i)) { break; }
                    await Code?.Invoke(chtr, counter, i);
                    await CH(chtr);
                }
            }
        }
        #endregion
    }
}