using System.Collections.Generic;
using MuseumGetaway;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class BuildMuseumGetawayMainMenu
{
    private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
    private const string GameplayScenePath = "Assets/Scenes/MuseumGetaway.unity";
    private const string MaterialFolder = "Assets/Art/Materials";

    [MenuItem("Tools/Build Museum Getaway Main Menu")]
    public static void BuildMenu()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        if (!Application.isBatchMode && AssetDatabase.LoadAssetAtPath<SceneAsset>(MainMenuScenePath) != null)
        {
            bool rebuild = EditorUtility.DisplayDialog(
                "Rebuild Museum Getaway Main Menu?",
                "This replaces only MainMenu.unity. The MuseumGetaway gameplay scene will not be modified.",
                "Rebuild Menu",
                "Cancel");
            if (!rebuild)
            {
                return;
            }
        }

        EnsureFolder("Assets/Scenes");
        EnsureFolder(MaterialFolder);

        Material wallMaterial = CreateMaterial("Menu Dark Museum Wall", new Color(0.025f, 0.055f, 0.12f));
        Material floorMaterial = CreateMaterial("Menu Museum Floor", new Color(0.055f, 0.075f, 0.12f));
        Material columnMaterial = CreateMaterial("Menu Classical Columns", new Color(0.42f, 0.48f, 0.62f));
        Material pedestalMaterial = CreateMaterial("Menu Exhibit Pedestals", new Color(0.06f, 0.12f, 0.22f));
        Material artifactMaterial = CreateMaterial("Menu Artifact Glow", new Color(1f, 0.48f, 0.07f), true);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        BuildBackground(wallMaterial, floorMaterial, columnMaterial, pedestalMaterial, artifactMaterial);
        BuildInterface();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, MainMenuScenePath);
        UpdateBuildSettingsSceneOrder();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(MainMenuScenePath);
        Debug.Log("Museum Getaway main menu built successfully at " + MainMenuScenePath);
        if (!Application.isBatchMode)
        {
            EditorUtility.DisplayDialog(
                "Museum Getaway Main Menu",
                "MainMenu.unity was created. It is Scene 0 and MuseumGetaway is Scene 1.",
                "OK");
        }
    }

    private static void BuildBackground(
        Material wallMaterial,
        Material floorMaterial,
        Material columnMaterial,
        Material pedestalMaterial,
        Material artifactMaterial)
    {
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.045f, 0.075f, 0.15f);
        RenderSettings.ambientIntensity = 1f;
        RenderSettings.reflectionIntensity = 0.25f;
        RenderSettings.fog = false;

        GameObject cameraObject = new GameObject("Main Camera - Menu");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 3.2f, -10f);
        cameraObject.transform.rotation = Quaternion.LookRotation(
            new Vector3(0f, 2f, 1.8f) - cameraObject.transform.position,
            Vector3.up);
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 45f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.008f, 0.015f, 0.04f);
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 40f;
        cameraObject.AddComponent<AudioListener>();

        GameObject backgroundRoot = new GameObject("CARTOON MUSEUM BACKGROUND");
        CreateDecorativePrimitive("Dark Museum Floor", PrimitiveType.Cube, new Vector3(0f, -0.15f, 1f), new Vector3(18f, 0.3f, 12f), floorMaterial, backgroundRoot.transform);
        CreateDecorativePrimitive("Dark Museum Back Wall", PrimitiveType.Cube, new Vector3(0f, 3f, 5f), new Vector3(18f, 6f, 0.35f), wallMaterial, backgroundRoot.transform);

        CreateMenuColumn("Left Classical Column", new Vector3(-6f, 0f, 3.5f), columnMaterial, backgroundRoot.transform);
        CreateMenuColumn("Right Classical Column", new Vector3(6f, 0f, 3.5f), columnMaterial, backgroundRoot.transform);

        CreateMenuExhibit("Left Exhibit", new Vector3(-3.7f, 0f, 2.8f), PrimitiveType.Sphere, pedestalMaterial, artifactMaterial, backgroundRoot.transform);
        CreateMenuExhibit("Center Exhibit", new Vector3(0f, 0f, 3.15f), PrimitiveType.Cube, pedestalMaterial, artifactMaterial, backgroundRoot.transform);
        CreateMenuExhibit("Right Exhibit", new Vector3(3.7f, 0f, 2.8f), PrimitiveType.Capsule, pedestalMaterial, artifactMaterial, backgroundRoot.transform);

        CreateMenuSpotlight("Warm Exhibit Light Left", new Vector3(-3.7f, 5f, 0.8f), new Vector3(-3.7f, 1.3f, 2.8f), new Color(1f, 0.65f, 0.35f));
        CreateMenuSpotlight("Warm Exhibit Light Center", new Vector3(0f, 5.2f, 1f), new Vector3(0f, 1.4f, 3.15f), new Color(0.5f, 0.72f, 1f));
        CreateMenuSpotlight("Warm Exhibit Light Right", new Vector3(3.7f, 5f, 0.8f), new Vector3(3.7f, 1.3f, 2.8f), new Color(1f, 0.65f, 0.35f));
    }

    private static void BuildInterface()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        Sprite roundedSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        GameObject canvasObject = new GameObject("Main Menu UI", typeof(RectTransform));
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject eventSystemObject = new GameObject("Event System", typeof(EventSystem));
        InputSystemUIInputModule inputModule = eventSystemObject.AddComponent<InputSystemUIInputModule>();
        inputModule.AssignDefaultActions();

        GameObject titleObject = new GameObject("Pulsing Cartoon Title", typeof(RectTransform), typeof(Image));
        titleObject.transform.SetParent(canvasObject.transform, false);
        RectTransform titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = titleRect.anchorMax = titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -155f);
        titleRect.sizeDelta = new Vector2(1120f, 215f);
        Image titleBacking = titleObject.GetComponent<Image>();
        titleBacking.sprite = roundedSprite;
        titleBacking.type = Image.Type.Sliced;
        titleBacking.color = new Color(0.018f, 0.08f, 0.22f, 0.9f);
        titleObject.AddComponent<MainMenuTitlePulse>();

        Text shadow = CreateText("Title Shadow", titleObject.transform, font, 104, TextAnchor.MiddleCenter, new Vector2(7f, -9f), new Vector2(1080f, 190f));
        shadow.text = "MUSEUM GETAWAY";
        shadow.fontStyle = FontStyle.Bold;
        shadow.color = new Color(0.005f, 0.025f, 0.11f);

        Text title = CreateText("Title", titleObject.transform, font, 104, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(1080f, 190f));
        title.text = "MUSEUM GETAWAY";
        title.fontStyle = FontStyle.Bold;
        title.color = new Color(0.12f, 0.58f, 1f);
        Outline outline = title.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.005f, 0.08f, 0.28f);
        outline.effectDistance = new Vector2(4f, -4f);
        outline.useGraphicAlpha = true;

        Text subtitle = CreateText("Tagline", canvasObject.transform, font, 35, TextAnchor.MiddleCenter, new Vector2(0f, -340f), new Vector2(1050f, 100f), new Vector2(0.5f, 1f));
        subtitle.text = "STEAL THE LOOT.\nDON'T GET CAUGHT.";
        subtitle.fontStyle = FontStyle.Bold;
        subtitle.color = new Color(0.9f, 0.95f, 1f);
        Outline subtitleOutline = subtitle.gameObject.AddComponent<Outline>();
        subtitleOutline.effectColor = new Color(0.01f, 0.04f, 0.12f, 0.9f);
        subtitleOutline.effectDistance = new Vector2(2f, -2f);

        Button playButton = CreateButton(
            "NEW GAME Button",
            "NEW GAME",
            canvasObject.transform,
            font,
            roundedSprite,
            new Vector2(0f, -75f),
            new Vector2(390f, 110f),
            50);
        Button controlsButton = CreateButton(
            "CONTROLS Button",
            "CONTROLS",
            canvasObject.transform,
            font,
            roundedSprite,
            new Vector2(0f, -205f),
            new Vector2(330f, 82f),
            36);
        Button quitButton = CreateButton(
            "QUIT Button",
            "QUIT",
            canvasObject.transform,
            font,
            roundedSprite,
            new Vector2(0f, -315f),
            new Vector2(230f, 68f),
            30);

        (GameObject controlsPanel, Button closeControlsButton) = CreateControlsPanel(
            canvasObject.transform,
            font,
            roundedSprite);

        MainMenuController controller = new GameObject("Main Menu Controller")
            .AddComponent<MainMenuController>();
        controller.Configure(
            playButton,
            controlsButton,
            quitButton,
            controlsPanel,
            closeControlsButton,
            "MuseumGetaway");
    }

    private static (GameObject panel, Button closeButton) CreateControlsPanel(
        Transform parent,
        Font font,
        Sprite roundedSprite)
    {
        GameObject overlay = new GameObject(
            "CONTROLS PANEL - Click Controls To Show",
            typeof(RectTransform),
            typeof(Image));
        overlay.transform.SetParent(parent, false);
        StretchToParent(overlay.GetComponent<RectTransform>());
        Image overlayImage = overlay.GetComponent<Image>();
        overlayImage.color = new Color(0.005f, 0.012f, 0.035f, 0.96f);

        GameObject card = new GameObject("Controls Card", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(overlay.transform, false);
        RectTransform cardRect = card.GetComponent<RectTransform>();
        cardRect.anchorMin = cardRect.anchorMax = cardRect.pivot = new Vector2(0.5f, 0.5f);
        cardRect.anchoredPosition = Vector2.zero;
        cardRect.sizeDelta = new Vector2(1360f, 900f);
        Image cardImage = card.GetComponent<Image>();
        cardImage.sprite = roundedSprite;
        cardImage.type = Image.Type.Sliced;
        cardImage.color = new Color(0.02f, 0.075f, 0.18f, 0.98f);
        Outline cardOutline = card.AddComponent<Outline>();
        cardOutline.effectColor = new Color(0.1f, 0.55f, 1f, 0.95f);
        cardOutline.effectDistance = new Vector2(5f, -5f);

        Text heading = CreateText(
            "CONTROLS Heading",
            card.transform,
            font,
            70,
            TextAnchor.MiddleCenter,
            new Vector2(0f, 365f),
            new Vector2(1200f, 100f));
        heading.text = "CONTROLS";
        heading.fontStyle = FontStyle.Bold;
        heading.color = new Color(0.18f, 0.7f, 1f);
        AddStrongTextOutline(heading, new Color(0.005f, 0.025f, 0.1f), 3f);

        CreateControlCallout(
            "RIGHT CLICK CAMERA Callout",
            "HOLD RIGHT MOUSE + MOVE MOUSE\nTO CONTROL THE CAMERA",
            new Vector2(0f, 225f),
            new Vector2(1160f, 165f),
            new Color(1f, 0.48f, 0.06f),
            card.transform,
            font,
            roundedSprite);

        CreateControlCallout(
            "SHIFT LOCK Callout",
            "PRESS SHIFT TO TOGGLE SHIFT LOCK\nPRESS SHIFT AGAIN TO UNLOCK",
            new Vector2(0f, 35f),
            new Vector2(1160f, 165f),
            new Color(0.12f, 0.88f, 1f),
            card.transform,
            font,
            roundedSprite);

        Text movement = CreateText(
            "Movement Controls",
            card.transform,
            font,
            38,
            TextAnchor.MiddleCenter,
            new Vector2(0f, -150f),
            new Vector2(1160f, 150f));
        movement.text =
            "W A S D  —  MOVE RELATIVE TO CAMERA\n" +
            "SHIFT LOCK  —  CAMERA-FACING MOVEMENT + STRAFING";
        movement.fontStyle = FontStyle.Bold;
        movement.color = Color.white;
        AddStrongTextOutline(movement, new Color(0.005f, 0.025f, 0.08f), 2f);

        Text objective = CreateText(
            "Objective Reminder",
            card.transform,
            font,
            30,
            TextAnchor.MiddleCenter,
            new Vector2(0f, -275f),
            new Vector2(1160f, 90f));
        objective.text = "STEAL EVERY ARTIFACT  •  DODGE RED LASERS  •  REACH THE GETAWAY VAN";
        objective.fontStyle = FontStyle.Bold;
        objective.color = new Color(0.82f, 0.9f, 1f);

        Button closeButton = CreateButton(
            "BACK Button",
            "BACK",
            card.transform,
            font,
            roundedSprite,
            new Vector2(0f, -385f),
            new Vector2(260f, 75f),
            34);

        overlay.SetActive(false);
        return (overlay, closeButton);
    }

    private static void CreateControlCallout(
        string name,
        string message,
        Vector2 position,
        Vector2 size,
        Color accent,
        Transform parent,
        Font font,
        Sprite roundedSprite)
    {
        GameObject callout = new GameObject(name, typeof(RectTransform), typeof(Image));
        callout.transform.SetParent(parent, false);
        RectTransform rect = callout.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Image image = callout.GetComponent<Image>();
        image.sprite = roundedSprite;
        image.type = Image.Type.Sliced;
        image.color = new Color(accent.r * 0.18f, accent.g * 0.18f, accent.b * 0.18f, 0.98f);
        Outline calloutOutline = callout.AddComponent<Outline>();
        calloutOutline.effectColor = accent;
        calloutOutline.effectDistance = new Vector2(4f, -4f);

        Text text = CreateText(
            "Bold Instructions",
            callout.transform,
            font,
            48,
            TextAnchor.MiddleCenter,
            Vector2.zero,
            Vector2.zero);
        StretchToParent(text.rectTransform);
        text.text = message;
        text.fontStyle = FontStyle.Bold;
        text.color = accent;
        AddStrongTextOutline(text, new Color(0.005f, 0.015f, 0.05f), 3f);
    }

    private static void AddStrongTextOutline(Text text, Color color, float distance)
    {
        Outline outline = text.gameObject.AddComponent<Outline>();
        outline.effectColor = color;
        outline.effectDistance = new Vector2(distance, -distance);
        outline.useGraphicAlpha = true;
    }

    private static Button CreateButton(
        string name,
        string label,
        Transform parent,
        Font font,
        Sprite roundedSprite,
        Vector2 position,
        Vector2 size,
        int fontSize)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = buttonObject.GetComponent<Image>();
        image.sprite = roundedSprite;
        image.type = Image.Type.Sliced;
        image.color = new Color(0.06f, 0.34f, 0.78f);

        Button button = buttonObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.06f, 0.34f, 0.78f);
        colors.highlightedColor = new Color(0.1f, 0.55f, 1f);
        colors.pressedColor = new Color(0.025f, 0.16f, 0.45f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.2f, 0.25f, 0.35f, 0.65f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        button.colors = colors;
        buttonObject.AddComponent<MainMenuButtonFeedback>();

        Text buttonText = CreateText("Label", buttonObject.transform, font, fontSize, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
        StretchToParent(buttonText.rectTransform);
        buttonText.text = label;
        buttonText.fontStyle = FontStyle.Bold;
        buttonText.color = Color.white;
        Outline outline = buttonText.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.005f, 0.04f, 0.14f);
        outline.effectDistance = new Vector2(2f, -2f);
        return button;
    }

    private static Text CreateText(
        string name,
        Transform parent,
        Font font,
        int fontSize,
        TextAnchor alignment,
        Vector2 position,
        Vector2 size,
        Vector2? anchor = null)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        Vector2 resolvedAnchor = anchor ?? new Vector2(0.5f, 0.5f);
        rect.anchorMin = rect.anchorMax = rect.pivot = resolvedAnchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Text text = textObject.GetComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        return text;
    }

    private static void CreateMenuColumn(string name, Vector3 position, Material material, Transform parent)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = position;
        CreateDecorativePrimitive("Base", PrimitiveType.Cylinder, new Vector3(0f, 0.15f, 0f), new Vector3(0.8f, 0.15f, 0.8f), material, root.transform, true);
        CreateDecorativePrimitive("Shaft", PrimitiveType.Cylinder, new Vector3(0f, 2.25f, 0f), new Vector3(0.52f, 2.0f, 0.52f), material, root.transform, true);
        CreateDecorativePrimitive("Capital", PrimitiveType.Cube, new Vector3(0f, 4.35f, 0f), new Vector3(1.5f, 0.35f, 1.5f), material, root.transform, true);
    }

    private static void CreateMenuExhibit(
        string name,
        Vector3 position,
        PrimitiveType artifactShape,
        Material pedestalMaterial,
        Material artifactMaterial,
        Transform parent)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = position;
        CreateDecorativePrimitive("Pedestal", PrimitiveType.Cube, new Vector3(0f, 0.55f, 0f), new Vector3(1.5f, 1.1f, 1.5f), pedestalMaterial, root.transform, true);
        GameObject artifact = CreateDecorativePrimitive("Priceless Artifact Silhouette", artifactShape, new Vector3(0f, 1.65f, 0f), Vector3.one * 0.72f, artifactMaterial, root.transform, true);
        if (artifactShape == PrimitiveType.Cube)
        {
            artifact.transform.localRotation = Quaternion.Euler(25f, 45f, 18f);
        }
    }

    private static void CreateMenuSpotlight(string name, Vector3 position, Vector3 target, Color color)
    {
        GameObject lightObject = new GameObject(name);
        lightObject.transform.position = position;
        lightObject.transform.rotation = Quaternion.LookRotation(target - position, Vector3.up);
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Spot;
        light.color = color;
        light.range = 12f;
        light.intensity = 16f;
        light.spotAngle = 45f;
        light.innerSpotAngle = 28f;
        light.shadows = LightShadows.None;
        light.lightmapBakeType = LightmapBakeType.Realtime;
    }

    private static GameObject CreateDecorativePrimitive(
        string name,
        PrimitiveType type,
        Vector3 position,
        Vector3 scale,
        Material material,
        Transform parent,
        bool localTransform = false)
    {
        GameObject gameObject = GameObject.CreatePrimitive(type);
        gameObject.name = name;
        gameObject.transform.SetParent(parent, false);
        if (localTransform)
        {
            gameObject.transform.localPosition = position;
            gameObject.transform.localScale = scale;
        }
        else
        {
            gameObject.transform.position = position;
            gameObject.transform.localScale = scale;
        }

        gameObject.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(gameObject.GetComponent<Collider>());
        return gameObject;
    }

    private static Material CreateMaterial(string name, Color color, bool emission = false)
    {
        string path = $"{MaterialFolder}/{name}.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }
        else
        {
            material.shader = shader;
        }

        material.name = name;
        material.color = color;
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (emission && material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 2.2f);
        }

        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", 0.18f);
        }

        EditorUtility.SetDirty(material);
        return material;
    }

    private static void StretchToParent(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void UpdateBuildSettingsSceneOrder()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(MainMenuScenePath, true),
            new EditorBuildSettingsScene(GameplayScenePath, true)
        };
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        int slash = path.LastIndexOf('/');
        string parent = path.Substring(0, slash);
        string name = path.Substring(slash + 1);
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }
}
