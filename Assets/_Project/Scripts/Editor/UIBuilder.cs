using TMPro;
using System.Collections.Generic;
using JFeel;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

public static class UIBuilder
{
    // =========================== CONSTANTS ===========================

    private const string MENU_PATH = "Tools/Pet Shift Run/Build UI";
    private const string UI_ROOT_NAME = "UI";
    private const string EVENT_SYSTEM_NAME = "EventSystem";
    private const string KENNEY_UI = "Assets/_Project/UI/Kenney";
    private const string SPRITE_SIZE_FOLDER = "Double";
    private const string FONT_PATH = KENNEY_UI + "/Font/Kenney Future.ttf";
    private const string FONT_FOLDER = "Assets/_Project/UI/Fonts";
    private const string FONT_ASSET_PATH = FONT_FOLDER + "/KenneyFuture SDF.asset";
    private const string OUTLINE_MATERIAL_PATH = FONT_FOLDER + "/KenneyFuture Outline.mat";
    private const string CLICK_SOUND_PATH = KENNEY_UI + "/Sounds/click-a.ogg";
    private const string COIN_SOUND_PATH = KENNEY_UI + "/Sounds/switch-a.ogg";
    private const string PRELOADED_CHARACTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 !?.,:/-'%+";
    private const float SLICE_BORDER_RATIO = 0.3f;
    private const float DOUBLE_SPRITE_PPU_MULTIPLIER = 2f;
    private const float RIBBON_TEXT_PADDING = 90f;
    private const float RIBBON_MIN_FONT_SIZE = 40f;
    // Every label may shrink to this share of its font size, so text never spills out of its box.
    private const float MIN_FONT_SCALE = 0.6f;
    // Button labels: the gap after the icon and the space kept clear at each end, as shares of the button height.
    private const float BUTTON_ICON_GAP = 0.12f;
    private const float BUTTON_TEXT_PADDING = 0.3f;
    private const float OBJECTIVE_CHECK_SIZE = 56f;
    private const float OBJECTIVE_TEXT_WIDTH = 440f;
    private const float OBJECTIVE_GAP = 20f;
    private const float COIN_ROW_GAP = 18f;
    private const float RESULT_COIN_SIZE = 70f;
    // Space between the coin count and the score on a result row.
    private const float RESULT_GROUP_GAP = 60f;
    private const float PILL_TEXT_PADDING = 25f;

    private const string BLUE = "Blue";
    private const string GREEN = "Green";
    private const string GREY = "Grey";
    private const string RED = "Red";
    private const string YELLOW = "Yellow";
    private const string EXTRA = "Extra";
    private const string BUTTON = "button_rectangle_depth_gloss";
    private const string ROUND_BUTTON = "button_round_depth_gloss";
    private const string CARD = "button_square_depth_border";
    private const string PILL = "button_rectangle_depth_flat";
    private const string INSET = "input_rectangle";
    private const string ARROW_LEFT = "arrow_basic_w";
    private const string ARROW_RIGHT = "arrow_basic_e";
    private const string ARROW_UP = "arrow_basic_n";
    private const string ARROW_DOWN = "arrow_basic_s";
    private const float HINT_GROUP_SPACING = 50f;
    private const string CHECK_ON = "check_square_color_checkmark";
    private const string CHECK_OFF = "check_square_grey";
    private const float KEYCAP_SIZE = 80f;
    private const float PORTRAIT_CONTENT_WIDTH = 1000f;
    // Gaps from the screen edges to the top-row items (HUD pills, pause and settings buttons): tight to the side,
    // roomier at the top so notches and status bars never cover them.
    private const float EDGE_SIDE_GAP = 16f;
    private const float EDGE_TOP_GAP = 39f;
    private const float HUD_ROW_GAP = 12f;
    private const float HUD_ITEM_GAP = 24f;
    private const float CORNER_BUTTON_SIZE = 112f;
    // Height of the progress marker's centre above the bar's centre; below zero it sits lower on the bar.
    private const float PROGRESS_MARKER_Y = -25f;
    // The play button and best stars grow in portrait, where the screen is tall enough for them.
    private const float PORTRAIT_MENU_SCALE = 1.2f;
    private const string DIVIDER = "divider_edges";
    private const float SHINE_WIDTH = 46f;
    private const float SHINE_ANGLE = -20f;
    private const float SHINE_MASK_INSET = 6f;
    private const string KEYCAP = "input_rectangle";
    private const string STAR = "star";
    private const string STAR_SLOT = "star_outline_depth";
    private const string ICON_PLAY = "icon_play_light";
    private const string ICON_REPEAT = "icon_repeat_light";
    private const string BAR_TRACK = "button_rectangle_border";
    private const string BAR_FILL = "button_rectangle_flat";
    private const float BAR_INSET = 9f;
    private const float PAUSE_BAR_GAP = 12f;
    private const float PAUSE_ICON_LIFT = 4f;
    private const string MARKER = "slide_hangle";
    private const string SLIDER_TRACK = "slide_horizontal_grey";
    private const string SLIDER_FILL = "slide_horizontal_color";
    // A blue disc with a white ring, matching the blue fill.
    private const string SLIDER_KNOB = "check_round_round_circle";
    // Settings rows: a left-aligned label column and a control column, each 50 in from the card's sides.
    private const float SETTINGS_LABEL_X = -210f;
    private const float SETTINGS_LABEL_WIDTH = 280f;
    private const float SETTINGS_CONTROL_X = 160f;
    private const float GEAR_ICON_SIZE = 58f;

    private static readonly Vector2 _referenceResolution = new Vector2(1920f, 1080f);
    private static readonly Vector2 _menuButtonSize = new Vector2(500f, 124f);
    private static readonly Vector2 _bestPillSize = new Vector2(460f, 100f);
    private static readonly Color _darkText = new Color(0.22f, 0.24f, 0.32f);
    private static readonly Color _outlineColor = new Color(0.12f, 0.13f, 0.2f);
    private static readonly Vector2 _hudBarSize = new Vector2(600f, 54f);
    private static readonly Vector2 _hudPillSize = new Vector2(310f, 100f);
    private static readonly Vector2 _formBadgeSize = new Vector2(310f, 96f);
    private static readonly Vector2 _failBarSize = new Vector2(540f, 54f);
    private static readonly Vector2 _pauseBarSize = new Vector2(14f, 44f);
    private static readonly Vector2 _toastSize = new Vector2(860f, 170f);
    private static readonly Vector2 _heartSize = new Vector2(76f, 70f);
    // The rail sprites are 192 x 32 with a 32-wide round cap at each end.
    private static readonly Vector2 _sliderSize = new Vector2(380f, 32f);
    private static readonly Vector4 _railBorder = new Vector4(32f, 0f, 32f, 0f);
    private static readonly Vector2 _sliderHandleSize = new Vector2(60f, 60f);
    private static readonly Vector2 _topEdge = new Vector2(0.5f, 1f);
    private static readonly Vector2 _bottomEdge = new Vector2(0.5f, 0f);
    private static readonly List<Button> _settingsButtons = new List<Button>();
    private static readonly List<OrientationLayout> _portraitLayouts = new List<OrientationLayout>();
    private static readonly Color _shineColor = new Color(1f, 1f, 1f, 0.35f);
    private static readonly Color _dividerTint = new Color(0.7f, 0.73f, 0.82f);
    private static readonly Color _dimColor = new Color(0.05f, 0.07f, 0.15f, 0.55f);
    private static readonly Color _lightDimColor = new Color(0.05f, 0.07f, 0.15f, 0.2f);
    private static readonly Color _emptyStarTint = new Color(0.62f, 0.65f, 0.75f);

