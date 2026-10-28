using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityExt
{
    /// <summary>Extension methods for common UI operations on <see cref="MaskableGraphic"/> collections (alpha changes, activation toggles, validation).</summary>
    public static class ExUI
    {
        // ========================= PUBLIC METHODS ========================

        /// <summary>Validates that each graphic in the list is non-null in development/editor builds.</summary>
        /// <param name="graphics">List of graphics to check.</param>
        /// <param name="context">Context string appended to exception for easier debugging.</param>
        public static void CheckGraphics(this List<MaskableGraphic> graphics, string context)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (graphics != null && graphics.Count > 0)
            {
                for (int i = 0; i < graphics.Count; i++)
                {
                    var mask = graphics[i];
                    if (mask == null)
                    {
                        throw new Exception("Invalid entry in the graphics list--context: " + context);
                    }
                }
            }
#endif
        }

        #region Alpha
        /// <summary>Sets the alpha channel of a single graphic.</summary>
        /// <param name="graphic">Target graphic (must be non-null).</param>
        /// <param name="alpha">Alpha value (0-1).</param>
        public static void ExSetAlpha(this MaskableGraphic graphic, float alpha)
        {
            var col = graphic.color;
            col.a = alpha;
            graphic.color = col;
        }
        /// <summary>Sets the alpha channel for each graphic in the list (skips null and optional exceptions).</summary>
        /// <param name="graphics">List of graphics.</param>
        /// <param name="alpha">Alpha value (0-1).</param>
        /// <param name="exceptions">Optional graphics to exclude.</param>
        public static void ExSetAlpha(this List<MaskableGraphic> graphics, float alpha, params MaskableGraphic[] exceptions)
        {
            if (graphics != null && graphics.Count > 0)
            {
                for (int i = 0; i < graphics.Count; i++)
                {
                    var graphic = graphics[i];
                    if (graphic == null) { continue; }
                    if (exceptions != null)
                    {
                        if (exceptions.ExContains(graphic)) { continue; }
                    }
                    graphic.ExSetAlpha(alpha);
                }
            }
        }
        #endregion

        #region Activation
        /// <summary>Enables or disables (via <c>enabled</c>) every graphic in the list.</summary>
        /// <param name="graphics">List of graphics.</param>
        /// <param name="enable">True to enable; false to disable.</param>
        public static void ExSetActive(this List<MaskableGraphic> graphics, bool enable)
        {
            if (graphics != null && graphics.Count > 0)
            {
                for (int i = 0; i < graphics.Count; i++)
                {
                    var graphic = graphics[i];
                    if (graphic == null) { continue; }
                    graphic.enabled = enable;
                }
            }
        }
        /// <summary>Enables or disables every graphic in the list excluding provided exceptions.</summary>
        /// <param name="graphics">List of graphics.</param>
        /// <param name="enable">True to enable; false to disable.</param>
        /// <param name="exceptions">Optional graphics to skip.</param>
        public static void ExSetActive(this List<MaskableGraphic> graphics, bool enable, params MaskableGraphic[] exceptions)
        {
            if (graphics != null && graphics.Count > 0)
            {
                for (int i = 0; i < graphics.Count; i++)
                {
                    var graphic = graphics[i];
                    if (graphic == null) { continue; }
                    if (exceptions != null)
                    {
                        if (exceptions.ExContains(graphic)) { continue; }
                    }
                    graphic.enabled = enable;
                }
            }
        }
        #endregion
    }
}