    // =================== PRIVATE/PROTECTED FIELDS ====================

    private static TMP_FontAsset _font;
    private static Material _outlineMaterial;

    // ========================= STATIC METHODS ========================

    [MenuItem(MENU_PATH)]
    public static void BuildUI()
    {
        var gameManager = Object.FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("Build the scene first: no GameManager found.");
            return;
        }

        _font = LoadOrCreateFont();
        _outlineMaterial = LoadOrCreateOutlineMaterial();
        PetTestSceneBuilder.DestroyExisting(UI_ROOT_NAME);
        EnsureEventSystem();

        GameObject root = CreateCanvas();
        var manager = root.AddComponent<UIManager>();
        var audioSource = root.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        var managerObject = new SerializedObject(manager);
        Set(managerObject, "_gameManager", gameManager);
        Set(managerObject, "_formController", Object.FindObjectOfType<PetFormController>());
        Set(managerObject, "_audioSource", audioSource);
        Set(managerObject, "_clickClip", AssetDatabase.LoadAssetAtPath<AudioClip>(CLICK_SOUND_PATH));
        Set(managerObject, "_coinClip", AssetDatabase.LoadAssetAtPath<AudioClip>(COIN_SOUND_PATH));
        Set(managerObject, "_starFullSprite", LoadSprite(YELLOW, STAR));
        Set(managerObject, "_starEmptySprite", LoadSprite(YELLOW, STAR_SLOT));
        Set(managerObject, "_checkOnSprite", LoadSprite(GREEN, CHECK_ON));
        Set(managerObject, "_checkOffSprite", LoadSprite(GREY, CHECK_OFF));
        managerObject.FindProperty("_emptyStarColor").colorValue = _emptyStarTint;

        _settingsButtons.Clear();
        _portraitLayouts.Clear();
        BuildHud(root.transform, managerObject);
        BuildStartScreen(root.transform, managerObject);
        BuildPauseScreen(root.transform, managerObject);
        BuildWinScreen(root.transform, managerObject);
        BuildFailScreen(root.transform, managerObject);
        // Built last so it draws over the title and pause screens it opens from.
        BuildSettingsScreen(root.transform, managerObject);
        SetArray(managerObject, "_settingsButtons", _settingsButtons.ToArray());
        managerObject.ApplyModifiedPropertiesWithoutUndo();

        var responsiveObject = new SerializedObject(root.GetComponent<ResponsiveCanvas>());
        SetArray(responsiveObject, "_layouts", _portraitLayouts.ToArray());
        responsiveObject.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(root.scene);
        Debug.Log("UI built.");
    }

    // ----- Screens -----

    private static void BuildHud(Transform root, SerializedObject manager)
    {
        UIScreen screen = CreateScreen("HUD", root, Color.clear, out RectTransform content);
        Set(manager, "_hud", screen);

        IconRenderer.EnsureHeartSprites(out Sprite heartFull, out Sprite heartEmpty);
        Image livesPill = CreatePill("LivesPill", content, TopLeftPosition(_hudPillSize, 0f), _hudPillSize, TextAnchor.UpperLeft);
        var lifeIcons = new Image[3];
        for (int index = 0; index < lifeIcons.Length; index++)
        {
            lifeIcons[index] = CreateImage($"Life_{index}", livesPill.rectTransform, heartFull, new Vector2((index - 1) * 90f, 5f), _heartSize);
        }

        SetArray(manager, "_lifeIcons", lifeIcons);
        Set(manager, "_lifeFullSprite", heartFull);
        Set(manager, "_lifeEmptySprite", heartEmpty);

        BuildFormBadge(content, manager);
        BuildFormToast(content, manager);
        BuildProgressBar(content, manager);

        // Left of the pause button, with the same top gap.
        var coinPillSize = new Vector2(250f, 100f);
        var coinPillPosition = new Vector2(-(EDGE_SIDE_GAP + CORNER_BUTTON_SIZE + HUD_ITEM_GAP + coinPillSize.x * 0.5f), -(EDGE_TOP_GAP + coinPillSize.y * 0.5f));
        Image coinPill = CreatePill("CoinPill", content, coinPillPosition, coinPillSize, TextAnchor.UpperRight);
        Image coinIcon = CreateImage("CoinIcon", coinPill.rectTransform, LoadSprite(YELLOW, ROUND_BUTTON), new Vector2(-70f, 3f), new Vector2(66f, 66f));
        Set(manager, "_coinIcon", coinIcon.rectTransform);
        TMP_Text coinText = CreateText("CoinText", coinPill.rectTransform, "0", 60f, _darkText, new Vector2(50f, 6f), new Vector2(110f, 90f), false);
        coinText.alignment = TextAlignmentOptions.Left;
        Set(manager, "_coinText", coinText);

        Button pause = CreateButton("PauseButton", content, string.Empty, BLUE, ROUND_BUTTON, TopRightCornerPosition(), Vector2.one * CORNER_BUTTON_SIZE, 50f, null, TextAnchor.UpperRight);
        AddPauseIcon((RectTransform)pause.transform);
        Set(manager, "_pauseButton", pause);
    }

    // Current animal, under the hearts.
    private static void BuildFormBadge(RectTransform parent, SerializedObject manager)
    {
        PetFormBuilder.EnsureForms(out PetForm dog, out _, out _, out _, out _);
        Image badge = CreatePill("FormBadge", parent, TopLeftPosition(_formBadgeSize, _hudPillSize.y + HUD_ROW_GAP), _formBadgeSize, TextAnchor.UpperLeft);
        Image icon = CreateImage("Icon", badge.rectTransform, dog.Icon, new Vector2(-100f, 6f), new Vector2(84f, 84f));
        // Between the icon and the badge's right edge; 36 fits the longest name, PENGUIN, at full size.
        TMP_Text name = CreateText("Name", badge.rectTransform, dog.DisplayName, 36f, _darkText, new Vector2(46f, 6f), new Vector2(188f, 70f), false);
        Set(manager, "_formIcon", icon);
        Set(manager, "_formNameText", name);
    }

    // Banner that pops up when a gate changes the animal, naming it and its ability.
    private static void BuildFormToast(RectTransform parent, SerializedObject manager)
    {
        RectTransform toast = CreateRect("FormToast", parent);
        toast.anchoredPosition = new Vector2(0f, 250f);
        toast.sizeDelta = _toastSize;
        var group = toast.gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;

        Image ribbon = CreateImage("Ribbon", toast, LoadSprite(YELLOW, BUTTON), Vector2.zero, _toastSize);
        ribbon.type = Image.Type.Sliced;
        Image icon = CreateImage("Icon", toast, null, new Vector2(-_toastSize.x * 0.5f + 90f, 12f), new Vector2(150f, 150f));
        // Text fills the space right of the icon, 20 in from the ribbon's edge.
        TMP_Text name = CreateText("Name", toast, "BUNNY!", 70f, Color.white, new Vector2(80f, 32f), new Vector2(640f, 90f), true);
        TMP_Text ability = CreateText("Ability", toast, "Super high jump!", 38f, Color.white, new Vector2(80f, -38f), new Vector2(640f, 60f), true);

        Set(manager, "_formToast", group);
        Set(manager, "_toastIcon", icon);
        Set(manager, "_toastNameText", name);
        Set(manager, "_toastAbilityText", ability);
    }

    private static void BuildProgressBar(RectTransform parent, SerializedObject manager)
    {
        // Level with the hearts and coins; the bar's 2-unit drop lines its face up with the pills' faces.
        float barY = TopLeftPosition(_hudPillSize, 0f).y - 2f;
        RectTransform fill = CreateProgressBar("ProgressBar", parent, new Vector2(-20f, barY), _hudBarSize, GREEN, TextAnchor.UpperCenter, out RectTransform track);
        Set(manager, "_progressFill", fill);
        // In portrait the bar moves down beside the animal badge, clear of the hearts and coins.
        float badgeY = TopLeftPosition(_formBadgeSize, _hudPillSize.y + HUD_ROW_GAP).y;
        AddPortraitLayout(track, new Vector2(115f, badgeY), _hudBarSize, 0.9f);

        CreateImage("GoalStar", track, LoadSprite(YELLOW, STAR), new Vector2(_hudBarSize.x * 0.5f + 34f, 4f), new Vector2(66f, 62f));

        Image marker = CreateImage("ProgressMarker", track, LoadSprite(YELLOW, MARKER), Vector2.zero, new Vector2(34f, 46f));
        marker.rectTransform.anchorMin = new Vector2(0f, 0.5f);
        marker.rectTransform.anchorMax = new Vector2(0f, 0.5f);
        marker.rectTransform.anchoredPosition = new Vector2(0f, PROGRESS_MARKER_Y);
        Set(manager, "_progressMarker", marker.rectTransform);
    }

    private static void BuildStartScreen(Transform root, SerializedObject manager)
    {
        UIScreen screen = CreateScreen("StartScreen", root, _lightDimColor, out RectTransform content);
        Set(manager, "_startScreen", screen);

        // Same spot and shape as the HUD's pause button.
        _settingsButtons.Add(CreateGearButton(content, TopRightCornerPosition(), TextAnchor.UpperRight));

        // The title fills a holder that changes shape with the orientation; in portrait it wraps onto two lines.
        RectTransform titleHolder = CreateRect("TitleHolder", content);
        titleHolder.anchoredPosition = new Vector2(0f, 300f);
        titleHolder.sizeDelta = new Vector2(1700f, 200f);
        // In portrait the title and subtitle hang from the top edge and the hints sit on the bottom edge, so a tall
        // phone spreads the menu over the whole screen instead of bunching it in the middle.
        AddPortraitLayout(titleHolder, new Vector2(0f, -430f), new Vector2(PORTRAIT_CONTENT_WIDTH, 360f), 1f, _topEdge);
        TMP_Text title = CreateText("Title", titleHolder, "PET SHIFT RUN", 150f, Color.white, Vector2.zero, Vector2.zero, true);
        Stretch(title.rectTransform);
        FitText(title, 80f, 150f);
        title.enableWordWrapping = true;
        Set(manager, "_title", title.rectTransform);

        Image banner = CreateImage("SubtitleBanner", content, LoadSprite(BLUE, BUTTON), new Vector2(0f, 175f), new Vector2(1180f, 100f));
        banner.type = Image.Type.Sliced;
        AddPortraitLayout(banner.rectTransform, new Vector2(0f, -720f), new Vector2(PORTRAIT_CONTENT_WIDTH, 100f), 1f, _topEdge);
        TMP_Text subtitle = CreateText("Subtitle", banner.rectTransform, "Run, jump and reach the summit flag!", 40f, Color.white, Vector2.zero, Vector2.zero, true);
        Stretch(subtitle.rectTransform);
        subtitle.rectTransform.offsetMin = new Vector2(50f, 5f);
        subtitle.rectTransform.offsetMax = new Vector2(-50f, 5f);
        FitText(subtitle, 24f, 40f);

        // The pulse tween owns the pulse wrapper's scale and the tap effect owns the button's, so a holder around
        // both is what grows in portrait.
        RectTransform playHolder = CreateRect("PlayHolder", content);
        // Best stars in the middle, PLAY just above the control hints so the thumb finds it near the bottom.
        playHolder.anchoredPosition = new Vector2(0f, -245f);
        playHolder.sizeDelta = new Vector2(480f, 150f);
        AddPortraitLayout(playHolder, new Vector2(0f, 400f), playHolder.sizeDelta, PORTRAIT_MENU_SCALE, _bottomEdge);
        RectTransform pulse = CreateRect("PlayPulse", playHolder);
        pulse.sizeDelta = playHolder.sizeDelta;
        Set(manager, "_playButtonPulse", pulse);
        Button play = CreateButton("PlayButton", pulse, "PLAY", GREEN, BUTTON, Vector2.zero, new Vector2(480f, 150f), 76f, LoadSprite(EXTRA, ICON_PLAY), TextAnchor.MiddleCenter, true);
        Set(manager, "_playButton", play);

        // Best stars, with the best score in a matching pill under them.
        Image bestPill = CreatePill("BestPill", content, new Vector2(0f, 20f), _bestPillSize);
        AddPortraitLayout(bestPill.rectTransform, new Vector2(0f, 40f), bestPill.rectTransform.sizeDelta, PORTRAIT_MENU_SCALE);
        CreateText("BestLabel", bestPill.rectTransform, "BEST", 40f, _darkText, new Vector2(-145f, 4f), new Vector2(140f, 60f), false);
        var bestStars = new Image[3];
        for (int index = 0; index < bestStars.Length; index++)
        {
            bestStars[index] = CreateImage($"BestStar_{index}", bestPill.rectTransform, LoadSprite(YELLOW, STAR_SLOT), new Vector2(25f + index * 78f, 4f), new Vector2(66f, 62f));
        }

        SetArray(manager, "_bestStars", bestStars);

        var scorePillSize = new Vector2(_bestPillSize.x, 80f);
        Image scorePill = CreatePill("BestScorePill", content, new Vector2(0f, -75f), scorePillSize);
        AddPortraitLayout(scorePill.rectTransform, new Vector2(0f, -90f), scorePillSize, PORTRAIT_MENU_SCALE);
        float pillInnerHalf = scorePillSize.x * 0.5f - PILL_TEXT_PADDING;
        TMP_Text scoreLabel = CreateText("Label", scorePill.rectTransform, "BEST SCORE", 30f, _darkText, new Vector2(-pillInnerHalf + 130f, 3f), new Vector2(260f, 60f), false);
        scoreLabel.alignment = TextAlignmentOptions.Left;
        TMP_Text bestScore = CreateText("Value", scorePill.rectTransform, "0", 40f, _darkText, new Vector2(pillInnerHalf - 75f, 3f), new Vector2(150f, 60f), false);
        bestScore.alignment = TextAlignmentOptions.Right;
        Set(manager, "_bestScoreText", bestScore);
        Set(manager, "_keyboardHints", BuildKeyboardHints(content));
        Set(manager, "_touchHints", BuildTouchHints(content));
    }

    private static GameObject BuildKeyboardHints(RectTransform parent)
    {
        RectTransform row = CreateHintRow("KeyboardHints", parent);
        CreateIconKeycap(row, LoadSprite(BLUE, ARROW_LEFT));
        CreateIconKeycap(row, LoadSprite(BLUE, ARROW_RIGHT));
        CreateHintLabel(row, "STEER", HINT_GROUP_SPACING);
        CreateKeycap(row, "SPACE", 200f);
        CreateHintLabel(row, "JUMP", HINT_GROUP_SPACING);
        CreateIconKeycap(row, LoadSprite(BLUE, ARROW_DOWN));
        CreateHintLabel(row, "ROLL", HINT_GROUP_SPACING);
        CreateKeycap(row, "ESC", 120f);
        CreateHintLabel(row, "PAUSE", 0f);
        FitHintRowInPortrait(row);
        return row.gameObject;
    }

    private static GameObject BuildTouchHints(RectTransform parent)
    {
        RectTransform row = CreateHintRow("TouchHints", parent);
        CreateHintLabel(row, "SWIPE", HINT_GROUP_SPACING * 0.5f);
        CreateIconKeycap(row, LoadSprite(BLUE, ARROW_LEFT));
        CreateIconKeycap(row, LoadSprite(BLUE, ARROW_RIGHT));
        CreateHintLabel(row, "STEER", HINT_GROUP_SPACING);
        CreateIconKeycap(row, LoadSprite(BLUE, ARROW_UP));
        CreateHintLabel(row, "JUMP", HINT_GROUP_SPACING);
        CreateIconKeycap(row, LoadSprite(BLUE, ARROW_DOWN));
        CreateHintLabel(row, "ROLL", 0f);
        FitHintRowInPortrait(row);
        return row.gameObject;
    }

    // Shrinks the row just enough to fit a portrait screen's width.
    private static void FitHintRowInPortrait(RectTransform row)
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(row);
        float contentWidth = LayoutUtility.GetPreferredWidth(row);
        float scale = Mathf.Min(1f, PORTRAIT_CONTENT_WIDTH / contentWidth);
        AddPortraitLayout(row, new Vector2(0f, 230f), row.sizeDelta, scale, _bottomEdge);
    }

    private static RectTransform CreateHintRow(string name, RectTransform parent)
    {
        RectTransform row = CreateRect(name, parent);
        row.anchoredPosition = new Vector2(0f, -390f);
        row.sizeDelta = new Vector2(1700f, 90f);

        var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 16f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        return row;
    }

    private static void BuildPauseScreen(Transform root, SerializedObject manager)
    {
        UIScreen screen = CreateScreen("PauseScreen", root, _dimColor, out RectTransform content);
        Set(manager, "_pauseScreen", screen);
        RectTransform card = CreateCard(content, BLUE, "PAUSED", new Vector2(700f, 760f));

        Set(manager, "_resumeButton", CreateButton("ResumeButton", card, "RESUME", GREEN, BUTTON, new Vector2(0f, 175f), _menuButtonSize, 54f, LoadSprite(EXTRA, ICON_PLAY)));
        Set(manager, "_pauseRestartButton", CreateButton("RestartButton", card, "RESTART", YELLOW, BUTTON, new Vector2(0f, 35f), _menuButtonSize, 54f, LoadSprite(EXTRA, ICON_REPEAT)));
        Set(manager, "_pauseHomeButton", CreateButton("HomeButton", card, "HOME", BLUE, BUTTON, new Vector2(0f, -105f), _menuButtonSize, 54f, LoadSprite(GREY, ARROW_LEFT)));
        CreateDivider(card, new Vector2(0f, -200f), 560f);
        _settingsButtons.Add(CreateGearButton(card, new Vector2(0f, -290f), TextAnchor.MiddleCenter));
    }

    // Overlay with the volume sliders, the vibration toggle and a Done button.
    private static void BuildSettingsScreen(Transform root, SerializedObject manager)
    {
        UIScreen screen = CreateScreen("SettingsScreen", root, _dimColor, out RectTransform content);
        RectTransform card = CreateCard(content, BLUE, "SETTINGS", new Vector2(800f, 620f));

        // Not "SOUND FX": in Kenney Future the X reads as an H at this size.
        CreateSettingsLabel(card, "EFFECTS", 150f);
        Slider sfxSlider = CreateSlider("SfxSlider", card, new Vector2(SETTINGS_CONTROL_X, 150f));
        CreateSettingsLabel(card, "MUSIC", 45f);
        Slider bgmSlider = CreateSlider("MusicSlider", card, new Vector2(SETTINGS_CONTROL_X, 45f));

        RectTransform vibrationRow = CreateRect("VibrationRow", card);
        vibrationRow.anchoredPosition = new Vector2(0f, -60f);
        vibrationRow.sizeDelta = new Vector2(700f, 90f);
        CreateSettingsLabel(vibrationRow, "VIBRATION", 0f);
        Image vibrationCheck = CreateImage("VibrationToggle", vibrationRow, LoadSprite(GREEN, CHECK_ON), new Vector2(SETTINGS_CONTROL_X - _sliderSize.x * 0.5f + 36f, 0f), new Vector2(72f, 72f));
        vibrationCheck.raycastTarget = true;
        var vibrationButton = vibrationCheck.gameObject.AddComponent<Button>();
        vibrationButton.navigation = new Navigation { mode = Navigation.Mode.None };
        vibrationCheck.gameObject.AddComponent<TapEffect>();

        CreateDivider(card, new Vector2(0f, -140f), 640f);
        Button close = CreateButton("DoneButton", card, "DONE", GREEN, BUTTON, new Vector2(0f, -225f), new Vector2(480f, 110f), 54f, null);

        var panel = screen.gameObject.AddComponent<SettingsPanel>();
        var panelObject = new SerializedObject(panel);
        Set(panelObject, "_screen", screen);
        Set(panelObject, "_sfxSlider", sfxSlider);
        Set(panelObject, "_bgmSlider", bgmSlider);
        Set(panelObject, "_vibrationRow", vibrationRow.gameObject);
        Set(panelObject, "_vibrationButton", vibrationButton);
        Set(panelObject, "_vibrationCheck", vibrationCheck);
        Set(panelObject, "_checkOnSprite", LoadSprite(GREEN, CHECK_ON));
        Set(panelObject, "_checkOffSprite", LoadSprite(GREY, CHECK_OFF));
        Set(panelObject, "_closeButton", close);
        Set(panelObject, "_gameAudio", Object.FindObjectOfType<GameAudio>());
        panelObject.ApplyModifiedPropertiesWithoutUndo();
        Set(manager, "_settingsPanel", panel);
    }

    private static void BuildWinScreen(Transform root, SerializedObject manager)
    {
        UIScreen screen = CreateScreen("WinScreen", root, _dimColor, out RectTransform content);
        Set(manager, "_winScreen", screen);
        RectTransform card = CreateCard(content, YELLOW, "COURSE CLEAR!", new Vector2(820f, 900f));
        CreateInset(card, new Vector2(0f, 250f), new Vector2(700f, 240f));

        var stars = new Image[3];
        for (int index = 0; index < stars.Length; index++)
        {
            bool isMiddle = index == 1;
            float size = isMiddle ? 175f : 145f;
            var position = new Vector2((index - 1) * 200f, isMiddle ? 262f : 238f);
            Image slot = CreateImage($"StarSlot_{index}", card, LoadSprite(YELLOW, STAR_SLOT), position, new Vector2(size, size * 0.94f));
            slot.color = _emptyStarTint;
            stars[index] = CreateImage("Star", slot.rectTransform, LoadSprite(YELLOW, STAR), Vector2.zero, new Vector2(size, size * 0.94f));
        }

        SetArray(manager, "_winStars", stars);

        Image badge = CreateImage("NewBestBadge", card, LoadSprite(RED, BUTTON), new Vector2(290f, 370f), new Vector2(250f, 72f));
        badge.type = Image.Type.Sliced;
        badge.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -10f);
        CreateText("Label", badge.rectTransform, "NEW BEST!", 34f, Color.white, new Vector2(0f, 3f), new Vector2(250f, 72f), true);
        Set(manager, "_newBestBadge", badge.rectTransform);

        var checks = new Image[3];
        checks[0] = CreateObjective(card, "REACH THE FLAG", new Vector2(0f, 82f), out _);
        checks[1] = CreateObjective(card, "COLLECT COINS", new Vector2(0f, 20f), out TMP_Text coinObjective);
        checks[2] = CreateObjective(card, "LOSE NO LIVES", new Vector2(0f, -42f), out _);
        SetArray(manager, "_objectiveChecks", checks);
        Set(manager, "_coinObjectiveText", coinObjective);

        Set(manager, "_winCoinText", CreateResultRow(card, new Vector2(0f, -115f), out TMP_Text winScore));
        Set(manager, "_winScoreText", winScore);
        CreateDivider(card, new Vector2(0f, -175f), 640f);
        Set(manager, "_winRetryButton", CreateButton("PlayAgainButton", card, "PLAY AGAIN", GREEN, BUTTON, new Vector2(0f, -250f), new Vector2(540f, 105f), 50f, LoadSprite(EXTRA, ICON_REPEAT), TextAnchor.MiddleCenter, true));
        Set(manager, "_winHomeButton", CreateButton("HomeButton", card, "HOME", BLUE, BUTTON, new Vector2(0f, -368f), new Vector2(540f, 105f), 50f, LoadSprite(GREY, ARROW_LEFT)));
    }

    private static void BuildFailScreen(Transform root, SerializedObject manager)
    {
        UIScreen screen = CreateScreen("FailScreen", root, _dimColor, out RectTransform content);
        Set(manager, "_failScreen", screen);
        RectTransform card = CreateCard(content, RED, "OUT OF LIVES", new Vector2(820f, 780f));
        CreateInset(card, new Vector2(0f, 110f), new Vector2(700f, 300f));
        // Midway between the inset's bottom edge (-40) and the top of the Retry button (-73).
        CreateDivider(card, new Vector2(0f, -57f), 640f);

        // 40 in from each side of the inset; the longer messages shrink a little to fit.
        Set(manager, "_failMessageText", CreateText("Message", card, "So close! Try again.", 40f, _darkText, new Vector2(0f, 215f), new Vector2(620f, 70f), false));
        CreateText("ProgressLabel", card, "HOW FAR YOU GOT", 32f, _darkText, new Vector2(0f, 150f), new Vector2(620f, 50f), false);
        RectTransform failFill = CreateProgressBar("ProgressBar", card, new Vector2(-25f, 98f), _failBarSize, RED, TextAnchor.MiddleCenter, out RectTransform failTrack);
        Set(manager, "_failProgressFill", failFill);
        CreateImage("GoalStar", failTrack, LoadSprite(YELLOW, STAR), new Vector2(_failBarSize.x * 0.5f + 32f, 4f), new Vector2(62f, 58f));
        Set(manager, "_failCoinText", CreateResultRow(card, new Vector2(0f, 20f), out TMP_Text failScore));
        Set(manager, "_failScoreText", failScore);
        Set(manager, "_failRetryButton", CreateButton("RetryButton", card, "RETRY", GREEN, BUTTON, new Vector2(0f, -135f), new Vector2(540f, 124f), 54f, LoadSprite(EXTRA, ICON_REPEAT), TextAnchor.MiddleCenter, true));
        Set(manager, "_failHomeButton", CreateButton("HomeButton", card, "HOME", BLUE, BUTTON, new Vector2(0f, -270f), new Vector2(540f, 124f), 54f, LoadSprite(GREY, ARROW_LEFT)));
    }

    // ----- Composite elements -----

    private static RectTransform CreateCard(RectTransform parent, string color, string title, Vector2 size)
    {
        Image card = CreateImage("Card", parent, LoadSprite(color, CARD), Vector2.zero, size);
        card.type = Image.Type.Sliced;
        card.raycastTarget = true;

        float ribbonWidth = Mathf.Min(size.x - 60f, 660f);
        Image ribbon = CreateImage("Ribbon", card.rectTransform, LoadSprite(color, BUTTON), new Vector2(0f, size.y * 0.5f), new Vector2(ribbonWidth, 130f));
        ribbon.type = Image.Type.Sliced;
        TMP_Text titleText = CreateText("Title", ribbon.rectTransform, title, 64f, Color.white, new Vector2(0f, 6f), new Vector2(ribbonWidth - RIBBON_TEXT_PADDING, 130f), true);
        titleText.enableAutoSizing = true;
        titleText.fontSizeMin = RIBBON_MIN_FONT_SIZE;
        titleText.fontSizeMax = 64f;
        return card.rectTransform;
    }

    // A checkbox and its label, as one block centred on the position, so the list sits in the middle of the card.
    private static Image CreateObjective(RectTransform parent, string label, Vector2 position, out TMP_Text labelText)
    {
        float blockLeft = -(OBJECTIVE_CHECK_SIZE + OBJECTIVE_GAP + OBJECTIVE_TEXT_WIDTH) * 0.5f;
        float checkX = blockLeft + OBJECTIVE_CHECK_SIZE * 0.5f;
        float textX = blockLeft + OBJECTIVE_CHECK_SIZE + OBJECTIVE_GAP + OBJECTIVE_TEXT_WIDTH * 0.5f;
        Image check = CreateImage($"Check_{label}", parent, LoadSprite(GREY, CHECK_OFF), position + new Vector2(checkX, 0f), Vector2.one * OBJECTIVE_CHECK_SIZE);
        labelText = CreateText($"Objective_{label}", parent, label, 36f, _darkText, position + new Vector2(textX, 2f), new Vector2(OBJECTIVE_TEXT_WIDTH, 60f), false);
        labelText.alignment = TextAlignmentOptions.Left;
        return check;
    }

    private static void CreateSettingsLabel(RectTransform parent, string label, float y)
    {
        TMP_Text text = CreateText($"Label_{label}", parent, label, 40f, _darkText, new Vector2(SETTINGS_LABEL_X, y + 4f), new Vector2(SETTINGS_LABEL_WIDTH, 70f), false);
        text.alignment = TextAlignmentOptions.Left;
    }

    // A UI Pack slider from 0 to 1: a grey rail, a blue rail up to the value and a round blue knob to drag.
    private static Slider CreateSlider(string name, RectTransform parent, Vector2 position)
    {
        Image track = CreateRail(name, parent, GREY, SLIDER_TRACK);
        track.rectTransform.anchoredPosition = position;
        track.rectTransform.sizeDelta = _sliderSize;
        track.raycastTarget = true;

        // The handle and the fill's area are both inset by half a knob, so the knob stops at the rail's ends and the
        // fill's right end always sits under the knob's centre. The fill reaches back out to the rail's left end.
        float knobInset = _sliderHandleSize.x * 0.5f;
        RectTransform fillArea = CreateRect("FillArea", track.rectTransform);
        Stretch(fillArea);
        fillArea.offsetMin = new Vector2(knobInset, 0f);
        fillArea.offsetMax = new Vector2(-knobInset, 0f);
        Image fill = CreateRail("Fill", fillArea, BLUE, SLIDER_FILL);
        Stretch(fill.rectTransform);
        fill.rectTransform.offsetMin = new Vector2(-knobInset, 0f);

        RectTransform handleArea = CreateRect("HandleArea", track.rectTransform);
        Stretch(handleArea);
        handleArea.offsetMin = new Vector2(knobInset, 0f);
        handleArea.offsetMax = new Vector2(-knobInset, 0f);
        // The slider stretches the handle to the rail's height, so its size here is what it adds to that height.
        Image handle = CreateImage("Knob", handleArea, LoadSprite(BLUE, SLIDER_KNOB), Vector2.zero, new Vector2(_sliderHandleSize.x, _sliderHandleSize.y - _sliderSize.y));
        handle.raycastTarget = true;

        var slider = track.gameObject.AddComponent<Slider>();
        slider.fillRect = fill.rectTransform;
        slider.handleRect = handle.rectTransform;
        slider.targetGraphic = handle;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        slider.navigation = new Navigation { mode = Navigation.Mode.None };
        return slider;
    }

    // The UI Pack's slider rail: a thin line with a round cap at each end. Only the line stretches, so the caps
    // stay round, and it is drawn at the sprite's own pixel size so the rail keeps its native thickness.
    private static Image CreateRail(string name, RectTransform parent, string color, string spriteName)
    {
        string path = $"{KENNEY_UI}/PNG/{color}/{SPRITE_SIZE_FOLDER}/{spriteName}.png";
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer.spriteBorder != _railBorder)
        {
            importer.spriteBorder = _railBorder;
            importer.SaveAndReimport();
        }

        Image rail = CreateImage(name, parent, AssetDatabase.LoadAssetAtPath<Sprite>(path), Vector2.zero, Vector2.zero);
        rail.type = Image.Type.Sliced;
        rail.pixelsPerUnitMultiplier = 1f;
        return rail;
    }

    // Round blue button the size of the HUD's pause button, with a gear centred on its face like the pause bars.
    private static Button CreateGearButton(RectTransform parent, Vector2 position, TextAnchor anchor)
    {
        Button button = CreateButton("SettingsButton", parent, string.Empty, BLUE, ROUND_BUTTON, position, Vector2.one * CORNER_BUTTON_SIZE, 50f, null, anchor);
        Image gear = CreateImage("Gear", (RectTransform)button.transform, IconRenderer.EnsureGearSprite(), new Vector2(0f, PAUSE_ICON_LIFT), Vector2.one * GEAR_ICON_SIZE);
        var shadow = gear.gameObject.AddComponent<Shadow>();
        shadow.effectColor = _outlineColor;
        shadow.effectDistance = new Vector2(0f, -3f);
        return button;
    }

    // Coin icon and count, then the run's score; the layout group keeps the row centred as the numbers change width.
    private static TMP_Text CreateResultRow(RectTransform parent, Vector2 position, out TMP_Text scoreText)
    {
        RectTransform row = CreateRect("ResultRow", parent);
        row.anchoredPosition = position;
        row.sizeDelta = new Vector2(660f, 80f);
        var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = COIN_ROW_GAP;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        Image icon = CreateImage("CoinIcon", row, LoadSprite(YELLOW, ROUND_BUTTON), Vector2.zero, Vector2.one * RESULT_COIN_SIZE);
        icon.gameObject.AddComponent<LayoutElement>().preferredWidth = RESULT_COIN_SIZE;
        TMP_Text coinText = CreateRowText("Coins", row, "0", 56f);

        RectTransform spacer = CreateRect("Spacer", row);
        spacer.gameObject.AddComponent<LayoutElement>().preferredWidth = RESULT_GROUP_GAP - COIN_ROW_GAP * 2f;

        CreateRowText("ScoreLabel", row, "SCORE", 36f);
        scoreText = CreateRowText("Score", row, "0", 56f);
        return coinText;
    }

    // Text in a layout row, which sizes it to its words, so it never needs to shrink.
    private static TMP_Text CreateRowText(string name, RectTransform row, string value, float fontSize)
    {
        TMP_Text text = CreateText(name, row, value, fontSize, _darkText, Vector2.zero, new Vector2(200f, 80f), false);
        text.enableAutoSizing = false;
        text.alignment = TextAlignmentOptions.Left;
        return text;
    }

    // A recessed track with a full-size fill revealed by a clipping mask, so the fill's corners never squash.
    // The returned mask's anchorMax.x is the bar's value (0 to 1).
    private static RectTransform CreateProgressBar(string name, RectTransform parent, Vector2 position, Vector2 size, string fillColor, TextAnchor anchor, out RectTransform track)
    {
        Image trackImage = CreateImage(name, parent, LoadSprite(GREY, BAR_TRACK), position, size, anchor);
        trackImage.type = Image.Type.Sliced;
        track = trackImage.rectTransform;

        RectTransform mask = CreateRect("FillMask", track);
        mask.anchorMin = Vector2.zero;
        mask.anchorMax = new Vector2(0f, 1f);
        mask.pivot = new Vector2(0f, 0.5f);
        mask.offsetMin = new Vector2(BAR_INSET, BAR_INSET);
        mask.offsetMax = new Vector2(BAR_INSET, -BAR_INSET);
        mask.gameObject.AddComponent<RectMask2D>();

        var innerSize = new Vector2(size.x - BAR_INSET * 2f, size.y - BAR_INSET * 2f);
        Image fill = CreateImage("Fill", mask, LoadSprite(fillColor, BAR_FILL), Vector2.zero, innerSize);
        fill.type = Image.Type.Sliced;
        fill.rectTransform.anchorMin = new Vector2(0f, 0.5f);
        fill.rectTransform.anchorMax = new Vector2(0f, 0.5f);
        fill.rectTransform.pivot = new Vector2(0f, 0.5f);
        fill.rectTransform.anchoredPosition = Vector2.zero;
        return mask;
    }

    // Two bars centred on the round button's face (the face sits slightly above centre because of the depth edge).
    private static void AddPauseIcon(RectTransform button)
    {
        foreach (float side in new[] { -1f, 1f })
        {
            Image bar = CreateImage("PauseBar", button, null, new Vector2(side * PAUSE_BAR_GAP, PAUSE_ICON_LIFT), _pauseBarSize);
            bar.color = Color.white;
            var shadow = bar.gameObject.AddComponent<Shadow>();
            shadow.effectColor = _outlineColor;
            shadow.effectDistance = new Vector2(0f, -3f);
        }
    }

    private static Image CreatePill(string name, RectTransform parent, Vector2 position, Vector2 size, TextAnchor anchor = TextAnchor.MiddleCenter)
    {
        Image pill = CreateImage(name, parent, LoadSprite(GREY, PILL), position, size, anchor);
        pill.type = Image.Type.Sliced;
        return pill;
    }

    // Recessed box that groups related content inside a card.
    private static void CreateInset(RectTransform parent, Vector2 position, Vector2 size)
    {
        Image inset = CreateImage("Inset", parent, LoadSprite(EXTRA, INSET), position, size);
        inset.type = Image.Type.Sliced;
    }

    private static void CreateDivider(RectTransform parent, Vector2 position, float width)
    {
        Image divider = CreateImage("Divider", parent, LoadSprite(EXTRA, DIVIDER), position, new Vector2(width, 8f));
        divider.type = Image.Type.Sliced;
        divider.color = _dividerTint;
    }

    // A soft light streak that sweeps across the button on a loop, clipped to the button.
    private static void AddShine(RectTransform button, Vector2 size)
    {
        RectTransform mask = CreateRect("ShineMask", button);
        Stretch(mask);
        mask.offsetMin = new Vector2(SHINE_MASK_INSET, SHINE_MASK_INSET * 2f);
        mask.offsetMax = new Vector2(-SHINE_MASK_INSET, -SHINE_MASK_INSET);
        mask.gameObject.AddComponent<RectMask2D>();

        float startX = -size.x * 0.5f - SHINE_WIDTH;
        Image shine = CreateImage("Shine", mask, null, new Vector2(startX, 0f), new Vector2(SHINE_WIDTH, size.y * 2f));
        shine.color = _shineColor;
        shine.rectTransform.localRotation = Quaternion.Euler(0f, 0f, SHINE_ANGLE);

        RectTransform target = CreateRect("ShineTarget", mask);
        target.anchoredPosition = new Vector2(-startX, 0f);

        var shineEffect = button.gameObject.AddComponent<ShineEffect>();
        var serializedShine = new SerializedObject(shineEffect);
        Set(serializedShine, "shine", shine.rectTransform);
        Set(serializedShine, "target", target);
        serializedShine.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateKeycap(RectTransform row, string key, float width)
    {
        Image keycap = CreateImage($"Key_{key}", row, LoadSprite(EXTRA, KEYCAP), Vector2.zero, new Vector2(width, 80f));
        keycap.type = Image.Type.Sliced;
        keycap.gameObject.AddComponent<LayoutElement>().preferredWidth = width;
        CreateText("Label", keycap.rectTransform, key, 34f, _darkText, new Vector2(0f, 2f), new Vector2(width, 80f), false);
    }

    private static void CreateIconKeycap(RectTransform row, Sprite icon)
    {
        Image keycap = CreateImage("Key_Arrow", row, LoadSprite(EXTRA, KEYCAP), Vector2.zero, new Vector2(KEYCAP_SIZE, KEYCAP_SIZE));
        keycap.type = Image.Type.Sliced;
        keycap.gameObject.AddComponent<LayoutElement>().preferredWidth = KEYCAP_SIZE;
        float iconHeight = KEYCAP_SIZE * 0.45f;
        CreateImage("Icon", keycap.rectTransform, icon, new Vector2(0f, 3f), new Vector2(iconHeight * icon.rect.width / icon.rect.height, iconHeight));
    }

    private static void CreateHintLabel(RectTransform row, string label, float trailingSpace)
    {
        TMP_Text text = CreateText($"Hint_{label}", row, label, 38f, Color.white, Vector2.zero, new Vector2(200f, 80f), true);
        // Sized to its text, and the whole row scales to fit, so the label itself never shrinks.
        text.enableAutoSizing = false;
        text.GetComponent<RectTransform>().sizeDelta = new Vector2(text.preferredWidth, 80f);

        if (trailingSpace <= 0f) return;

        RectTransform spacer = CreateRect("Spacer", row);
        spacer.gameObject.AddComponent<LayoutElement>().preferredWidth = trailingSpace;
    }

    // ----- Elements -----

    private static GameObject CreateCanvas()
    {
        var root = new GameObject(UI_ROOT_NAME, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(root, MENU_PATH);

        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        var scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = _referenceResolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

        root.AddComponent<ResponsiveCanvas>();
        root.AddComponent<GraphicRaycaster>();
        return root;
    }

    // Centre of an upper-left-anchored element, set in from the left edge and the top edge, plus any rows above it.
    private static Vector2 TopLeftPosition(Vector2 size, float rowsAbove)
    {
        return new Vector2(EDGE_SIDE_GAP + size.x * 0.5f, -(EDGE_TOP_GAP + rowsAbove + size.y * 0.5f));
    }

    // Centre of the round corner button (pause or settings), anchored upper right.
    private static Vector2 TopRightCornerPosition()
    {
        return new Vector2(-(EDGE_SIDE_GAP + CORNER_BUTTON_SIZE * 0.5f), -(EDGE_TOP_GAP + CORNER_BUTTON_SIZE * 0.5f));
    }

    // Gives an element its own position, size and scale for portrait screens.
    private static OrientationLayout AddPortraitLayout(RectTransform rect, Vector2 position, Vector2 size, float scale)
    {
        var layout = rect.gameObject.AddComponent<OrientationLayout>();
        layout.Configure(position, size, scale);
        _portraitLayouts.Add(layout);
        return layout;
    }

    // Same, measured from a screen edge in portrait (top or bottom), so the element follows that edge on tall phones.
    private static void AddPortraitLayout(RectTransform rect, Vector2 position, Vector2 size, float scale, Vector2 anchor)
    {
        AddPortraitLayout(rect, position, size, scale).ConfigureAnchor(anchor);
    }

    private static void FitText(TMP_Text text, float minSize, float maxSize)
    {
        text.enableAutoSizing = true;
        text.fontSizeMin = minSize;
        text.fontSizeMax = maxSize;
    }

    private static UIScreen CreateScreen(string name, Transform parent, Color dimColor, out RectTransform content)
    {
        RectTransform screenRect = CreateRect(name, parent);
        Stretch(screenRect);
        screenRect.gameObject.AddComponent<CanvasGroup>();

        if (dimColor.a > 0f)
        {
            RectTransform dim = CreateRect("Dim", screenRect);
            Stretch(dim);
            dim.gameObject.AddComponent<Image>().color = dimColor;
        }

        content = CreateRect("Content", screenRect);
        Stretch(content);

        var screen = screenRect.gameObject.AddComponent<UIScreen>();
        var serializedScreen = new SerializedObject(screen);
        Set(serializedScreen, "_content", content);
        serializedScreen.ApplyModifiedPropertiesWithoutUndo();
        return screen;
    }

    private static Button CreateButton(string name, RectTransform parent, string label, string color, string spriteName, Vector2 position, Vector2 size, float fontSize, Sprite icon, TextAnchor anchor = TextAnchor.MiddleCenter, bool hasShine = false)
    {
        Image image = CreateImage(name, parent, LoadSprite(color, spriteName), position, size, anchor);
        image.type = Image.Type.Sliced;
        image.raycastTarget = true;

        var button = image.gameObject.AddComponent<Button>();
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        image.gameObject.AddComponent<TapEffect>();

        if (hasShine)
        {
            AddShine(image.rectTransform, size);
        }

        // The label fills the face between the icon (if any) and the right end, and is centred in that space,
        // so a long word shrinks instead of running under the icon.
        float labelLeft = -size.x * 0.5f + size.y * BUTTON_TEXT_PADDING;
        float labelRight = size.x * 0.5f - size.y * BUTTON_TEXT_PADDING;
        if (icon != null)
        {
            float iconHeight = size.y * 0.42f;
            float iconWidth = iconHeight * icon.rect.width / icon.rect.height;
            float iconX = -size.x * 0.5f + size.y * 0.55f;
            CreateImage("Icon", image.rectTransform, icon, new Vector2(iconX, 5f), new Vector2(iconWidth, iconHeight));
            labelLeft = iconX + iconWidth * 0.5f + size.y * BUTTON_ICON_GAP;
        }

        var labelSize = new Vector2(labelRight - labelLeft, size.y);
        CreateText("Label", image.rectTransform, label, fontSize, Color.white, new Vector2((labelLeft + labelRight) * 0.5f, 6f), labelSize, true);
        return button;
    }

    private static Image CreateImage(string name, RectTransform parent, Sprite sprite, Vector2 position, Vector2 size, TextAnchor anchor = TextAnchor.MiddleCenter)
    {
        RectTransform rect = CreateRect(name, parent);
        Anchor(rect, anchor);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        var image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.pixelsPerUnitMultiplier = DOUBLE_SPRITE_PPU_MULTIPLIER;
        image.raycastTarget = false;
        return image;
    }

    private static TMP_Text CreateText(string name, RectTransform parent, string value, float fontSize, Color color, Vector2 position, Vector2 size, bool hasOutline, TextAnchor anchor = TextAnchor.MiddleCenter)
    {
        RectTransform rect = CreateRect(name, parent);
        Anchor(rect, anchor);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = _font;
        text.fontSize = fontSize;
        text.color = color;
        text.text = value;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = false;
        text.raycastTarget = false;
        FitText(text, fontSize * MIN_FONT_SCALE, fontSize);

        if (hasOutline)
        {
            text.fontSharedMaterial = _outlineMaterial;
        }

        return text;
    }

    private static RectTransform CreateRect(string name, Transform parent)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void Anchor(RectTransform rect, TextAnchor anchor)
    {
        Vector2 point = anchor switch
        {
            TextAnchor.UpperLeft => new Vector2(0f, 1f),
            TextAnchor.UpperCenter => new Vector2(0.5f, 1f),
            TextAnchor.UpperRight => new Vector2(1f, 1f),
            _ => new Vector2(0.5f, 0.5f),
        };

        rect.anchorMin = point;
        rect.anchorMax = point;
        rect.pivot = new Vector2(0.5f, 0.5f);
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() != null) return;

        var eventSystem = new GameObject(EVENT_SYSTEM_NAME, typeof(EventSystem), typeof(InputSystemUIInputModule));
        Undo.RegisterCreatedObjectUndo(eventSystem, MENU_PATH);
    }

    // ----- Assets -----

    private static Sprite LoadSprite(string color, string spriteName)
    {
        string path = $"{KENNEY_UI}/PNG/{color}/{SPRITE_SIZE_FOLDER}/{spriteName}.png";
        EnsureSliceBorder(path);
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static void EnsureSliceBorder(string path)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer.spriteBorder != Vector4.zero) return;

        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        float border = Mathf.Round(Mathf.Min(texture.width, texture.height) * SLICE_BORDER_RATIO);
        importer.spriteBorder = new Vector4(border, border, border, border);
        importer.SaveAndReimport();
    }

    private static TMP_FontAsset LoadOrCreateFont()
    {
        var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_ASSET_PATH);
        if (fontAsset != null) return fontAsset;

        if (!AssetDatabase.IsValidFolder(FONT_FOLDER))
        {
            AssetDatabase.CreateFolder("Assets/_Project/UI", "Fonts");
        }

        var sourceFont = AssetDatabase.LoadAssetAtPath<Font>(FONT_PATH);
        fontAsset = TMP_FontAsset.CreateFontAsset(sourceFont, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, true);
        fontAsset.name = "KenneyFuture SDF";
        AssetDatabase.CreateAsset(fontAsset, FONT_ASSET_PATH);

        fontAsset.atlasTextures[0].name = "KenneyFuture Atlas";
        AssetDatabase.AddObjectToAsset(fontAsset.atlasTextures[0], fontAsset);
        fontAsset.material.name = "KenneyFuture Material";
        AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);

        fontAsset.TryAddCharacters(PRELOADED_CHARACTERS);
        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.SaveAssets();
        return fontAsset;
    }

    private static Material LoadOrCreateOutlineMaterial()
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(OUTLINE_MATERIAL_PATH);
        if (material != null) return material;

        material = new Material(_font.material);
        material.EnableKeyword(ShaderUtilities.Keyword_Outline);
        material.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.2f);
        material.SetColor(ShaderUtilities.ID_OutlineColor, _outlineColor);
        material.SetFloat(ShaderUtilities.ID_FaceDilate, 0.15f);
        AssetDatabase.CreateAsset(material, OUTLINE_MATERIAL_PATH);
        return material;
    }

    // ----- Serialized wiring -----

    private static void Set(SerializedObject target, string property, Object value)
    {
        target.FindProperty(property).objectReferenceValue = value;
    }

    private static void SetArray(SerializedObject target, string property, Object[] values)
    {
        SerializedProperty array = target.FindProperty(property);
        array.arraySize = values.Length;
        for (int index = 0; index < values.Length; index++)
        {
            array.GetArrayElementAtIndex(index).objectReferenceValue = values[index];
        }
    }
}
