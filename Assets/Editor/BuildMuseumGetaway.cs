using System.IO;
using System.Collections.Generic;
using System.Linq;
using MuseumGetaway;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class BuildMuseumGetaway
{
    private const int CameraObstacleLayer = 8;
    private const int GameplayHiddenRoofLayer = 9;
    private const string ScenePath = "Assets/Scenes/MuseumGetaway.unity";
    private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
    private const string MaterialFolder = "Assets/Art/Materials";
    private const string FloorTexturePath = "Assets/Art/Textures/MuseumFloorTiles.png";
    private const string InputActionsPath = "Assets/InputSystem_Actions.inputactions";
    private const string GoldenIdolPath = "Assets/Art/Models/Tripo/GoldenIdol/GoldenIdol.glb";
    private const string LaserEmitterPath = "Assets/Art/Models/Tripo/LaserEmitter/SecurityLaserEmitter.glb";
    private const string AlternateLaserEmitterPath = "Assets/Art/Models/Tripo/LaserEmitter/LaserEmitter.glb";
    private const string GetawayVanPath = "Assets/Art/Models/Tripo/GetawayVan/GetawayVan.glb";
    private const string RomanPillarPath = "Assets/Art/Models/Tripo/RomanPillar/RomanPillar.glb";
    private const string ThiefCharacterPath = "Assets/Art/Models/Tripo/ThiefCharacter/ThiefCharacter.glb";
    private const string ThiefAnimatorControllerPath = "Assets/Art/Models/Tripo/ThiefCharacter/ThiefCharacter.controller";
    private const string ThiefIdleAnimationPath = "Assets/Art/Models/Tripo/ThiefCharacter/ThiefCharacter_Idle.anim";
    private const string ThiefWalkAnimationPath = "Assets/Art/Models/Tripo/ThiefCharacter/ThiefCharacter_SneakingWalk.anim";
    private const string ThiefVictoryAnimationPath = "Assets/Art/Models/Tripo/ThiefCharacter/ThiefCharacter_CartoonVictory.anim";
    private const string ImportedThiefWalkClipName = "A cartoon museum thief sneaking cautiously through a museum, wa";
    private const string ImportedThiefIdleClipName = "idle.001";
    private const string ImportedThiefVictoryClipName = "A short, exaggerated cartoon victory celebration for a goofy mu";
    private const string PickupAudioPath = "Assets/Audio/SFX/pickup.wav";
    private const string DamageAudioPath = "Assets/Audio/SFX/alarm.wav";
    private const string WinAudioPath = "Assets/Audio/SFX/win.wav";
    private const string MusicAudioPath = "Assets/Audio/Music/music.wav";

    [MenuItem("Tools/Build Museum Getaway")]
    public static void BuildGame()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        if (!Application.isBatchMode && AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
        {
            bool rebuild = EditorUtility.DisplayDialog(
                "Rebuild Museum Getaway?",
                "This will replace the generated MuseumGetaway scene. Save any manual changes elsewhere before continuing.",
                "Rebuild",
                "Cancel");
            if (!rebuild)
            {
                return;
            }
        }

        AssetDatabase.Refresh();
        EnsureProjectFolders();
        ConfigureAudioImporters();

        AudioClip pickupClip = AssetDatabase.LoadAssetAtPath<AudioClip>(PickupAudioPath);
        AudioClip damageClip = AssetDatabase.LoadAssetAtPath<AudioClip>(DamageAudioPath);
        AudioClip winClip = AssetDatabase.LoadAssetAtPath<AudioClip>(WinAudioPath);
        AudioClip musicClip = AssetDatabase.LoadAssetAtPath<AudioClip>(MusicAudioPath);

        GameObject goldenIdolModel = LoadOptionalModel("Golden Idol", GoldenIdolPath);
        GameObject laserEmitterModel = LoadOptionalModel("Laser Emitter", LaserEmitterPath, AlternateLaserEmitterPath);
        GameObject getawayVanModel = LoadOptionalModel("Getaway Van", GetawayVanPath);
        GameObject romanPillarModel = LoadOptionalModel("Roman Pillar", RomanPillarPath);
        GameObject thiefCharacterModel = LoadOptionalModel("Thief Character", ThiefCharacterPath);
        AnimatorController thiefAnimatorController = thiefCharacterModel != null
            ? CreateOrUpdateThiefAnimatorController()
            : null;

        Texture2D floorTexture = CreateMuseumFloorTexture();
        Material floorMaterial = CreateFloorMaterial(floorTexture);
        Material wallMaterial = CreateMaterial("Museum Walls", new Color(0.78f, 0.76f, 0.69f));
        Material roofMaterial = CreateMaterial("Museum Roof", new Color(0.12f, 0.14f, 0.18f));
        Material pillarMaterial = CreateMaterial("Classical Pillars", new Color(0.76f, 0.72f, 0.64f));
        Material pedestalMaterial = CreateMaterial("Display Pedestals", new Color(0.16f, 0.3f, 0.45f), true);
        Material playerMaterial = CreateMaterial("Player", new Color(0.12f, 0.45f, 0.9f));
        Material artifactMaterial = CreateMaterial("Artifact Gold", new Color(1f, 0.58f, 0.05f), true);
        Material laserMaterial = CreateMaterial("Security Laser", new Color(1f, 0.02f, 0.02f), true);
        Material emitterMaterial = CreateMaterial("Laser Emitter", new Color(0.08f, 0.08f, 0.1f));
        Material exitActiveMaterial = CreateMaterial("Exit Active", new Color(0.05f, 1f, 0.25f), true);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject environmentRoot = new GameObject("RUNTIME GENERATED MUSEUM");
        GameObject gameplayRoot = new GameObject("GAMEPLAY");

        MuseumGameManager manager = new GameObject("Museum Game Manager").AddComponent<MuseumGameManager>();
        BuildAudio(gameplayRoot.transform, manager, pickupClip, damageClip, winClip, musicClip);

        GameObject player = BuildPlayer(
            gameplayRoot.transform,
            manager,
            playerMaterial,
            thiefCharacterModel,
            thiefAnimatorController);
        BuildCamera(manager, player.transform);

        (GameHUD hud, EndScreenUI endScreen) = BuildInterface(manager);
        MuseumProceduralGenerator generator = new GameObject("Museum Procedural Generator")
            .AddComponent<MuseumProceduralGenerator>();
        generator.Configure(
            manager,
            hud,
            endScreen,
            player.transform,
            environmentRoot.transform,
            floorMaterial,
            wallMaterial,
            roofMaterial,
            pillarMaterial,
            pedestalMaterial,
            artifactMaterial,
            laserMaterial,
            emitterMaterial,
            exitActiveMaterial,
            goldenIdolModel,
            laserEmitterModel,
            getawayVanModel,
            romanPillarModel);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        UpdateBuildSettingsSceneOrder();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        SceneAsset mainMenuScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(MainMenuScenePath);
        if (!Application.isBatchMode && mainMenuScene != null)
        {
            EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
            Selection.activeObject = mainMenuScene;
        }
        else
        {
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
        }

        Debug.Log("Museum Getaway procedural scene built successfully at " + ScenePath);
        if (!Application.isBatchMode)
        {
            EditorUtility.DisplayDialog(
                "Museum Getaway",
                "Procedural gameplay scene created successfully.\n\nMainMenu is open and remains Scene 0. Open the Game view and press Play to begin at the menu.",
                "OK");
        }
    }

    private static void BuildMuseumShell(Transform parent, Material floor, Material walls, Material roof)
    {
        CreateCameraObstaclePrimitive("Floor", PrimitiveType.Cube, new Vector3(0f, -0.1f, 0f), new Vector3(24f, 0.2f, 18f), floor, parent);

        CreateCameraObstaclePrimitive("North Wall", PrimitiveType.Cube, new Vector3(0f, 2f, 9f), new Vector3(24f, 4f, 0.3f), walls, parent);
        CreateCameraObstaclePrimitive("South Wall", PrimitiveType.Cube, new Vector3(0f, 2f, -9f), new Vector3(24f, 4f, 0.3f), walls, parent);
        CreateCameraObstaclePrimitive("East Wall", PrimitiveType.Cube, new Vector3(12f, 2f, 0f), new Vector3(0.3f, 4f, 18f), walls, parent);
        CreateCameraObstaclePrimitive("West Wall", PrimitiveType.Cube, new Vector3(-12f, 2f, 0f), new Vector3(0.3f, 4f, 18f), walls, parent);

        CreateCameraObstaclePrimitive("West Gallery Wall", PrimitiveType.Cube, new Vector3(-3.8f, 1.25f, 0.5f), new Vector3(0.25f, 2.5f, 5f), walls, parent);
        CreateCameraObstaclePrimitive("East Gallery Wall", PrimitiveType.Cube, new Vector3(3.8f, 1.25f, 0.5f), new Vector3(0.25f, 2.5f, 5f), walls, parent);

        CreateCameraObstaclePrimitive("Entrance Header", PrimitiveType.Cube, new Vector3(0f, 3.55f, -8.7f), new Vector3(5f, 0.6f, 0.5f), walls, parent);

        GameObject roofRoot = new GameObject("ROOF - Gameplay Hidden");
        roofRoot.transform.SetParent(parent);
        roofRoot.layer = GameplayHiddenRoofLayer;
        GameObject roofObject = CreatePrimitive(
            "Flat Museum Roof",
            PrimitiveType.Cube,
            new Vector3(0f, 4.2f, 0f),
            new Vector3(24.4f, 0.25f, 18.4f),
            roof,
            roofRoot.transform);
        roofObject.layer = GameplayHiddenRoofLayer;
    }

    private static void BuildClassicalPillars(
        Transform parent,
        Material fallbackMaterial,
        GameObject romanPillarModel)
    {
        Vector3[] positions =
        {
            new Vector3(-10.5f, 0f, -6.8f),
            new Vector3(10.5f, 0f, -6.8f),
            new Vector3(-4.8f, 0f, -6.8f),
            new Vector3(4.8f, 0f, -6.8f),
            new Vector3(-10.5f, 0f, 2.8f),
            new Vector3(10.5f, 0f, 2.8f),
            new Vector3(-10.5f, 0f, 7.2f),
            new Vector3(10.5f, 0f, 7.2f),
            new Vector3(-4.8f, 0f, 7.2f),
            new Vector3(4.8f, 0f, 7.2f),
            new Vector3(-3.15f, 0f, 1f),
            new Vector3(3.15f, 0f, 1f)
        };

        GameObject pillarRoot = new GameObject("CLASSICAL PILLARS - Decorative");
        pillarRoot.transform.SetParent(parent, false);

        for (int index = 0; index < positions.Length; index++)
        {
            GameObject anchor = new GameObject($"Roman Pillar {index + 1:00}");
            anchor.transform.SetParent(pillarRoot.transform, false);
            anchor.transform.position = positions[index];
            anchor.layer = CameraObstacleLayer;

            // Trigger-only collider is visible to the camera cast but does not block
            // the existing CharacterController or alter gameplay collision.
            CapsuleCollider cameraCollider = anchor.AddComponent<CapsuleCollider>();
            cameraCollider.center = new Vector3(0f, 1.7f, 0f);
            cameraCollider.radius = 0.58f;
            cameraCollider.height = 3.4f;
            cameraCollider.isTrigger = true;

            if (romanPillarModel != null)
            {
                CreateImportedVisual(
                    romanPillarModel,
                    $"RomanPillar_VISUAL_{index + 1:00}",
                    anchor.transform,
                    Vector3.zero,
                    Quaternion.identity,
                    Vector3.one * 1.35f);
            }
            else
            {
                BuildPrimitivePillar(anchor.transform, fallbackMaterial);
            }
        }
    }

    private static void BuildPrimitivePillar(Transform parent, Material material)
    {
        CreateDecorativePrimitive(
            "Lower Plinth",
            PrimitiveType.Cylinder,
            new Vector3(0f, 0.15f, 0f),
            new Vector3(0.68f, 0.15f, 0.68f),
            material,
            parent,
            true);
        CreateDecorativePrimitive(
            "Lower Moulding",
            PrimitiveType.Cylinder,
            new Vector3(0f, 0.38f, 0f),
            new Vector3(0.55f, 0.08f, 0.55f),
            material,
            parent,
            true);
        CreateDecorativePrimitive(
            "Column Shaft",
            PrimitiveType.Cylinder,
            new Vector3(0f, 1.65f, 0f),
            new Vector3(0.4f, 1.2f, 0.4f),
            material,
            parent,
            true);
        CreateDecorativePrimitive(
            "Upper Moulding",
            PrimitiveType.Cylinder,
            new Vector3(0f, 2.92f, 0f),
            new Vector3(0.55f, 0.08f, 0.55f),
            material,
            parent,
            true);
        CreateDecorativePrimitive(
            "Capital",
            PrimitiveType.Cube,
            new Vector3(0f, 3.15f, 0f),
            new Vector3(1.2f, 0.3f, 1.2f),
            material,
            parent,
            true);
    }

    private static void BuildMuseumDisplayDetails(
        Transform parent,
        Material platformMaterial,
        Material caseFrameMaterial,
        Material velvetMaterial,
        Material brassMaterial)
    {
        Vector3[] artifactPositions =
        {
            new Vector3(-8f, 0f, -4f),
            new Vector3(-7f, 0f, 4.8f),
            new Vector3(0f, 0f, 5.8f),
            new Vector3(7f, 0f, 4.8f),
            new Vector3(8f, 0f, -4f)
        };

        GameObject detailRoot = new GameObject("MUSEUM DISPLAY DETAILS - Decorative");
        detailRoot.transform.SetParent(parent, false);

        for (int index = 0; index < artifactPositions.Length; index++)
        {
            CreateDecorativePrimitive(
                $"Exhibit Platform {index + 1}",
                PrimitiveType.Cube,
                artifactPositions[index] + Vector3.up * 0.06f,
                new Vector3(2.1f, 0.12f, 2.1f),
                platformMaterial,
                detailRoot.transform);
        }

        CreateOpenDisplayCase("Display Case North West", artifactPositions[1], caseFrameMaterial, detailRoot.transform);
        CreateOpenDisplayCase("Display Case North Center", artifactPositions[2], caseFrameMaterial, detailRoot.transform);
        CreateOpenDisplayCase("Display Case North East", artifactPositions[3], caseFrameMaterial, detailRoot.transform);

        CreateVelvetRopeBarrier("Velvet Barrier North West", new Vector3(-7f, 0f, 3.45f), 2.5f, velvetMaterial, brassMaterial, detailRoot.transform);
        CreateVelvetRopeBarrier("Velvet Barrier North East", new Vector3(7f, 0f, 3.45f), 2.5f, velvetMaterial, brassMaterial, detailRoot.transform);
        CreateVelvetRopeBarrier("Velvet Barrier South West", new Vector3(-8f, 0f, -5.35f), 2.5f, velvetMaterial, brassMaterial, detailRoot.transform);
        CreateVelvetRopeBarrier("Velvet Barrier South East", new Vector3(8f, 0f, -5.35f), 2.5f, velvetMaterial, brassMaterial, detailRoot.transform);
    }

    private static void CreateOpenDisplayCase(
        string name,
        Vector3 position,
        Material material,
        Transform parent)
    {
        GameObject caseRoot = new GameObject(name + " - Open Frame");
        caseRoot.transform.SetParent(parent, false);
        caseRoot.transform.position = position;

        Vector3[] postPositions =
        {
            new Vector3(-0.76f, 1.75f, -0.76f),
            new Vector3(0.76f, 1.75f, -0.76f),
            new Vector3(-0.76f, 1.75f, 0.76f),
            new Vector3(0.76f, 1.75f, 0.76f)
        };

        for (int index = 0; index < postPositions.Length; index++)
        {
            CreateDecorativePrimitive(
                $"Case Post {index + 1}",
                PrimitiveType.Cube,
                postPositions[index],
                new Vector3(0.06f, 1.5f, 0.06f),
                material,
                caseRoot.transform,
                true);
        }

        float[] frameHeights = { 1.02f, 2.5f };
        foreach (float height in frameHeights)
        {
            CreateDecorativePrimitive("Frame North", PrimitiveType.Cube, new Vector3(0f, height, 0.76f), new Vector3(1.58f, 0.06f, 0.06f), material, caseRoot.transform, true);
            CreateDecorativePrimitive("Frame South", PrimitiveType.Cube, new Vector3(0f, height, -0.76f), new Vector3(1.58f, 0.06f, 0.06f), material, caseRoot.transform, true);
            CreateDecorativePrimitive("Frame East", PrimitiveType.Cube, new Vector3(0.76f, height, 0f), new Vector3(0.06f, 0.06f, 1.58f), material, caseRoot.transform, true);
            CreateDecorativePrimitive("Frame West", PrimitiveType.Cube, new Vector3(-0.76f, height, 0f), new Vector3(0.06f, 0.06f, 1.58f), material, caseRoot.transform, true);
        }
    }

    private static void CreateVelvetRopeBarrier(
        string name,
        Vector3 center,
        float width,
        Material ropeMaterial,
        Material postMaterial,
        Transform parent)
    {
        GameObject barrierRoot = new GameObject(name);
        barrierRoot.transform.SetParent(parent, false);

        Vector3 leftPost = center + Vector3.left * width * 0.5f;
        Vector3 rightPost = center + Vector3.right * width * 0.5f;
        CreateRopePost("Left Post", leftPost, postMaterial, barrierRoot.transform);
        CreateRopePost("Right Post", rightPost, postMaterial, barrierRoot.transform);

        Vector3 centerSag = center + Vector3.up * 0.62f;
        CreateDecorativeBeam("Velvet Rope Left", leftPost + Vector3.up * 0.78f, centerSag, 0.09f, ropeMaterial, barrierRoot.transform);
        CreateDecorativeBeam("Velvet Rope Right", centerSag, rightPost + Vector3.up * 0.78f, 0.09f, ropeMaterial, barrierRoot.transform);
    }

    private static void CreateRopePost(string name, Vector3 position, Material material, Transform parent)
    {
        CreateDecorativePrimitive(name + " Base", PrimitiveType.Cylinder, position + Vector3.up * 0.08f, new Vector3(0.24f, 0.08f, 0.24f), material, parent);
        CreateDecorativePrimitive(name + " Stem", PrimitiveType.Cylinder, position + Vector3.up * 0.48f, new Vector3(0.1f, 0.4f, 0.1f), material, parent);
        CreateDecorativePrimitive(name + " Finial", PrimitiveType.Sphere, position + Vector3.up * 0.92f, Vector3.one * 0.18f, material, parent);
    }

    private static void CreateDecorativeBeam(
        string name,
        Vector3 start,
        Vector3 end,
        float thickness,
        Material material,
        Transform parent)
    {
        Vector3 offset = end - start;
        GameObject beam = CreateDecorativePrimitive(
            name,
            PrimitiveType.Cube,
            (start + end) * 0.5f,
            new Vector3(offset.magnitude, thickness, thickness),
            material,
            parent);
        beam.transform.rotation = Quaternion.FromToRotation(Vector3.right, offset.normalized);
    }

    private static void BuildLighting(Transform parent)
    {
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.13f, 0.14f, 0.18f);
        RenderSettings.ambientIntensity = 1f;
        RenderSettings.reflectionIntensity = 0.45f;
        RenderSettings.fog = false;

        Color warmLight = new Color(1f, 0.82f, 0.62f);
        Color coolLight = new Color(0.68f, 0.82f, 1f);
        CreateCeilingSpotLight("Ceiling Light South West", new Vector3(-7f, 3.7f, -4f), warmLight, parent);
        CreateCeilingSpotLight("Ceiling Light South Center", new Vector3(0f, 3.7f, -4f), coolLight, parent);
        CreateCeilingSpotLight("Ceiling Light South East", new Vector3(7f, 3.7f, -4f), warmLight, parent);
        CreateCeilingSpotLight("Ceiling Light North West", new Vector3(-7f, 3.7f, 4f), warmLight, parent);
        CreateCeilingSpotLight("Ceiling Light North Center", new Vector3(0f, 3.7f, 4f), coolLight, parent);
        CreateCeilingSpotLight("Ceiling Light North East", new Vector3(7f, 3.7f, 4f), warmLight, parent);
    }

    private static GameObject BuildPlayer(
        Transform parent,
        MuseumGameManager manager,
        Material material,
        GameObject thiefCharacterModel,
        AnimatorController thiefAnimatorController)
    {
        GameObject player = new GameObject("Player");
        player.transform.SetParent(parent);
        player.transform.position = new Vector3(0f, 0f, -6.5f);

        CharacterController controller = player.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.45f;
        controller.center = new Vector3(0f, 1f, 0f);

        InputActionAsset inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
        if (inputActions != null)
        {
            PlayerInput playerInput = player.AddComponent<PlayerInput>();
            playerInput.actions = inputActions;
            playerInput.defaultActionMap = "Player";
            playerInput.neverAutoSwitchControlSchemes = true;
        }
        else
        {
            Debug.LogWarning(
                "Museum Getaway builder: InputSystem_Actions.inputactions was not found. " +
                "Keyboard and mouse Input System fallbacks will remain active.");
        }

        GameObject visual = CreatePrimitive(
            "Player Capsule Visual",
            PrimitiveType.Capsule,
            new Vector3(0f, 1f, 0f),
            Vector3.one,
            material,
            player.transform,
            true);
        Object.DestroyImmediate(visual.GetComponent<Collider>());
        Renderer capsuleRenderer = visual.GetComponent<Renderer>();

        if (thiefCharacterModel != null)
        {
            capsuleRenderer.enabled = false;
            GameObject thiefVisualRoot = new GameObject("ThiefCharacter_VISUAL");
            thiefVisualRoot.transform.SetParent(player.transform, false);
            thiefVisualRoot.transform.localPosition = Vector3.zero;
            thiefVisualRoot.transform.localRotation = Quaternion.identity;
            thiefVisualRoot.transform.localScale = Vector3.one;

            GameObject thiefVisual = CreateImportedVisual(
                thiefCharacterModel,
                "Animated Thief Model",
                thiefVisualRoot.transform,
                Vector3.zero,
                Quaternion.identity,
                Vector3.one * 1.8f);

            Animator animator = thiefVisual.GetComponentInChildren<Animator>(true);
            if (animator == null)
            {
                animator = thiefVisual.AddComponent<Animator>();
            }

            if (thiefAnimatorController != null)
            {
                animator.runtimeAnimatorController = thiefAnimatorController;
                animator.applyRootMotion = false;
                player.AddComponent<PlayerAnimationDriver>().Configure(manager, controller, animator);
            }
            else
            {
                Debug.LogWarning(
                    "Museum Getaway builder: ThiefCharacter visual was imported, but its Animator Controller " +
                    "could not be generated. The character will remain a static visual fallback.");
            }
        }

        PlayerMovement movement = player.AddComponent<PlayerMovement>();
        movement.Configure(manager, 5f);

        PlayerHealth health = player.AddComponent<PlayerHealth>();
        health.Configure(manager, capsuleRenderer);
        return player;
    }

    private static void BuildCamera(MuseumGameManager manager, Transform player)
    {
        const float shoulderOffset = 1.2f;
        const float cameraDistance = 5.5f;
        const float lookHeight = 1.45f;
        const float initialPitch = 20f;
        const float horizontalSensitivity = 150f;
        const float verticalSensitivity = 110f;

        GameObject cameraObject = new GameObject("Main Camera - Manual Orbit");
        cameraObject.tag = "MainCamera";
        Quaternion initialOrbit = Quaternion.Euler(initialPitch, player.eulerAngles.y, 0f);
        cameraObject.transform.position = player.position
            + Vector3.up * lookHeight
            - initialOrbit * Vector3.forward * cameraDistance
            + player.right * shoulderOffset;
        cameraObject.transform.rotation = Quaternion.LookRotation(
            player.position + Vector3.up * lookHeight - cameraObject.transform.position,
            Vector3.up);
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = false;
        camera.fieldOfView = 58f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.015f, 0.02f, 0.035f);
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 50f;
        camera.cullingMask &= ~(1 << GameplayHiddenRoofLayer);
        cameraObject.AddComponent<AudioListener>();
        cameraObject.AddComponent<SimpleFollowCamera>().Configure(
            manager,
            player,
            cameraDistance,
            shoulderOffset,
            lookHeight,
            initialPitch,
            horizontalSensitivity,
            verticalSensitivity);
    }

    private static void BuildAudio(
        Transform parent,
        MuseumGameManager manager,
        AudioClip pickupClip,
        AudioClip damageClip,
        AudioClip winClip,
        AudioClip musicClip)
    {
        GameObject audioRoot = new GameObject("Audio");
        audioRoot.transform.SetParent(parent, false);

        GameObject musicObject = new GameObject("Music");
        musicObject.transform.SetParent(audioRoot.transform, false);
        AudioSource musicSource = musicObject.AddComponent<AudioSource>();
        musicSource.clip = musicClip;
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.spatialBlend = 0f;
        musicSource.volume = 0.35f;

        GameObject sfxObject = new GameObject("SFX");
        sfxObject.transform.SetParent(audioRoot.transform, false);
        AudioSource sfxSource = sfxObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f;
        sfxSource.volume = 0.8f;

        manager.ConfigureAudio(musicSource, sfxSource, pickupClip, damageClip, winClip);
    }

    private static void ConfigureAudioImporters()
    {
        ConfigureAudioImporter(PickupAudioPath, AudioClipLoadType.DecompressOnLoad, AudioCompressionFormat.PCM, 1f);
        ConfigureAudioImporter(DamageAudioPath, AudioClipLoadType.DecompressOnLoad, AudioCompressionFormat.PCM, 1f);
        ConfigureAudioImporter(WinAudioPath, AudioClipLoadType.DecompressOnLoad, AudioCompressionFormat.PCM, 1f);
        ConfigureAudioImporter(MusicAudioPath, AudioClipLoadType.Streaming, AudioCompressionFormat.Vorbis, 0.7f);
    }

    private static void ConfigureAudioImporter(
        string path,
        AudioClipLoadType loadType,
        AudioCompressionFormat compressionFormat,
        float quality)
    {
        if (AssetImporter.GetAtPath(path) is not AudioImporter importer)
        {
            Debug.LogWarning($"Museum Getaway audio file was not imported: {path}");
            return;
        }

        AudioImporterSampleSettings settings = importer.defaultSampleSettings;
        settings.loadType = loadType;
        settings.compressionFormat = compressionFormat;
        settings.quality = quality;
        settings.preloadAudioData = loadType != AudioClipLoadType.Streaming;
        importer.defaultSampleSettings = settings;
        importer.forceToMono = false;
        importer.SaveAndReimport();
    }

    private static void BuildArtifacts(
        Transform parent,
        MuseumGameManager manager,
        Material pedestalMaterial,
        Material artifactMaterial,
        GameObject goldenIdolModel)
    {
        Vector3[] positions =
        {
            new Vector3(-8f, 0f, -4f),
            new Vector3(-7f, 0f, 4.8f),
            new Vector3(0f, 0f, 5.8f),
            new Vector3(7f, 0f, 4.8f),
            new Vector3(8f, 0f, -4f)
        };
        int[] values = { 100, 150, 200, 250, 300 };
        PrimitiveType[] shapes =
        {
            PrimitiveType.Sphere,
            PrimitiveType.Cylinder,
            PrimitiveType.Capsule,
            PrimitiveType.Sphere,
            PrimitiveType.Cylinder
        };

        GameObject artifactRoot = new GameObject("Artifacts and Displays");
        artifactRoot.transform.SetParent(parent);

        for (int index = 0; index < positions.Length; index++)
        {
            Vector3 pedestalPosition = positions[index] + Vector3.up * 0.5f;
            CreatePrimitive(
                $"Display Pedestal {index + 1}",
                PrimitiveType.Cube,
                pedestalPosition,
                new Vector3(1.4f, 1f, 1.4f),
                pedestalMaterial,
                artifactRoot.transform);

            GameObject artifact = new GameObject($"Artifact {index + 1} - ${values[index]}");
            artifact.transform.SetParent(artifactRoot.transform);
            artifact.transform.position = positions[index] + Vector3.up * 1.55f;
            artifact.transform.localScale = Vector3.one * 0.65f;

            SphereCollider pickupTrigger = artifact.AddComponent<SphereCollider>();
            pickupTrigger.radius = 1.8f;
            pickupTrigger.isTrigger = true;
            Rigidbody body = artifact.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            artifact.AddComponent<ArtifactCollectible>().Configure(manager, values[index]);

            if (goldenIdolModel != null)
            {
                CreateImportedVisual(
                    goldenIdolModel,
                    "GoldenIdol_VISUAL",
                    artifact.transform,
                    new Vector3(0f, -0.5f, 0f),
                    Quaternion.Euler(0f, 180f, 0f),
                    Vector3.one * 1.7f);
            }
            else
            {
                GameObject placeholder = CreatePrimitive(
                    "Artifact Primitive Visual",
                    shapes[index],
                    Vector3.zero,
                    Vector3.one,
                    artifactMaterial,
                    artifact.transform,
                    true);
                Object.DestroyImmediate(placeholder.GetComponent<Collider>());
            }
        }
    }

    private static void BuildLasers(
        Transform parent,
        Material laserMaterial,
        Material emitterMaterial,
        GameObject laserEmitterModel)
    {
        GameObject laserRoot = new GameObject("Security Lasers");
        laserRoot.transform.SetParent(parent);

        CreateLaser("Laser A", new Vector3(-6.8f, 0.75f, -1.6f), new Vector3(4.2f, 0.15f, 0.15f), 1f, 1.5f, 0.75f, 1.15f, 0f, 1.2f, laserMaterial, emitterMaterial, laserEmitterModel, laserRoot.transform);
        CreateLaser("Laser B", new Vector3(6.8f, 0.75f, -1.6f), new Vector3(4.2f, 0.15f, 0.15f), 1.2f, 1.3f, 1.05f, 0.68f, 1.1f, 2.4f, laserMaterial, emitterMaterial, laserEmitterModel, laserRoot.transform);
        CreateLaser("Laser C", new Vector3(0f, 0.75f, 3.5f), new Vector3(4.5f, 0.15f, 0.15f), 1.4f, 1f, 0.82f, 1.32f, 2.2f, 0.4f, laserMaterial, emitterMaterial, laserEmitterModel, laserRoot.transform);
        CreateLaser("Laser D", new Vector3(0f, 0.75f, -3.1f), new Vector3(0.15f, 0.15f, 3.2f), 1.8f, 1.1f, 1.24f, 0.74f, 0.7f, 2.8f, laserMaterial, emitterMaterial, laserEmitterModel, laserRoot.transform);
    }

    private static GetawayExit BuildExit(
        Transform parent,
        MuseumGameManager manager,
        Material inactiveMaterial,
        Material activeMaterial,
        GameObject getawayVanModel)
    {
        GameObject exit = new GameObject("Getaway Exit Trigger - No Visible Block");
        exit.transform.SetParent(parent, false);
        exit.transform.position = new Vector3(0f, 1.5f, 8.55f);
        BoxCollider exitTrigger = exit.AddComponent<BoxCollider>();
        exitTrigger.size = new Vector3(3f, 3f, 0.45f);
        exitTrigger.isTrigger = true;
        Rigidbody body = exit.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;

        GameObject getawayReadyVisual = new GameObject("Getaway Ready Visuals - Hidden Until Active");
        getawayReadyVisual.transform.SetParent(parent, false);
        BuildGetawayFloorIndicator(getawayReadyVisual.transform, activeMaterial);

        if (getawayVanModel != null)
        {
            CreateImportedVisual(
                getawayVanModel,
                "GetawayVan_VISUAL",
                getawayReadyVisual.transform,
                new Vector3(0f, 0.15f, 8.55f),
                Quaternion.Euler(0f, -90f, 0f),
                Vector3.one * 4.25f);
        }
        getawayReadyVisual.SetActive(false);

        GetawayExit getawayExit = exit.AddComponent<GetawayExit>();
        getawayExit.Configure(
            manager,
            null,
            inactiveMaterial,
            activeMaterial,
            getawayReadyVisual);

        return getawayExit;
    }

    private static void BuildGetawayFloorIndicator(Transform parent, Material material)
    {
        const float centerZ = 8.2f;
        const float halfWidth = 1.45f;
        const float halfDepth = 0.55f;
        const float barThickness = 0.08f;
        const float floorHeight = 0.035f;

        CreateDecorativePrimitive(
            "Escape Outline North",
            PrimitiveType.Cube,
            new Vector3(0f, floorHeight, centerZ + halfDepth),
            new Vector3(3f, 0.05f, barThickness),
            material,
            parent);
        CreateDecorativePrimitive(
            "Escape Outline South",
            PrimitiveType.Cube,
            new Vector3(0f, floorHeight, centerZ - halfDepth),
            new Vector3(3f, 0.05f, barThickness),
            material,
            parent);
        CreateDecorativePrimitive(
            "Escape Outline East",
            PrimitiveType.Cube,
            new Vector3(halfWidth, floorHeight, centerZ),
            new Vector3(barThickness, 0.05f, 1.18f),
            material,
            parent);
        CreateDecorativePrimitive(
            "Escape Outline West",
            PrimitiveType.Cube,
            new Vector3(-halfWidth, floorHeight, centerZ),
            new Vector3(barThickness, 0.05f, 1.18f),
            material,
            parent);

        GameObject glowObject = new GameObject("Subtle Getaway Glow");
        glowObject.transform.SetParent(parent, false);
        glowObject.transform.position = new Vector3(0f, 0.2f, centerZ);
        Light glow = glowObject.AddComponent<Light>();
        glow.type = LightType.Point;
        glow.color = new Color(0.1f, 1f, 0.28f);
        glow.range = 3f;
        glow.intensity = 2f;
        glow.shadows = LightShadows.None;
        glow.lightmapBakeType = LightmapBakeType.Realtime;
    }

    private static (GameHUD, EndScreenUI) BuildInterface(MuseumGameManager manager)
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        GameObject canvasObject = new GameObject("Game UI", typeof(RectTransform));
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

        GameObject hudObject = new GameObject("HUD", typeof(RectTransform));
        hudObject.transform.SetParent(canvasObject.transform, false);
        StretchToParent(hudObject.GetComponent<RectTransform>());

        Text loot = CreateText("Loot", hudObject.transform, font, 36, TextAnchor.MiddleLeft, new Vector2(35f, -35f), new Vector2(540f, 52f), new Vector2(0f, 1f));
        loot.color = new Color(0.2f, 1f, 0.35f);
        loot.fontStyle = FontStyle.Bold;
        Text lootGain = CreateText("Loot Gain", hudObject.transform, font, 28, TextAnchor.MiddleLeft, new Vector2(35f, -82f), new Vector2(420f, 44f), new Vector2(0f, 1f));
        lootGain.color = new Color(0.35f, 1f, 0.45f);
        lootGain.fontStyle = FontStyle.Bold;
        lootGain.gameObject.SetActive(false);
        Text rooms = CreateText("Rooms", hudObject.transform, font, 30, TextAnchor.MiddleLeft, new Vector2(35f, -125f), new Vector2(500f, 46f), new Vector2(0f, 1f));
        Text artifacts = CreateText("Artifacts", hudObject.transform, font, 30, TextAnchor.MiddleLeft, new Vector2(35f, -170f), new Vector2(500f, 46f), new Vector2(0f, 1f));
        Text health = CreateText("Health", hudObject.transform, font, 30, TextAnchor.MiddleLeft, new Vector2(35f, -215f), new Vector2(500f, 46f), new Vector2(0f, 1f));
        Text time = CreateText("Time", hudObject.transform, font, 34, TextAnchor.MiddleRight, new Vector2(-35f, -35f), new Vector2(400f, 52f), new Vector2(1f, 1f));
        Text message = CreateText("Message", hudObject.transform, font, 54, TextAnchor.MiddleCenter, new Vector2(0f, -120f), new Vector2(900f, 90f), new Vector2(0.5f, 1f));
        message.color = new Color(1f, 0.25f, 0.12f);
        message.fontStyle = FontStyle.Bold;
        message.gameObject.SetActive(false);

        GameHUD hud = hudObject.AddComponent<GameHUD>();
        hud.Configure(loot, lootGain, rooms, artifacts, health, time, message);

        GameObject endScreens = new GameObject("End Screens", typeof(RectTransform));
        endScreens.transform.SetParent(canvasObject.transform, false);
        StretchToParent(endScreens.GetComponent<RectTransform>());
        EndScreenUI endScreen = endScreens.AddComponent<EndScreenUI>();

        (GameObject winPanel, Text winSummary, Button winButton) = CreateEndPanel(
            "Win Panel",
            "MUSEUM GETAWAY COMPLETE",
            new Color(0.03f, 0.3f, 0.12f, 0.96f),
            endScreens.transform,
            font);
        (GameObject losePanel, Text loseSummary, Button loseButton) = CreateEndPanel(
            "Lose Panel",
            "THE HEIST FAILED",
            new Color(0.35f, 0.04f, 0.04f, 0.96f),
            endScreens.transform,
            font);

        endScreen.Configure(manager, winPanel, winSummary, winButton, losePanel, loseSummary, loseButton);
        winPanel.SetActive(false);
        losePanel.SetActive(false);
        return (hud, endScreen);
    }

    private static (GameObject, Text, Button) CreateEndPanel(
        string name,
        string title,
        Color color,
        Transform parent,
        Font font)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(700f, 430f);
        panel.GetComponent<Image>().color = color;

        Text titleText = CreateText("Title", panel.transform, font, 48, TextAnchor.MiddleCenter, new Vector2(0f, -65f), new Vector2(640f, 80f), new Vector2(0.5f, 1f));
        titleText.text = title;
        titleText.fontStyle = FontStyle.Bold;

        Text summary = CreateText("Summary", panel.transform, font, 34, TextAnchor.MiddleCenter, new Vector2(0f, 5f), new Vector2(620f, 150f), new Vector2(0.5f, 0.5f));

        GameObject buttonObject = new GameObject("Restart Button", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(panel.transform, false);
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = buttonRect.anchorMax = buttonRect.pivot = new Vector2(0.5f, 0f);
        buttonRect.anchoredPosition = new Vector2(0f, 55f);
        buttonRect.sizeDelta = new Vector2(300f, 72f);
        buttonObject.GetComponent<Image>().color = new Color(0.95f, 0.75f, 0.15f);
        Button button = buttonObject.GetComponent<Button>();

        Text buttonText = CreateText("Label", buttonObject.transform, font, 32, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
        StretchToParent(buttonText.rectTransform);
        buttonText.text = "RESTART";
        buttonText.color = Color.black;
        buttonText.fontStyle = FontStyle.Bold;
        return (panel, summary, button);
    }

    private static Text CreateText(
        string name,
        Transform parent,
        Font font,
        int fontSize,
        TextAnchor alignment,
        Vector2 position,
        Vector2 size,
        Vector2 anchor)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Text text = textObject.GetComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.text = name;
        return text;
    }

    private static void CreateLaser(
        string name,
        Vector3 position,
        Vector3 scale,
        float xAmplitude,
        float zAmplitude,
        float xFrequency,
        float zFrequency,
        float xPhase,
        float zPhase,
        Material beamMaterial,
        Material emitterMaterial,
        GameObject laserEmitterModel,
        Transform parent)
    {
        GameObject assembly = new GameObject(name);
        assembly.transform.SetParent(parent);
        assembly.AddComponent<MovingLaser>().Configure(
            xAmplitude,
            zAmplitude,
            xFrequency,
            zFrequency,
            xPhase,
            zPhase);

        GameObject beam = CreatePrimitive("Beam", PrimitiveType.Cube, position, scale, beamMaterial, assembly.transform);
        beam.GetComponent<Collider>().isTrigger = true;
        Rigidbody body = beam.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
        beam.AddComponent<LaserHazard>();

        bool runsAlongX = scale.x > scale.z;
        Vector3 halfOffset = runsAlongX
            ? new Vector3(scale.x * 0.5f + 0.15f, 0f, 0f)
            : new Vector3(0f, 0f, scale.z * 0.5f + 0.15f);
        Vector3 emitterScale = runsAlongX
            ? new Vector3(0.3f, 0.65f, 0.65f)
            : new Vector3(0.65f, 0.65f, 0.3f);
        Quaternion leftRotation = Quaternion.Euler(0f, runsAlongX ? 90f : 0f, 0f);
        Quaternion rightRotation = Quaternion.Euler(0f, runsAlongX ? -90f : 180f, 0f);
        CreateLaserEmitterVisual("LaserEmitter_VISUAL_Left", position - halfOffset, emitterScale, leftRotation, emitterMaterial, laserEmitterModel, assembly.transform);
        CreateLaserEmitterVisual("LaserEmitter_VISUAL_Right", position + halfOffset, emitterScale, rightRotation, emitterMaterial, laserEmitterModel, assembly.transform);
    }

    private static void CreateLaserEmitterVisual(
        string name,
        Vector3 position,
        Vector3 placeholderScale,
        Quaternion rotation,
        Material placeholderMaterial,
        GameObject importedModel,
        Transform parent)
    {
        if (importedModel == null)
        {
            CreatePrimitive(name, PrimitiveType.Cube, position, placeholderScale, placeholderMaterial, parent);
            return;
        }

        CreateImportedVisual(
            importedModel,
            name,
            parent,
            position,
            rotation,
            Vector3.one * 0.85f);
    }

    private static AnimatorController CreateOrUpdateThiefAnimatorController()
    {
        AssetDatabase.ImportAsset(
            ThiefCharacterPath,
            ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);

        AnimationClip[] importedClips = AssetDatabase.LoadAllAssetsAtPath(ThiefCharacterPath)
            .OfType<AnimationClip>()
            .Where(clip => !clip.name.StartsWith("__preview__"))
            .ToArray();

        foreach (AnimationClip clip in importedClips)
        {
            Debug.Log($"Museum Getaway builder: detected ThiefCharacter animation clip '{clip.name}'.");
        }

        AnimationClip idleSource = FindAnimationClip(importedClips, ImportedThiefIdleClipName, "idle");
        AnimationClip walkSource = FindAnimationClip(importedClips, ImportedThiefWalkClipName, "sneak", "walk");
        AnimationClip victorySource = FindAnimationClip(importedClips, ImportedThiefVictoryClipName, "victory", "celebration");

        if (idleSource == null || walkSource == null || victorySource == null)
        {
            Debug.LogWarning(
                "Museum Getaway builder: the animated ThiefCharacter requires Idle, Sneaking Walk, and " +
                "Cartoon Victory clips. The blue capsule/model visual fallback will remain available until " +
                "all three clips import successfully.");
            return null;
        }

        AnimationClip idleClip = CreateOrUpdateAnimationClip(
            idleSource,
            ThiefIdleAnimationPath,
            "ThiefCharacter_Idle",
            true,
            true);
        AnimationClip walkClip = CreateOrUpdateAnimationClip(
            walkSource,
            ThiefWalkAnimationPath,
            "ThiefCharacter_SneakingWalk",
            true,
            true);
        AnimationClip victoryClip = CreateOrUpdateAnimationClip(
            victorySource,
            ThiefVictoryAnimationPath,
            "ThiefCharacter_CartoonVictory",
            false,
            false);

        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ThiefAnimatorControllerPath);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(ThiefAnimatorControllerPath);
        }

        controller.parameters = new[]
        {
            new AnimatorControllerParameter
            {
                name = "Speed",
                type = AnimatorControllerParameterType.Float
            },
            new AnimatorControllerParameter
            {
                name = "Victory",
                type = AnimatorControllerParameterType.Trigger
            }
        };

        if (controller.layers.Length == 0)
        {
            controller.AddLayer("Base Layer");
        }

        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        foreach (ChildAnimatorState childState in stateMachine.states)
        {
            stateMachine.RemoveState(childState.state);
        }

        foreach (AnimatorStateTransition transition in stateMachine.anyStateTransitions)
        {
            stateMachine.RemoveAnyStateTransition(transition);
        }

        foreach (AnimatorTransition transition in stateMachine.entryTransitions)
        {
            stateMachine.RemoveEntryTransition(transition);
        }

        AnimatorState idleState = stateMachine.AddState("Idle");
        idleState.motion = idleClip;
        AnimatorState walkState = stateMachine.AddState("Sneaking Walk");
        walkState.motion = walkClip;
        AnimatorState victoryState = stateMachine.AddState("Cartoon Victory");
        victoryState.motion = victoryClip;
        stateMachine.defaultState = idleState;

        AnimatorStateTransition idleToWalk = idleState.AddTransition(walkState);
        ConfigureAnimatorTransition(idleToWalk, 0.12f);
        idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.05f, "Speed");

        AnimatorStateTransition walkToIdle = walkState.AddTransition(idleState);
        ConfigureAnimatorTransition(walkToIdle, 0.12f);
        walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.05f, "Speed");

        AnimatorStateTransition anyToVictory = stateMachine.AddAnyStateTransition(victoryState);
        ConfigureAnimatorTransition(anyToVictory, 0.05f);
        anyToVictory.canTransitionToSelf = false;
        anyToVictory.AddCondition(AnimatorConditionMode.If, 0f, "Victory");

        EditorUtility.SetDirty(stateMachine);
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        return controller;
    }

    private static AnimationClip FindAnimationClip(
        IEnumerable<AnimationClip> clips,
        string exactName,
        params string[] fallbackKeywords)
    {
        AnimationClip exactClip = clips.FirstOrDefault(clip => clip.name == exactName);
        if (exactClip != null)
        {
            return exactClip;
        }

        foreach (string keyword in fallbackKeywords)
        {
            AnimationClip matchingClip = clips.FirstOrDefault(
                clip => clip.name.ToLowerInvariant().Contains(keyword.ToLowerInvariant()));
            if (matchingClip != null)
            {
                return matchingClip;
            }
        }

        return null;
    }

    private static AnimationClip CreateOrUpdateAnimationClip(
        AnimationClip source,
        string assetPath,
        string assetName,
        bool loop,
        bool bakeVerticalMotion)
    {
        AnimationClip generatedClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
        if (generatedClip == null)
        {
            generatedClip = Object.Instantiate(source);
            generatedClip.hideFlags = HideFlags.None;
            generatedClip.name = assetName;
            AssetDatabase.CreateAsset(generatedClip, assetPath);
        }
        else
        {
            EditorUtility.CopySerialized(source, generatedClip);
            generatedClip.hideFlags = HideFlags.None;
            generatedClip.name = assetName;
        }

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(generatedClip);
        settings.loopTime = loop;
        settings.loopBlend = loop;
        settings.loopBlendOrientation = true;
        settings.keepOriginalOrientation = true;
        settings.loopBlendPositionXZ = true;
        settings.keepOriginalPositionXZ = true;
        settings.loopBlendPositionY = bakeVerticalMotion;
        settings.keepOriginalPositionY = true;
        settings.heightFromFeet = true;
        AnimationUtility.SetAnimationClipSettings(generatedClip, settings);
        generatedClip.wrapMode = loop ? WrapMode.Loop : WrapMode.Once;
        EditorUtility.SetDirty(generatedClip);
        return generatedClip;
    }

    private static void ConfigureAnimatorTransition(
        AnimatorStateTransition transition,
        float duration)
    {
        transition.hasExitTime = false;
        transition.hasFixedDuration = true;
        transition.duration = duration;
    }

    private static GameObject LoadOptionalModel(string displayName, params string[] paths)
    {
        bool foundUnimportedAsset = false;
        foreach (string path in paths)
        {
            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (model != null)
            {
                Debug.Log($"Museum Getaway builder: using {displayName} visual from {path}");
                return model;
            }

            foundUnimportedAsset |= AssetDatabase.LoadMainAssetAtPath(path) != null;
        }

        string checkedPaths = string.Join(" or ", paths);
        if (foundUnimportedAsset)
        {
            Debug.LogWarning(
                $"Museum Getaway builder: {displayName} exists at {checkedPaths}, but Unity did not import it as a GameObject. " +
                "The primitive fallback will be used. Confirm that a GLB importer is installed and the asset imports without errors.");
        }
        else
        {
            Debug.LogWarning(
                $"Museum Getaway builder: optional {displayName} model is missing at {checkedPaths}. " +
                "The primitive fallback will be used.");
        }

        return null;
    }

    private static GameObject CreateImportedVisual(
        GameObject modelAsset,
        string name,
        Transform parent,
        Vector3 localPosition,
        Quaternion localRotation,
        Vector3 localScale)
    {
        GameObject visual = Object.Instantiate(modelAsset);
        visual.name = name;
        visual.transform.SetParent(parent, false);
        visual.transform.localPosition = localPosition;
        visual.transform.localRotation = localRotation;
        visual.transform.localScale = localScale;

        foreach (Collider collider in visual.GetComponentsInChildren<Collider>(true))
        {
            Object.DestroyImmediate(collider);
        }

        foreach (Rigidbody body in visual.GetComponentsInChildren<Rigidbody>(true))
        {
            Object.DestroyImmediate(body);
        }

        return visual;
    }

    private static GameObject CreatePrimitive(
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
        return gameObject;
    }

    private static GameObject CreateCameraObstaclePrimitive(
        string name,
        PrimitiveType type,
        Vector3 position,
        Vector3 scale,
        Material material,
        Transform parent)
    {
        GameObject obstacle = CreatePrimitive(name, type, position, scale, material, parent);
        obstacle.layer = CameraObstacleLayer;
        return obstacle;
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
        GameObject decorativeObject = CreatePrimitive(
            name,
            type,
            position,
            scale,
            material,
            parent,
            localTransform);
        Object.DestroyImmediate(decorativeObject.GetComponent<Collider>());
        return decorativeObject;
    }

    private static void CreateCeilingSpotLight(string name, Vector3 position, Color color, Transform parent)
    {
        GameObject lightObject = new GameObject(name);
        lightObject.transform.SetParent(parent);
        lightObject.transform.position = position;
        lightObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Spot;
        light.range = 14f;
        light.intensity = 17f;
        light.spotAngle = 125f;
        light.innerSpotAngle = 90f;
        light.color = color;
        light.shadows = LightShadows.None;
        light.lightmapBakeType = LightmapBakeType.Realtime;
    }

    private static Texture2D CreateMuseumFloorTexture()
    {
        const int textureSize = 256;
        const int tilesPerAxis = 4;
        const int groutPixels = 3;
        int tileSize = textureSize / tilesPerAxis;

        Color[] stoneTones =
        {
            new Color(0.74f, 0.71f, 0.65f),
            new Color(0.79f, 0.76f, 0.69f),
            new Color(0.71f, 0.70f, 0.66f),
            new Color(0.77f, 0.73f, 0.66f),
            new Color(0.73f, 0.72f, 0.68f)
        };
        Color groutColor = new Color(0.35f, 0.36f, 0.36f);

        Texture2D generatedTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, true, false);
        generatedTexture.name = "MuseumFloorTiles";

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                int tileX = x / tileSize;
                int tileY = y / tileSize;
                int localX = x % tileSize;
                int localY = y % tileSize;
                bool isGrout = localX < groutPixels || localY < groutPixels;

                Color pixelColor;
                if (isGrout)
                {
                    pixelColor = groutColor;
                }
                else
                {
                    int toneIndex = (tileX + (tileY * 3)) % stoneTones.Length;
                    float grain =
                        (Mathf.Sin((x * 0.11f) + (tileY * 0.9f)) * 0.014f) +
                        (Mathf.Sin((y * 0.17f) + (tileX * 1.3f)) * 0.009f) +
                        (Mathf.Sin((x + y) * 0.035f) * 0.008f);
                    Color stone = stoneTones[toneIndex];
                    pixelColor = new Color(
                        Mathf.Clamp01(stone.r + grain),
                        Mathf.Clamp01(stone.g + grain),
                        Mathf.Clamp01(stone.b + grain * 0.8f),
                        1f);
                }

                generatedTexture.SetPixel(x, y, pixelColor);
            }
        }

        generatedTexture.Apply(true, false);
        byte[] pngBytes = generatedTexture.EncodeToPNG();
        Object.DestroyImmediate(generatedTexture);

        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string absoluteTexturePath = Path.Combine(projectRoot, FloorTexturePath);
        File.WriteAllBytes(absoluteTexturePath, pngBytes);

        AssetDatabase.ImportAsset(FloorTexturePath, ImportAssetOptions.ForceSynchronousImport);
        TextureImporter importer = AssetImporter.GetAtPath(FloorTexturePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true;
            importer.alphaSource = TextureImporterAlphaSource.None;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Bilinear;
            importer.mipmapEnabled = true;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.maxTextureSize = textureSize;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Texture2D>(FloorTexturePath);
    }

    private static Material CreateFloorMaterial(Texture2D floorTexture)
    {
        Material material = CreateMaterial("Museum Floor", Color.white);
        material.mainTexture = floorTexture;
        material.mainTextureScale = new Vector2(3f, 2.25f);
        material.mainTextureOffset = Vector2.zero;

        if (material.HasProperty("_BaseMap"))
        {
            material.SetTexture("_BaseMap", floorTexture);
            material.SetTextureScale("_BaseMap", new Vector2(3f, 2.25f));
        }

        if (material.HasProperty("_Metallic"))
        {
            material.SetFloat("_Metallic", 0f);
        }

        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", 0.22f);
        }

        EditorUtility.SetDirty(material);
        return material;
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
            material.SetColor("_EmissionColor", color * 2.5f);
        }
        else if (material.HasProperty("_EmissionColor"))
        {
            material.DisableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", Color.black);
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
        List<EditorBuildSettingsScene> orderedScenes = new List<EditorBuildSettingsScene>();
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(MainMenuScenePath) != null)
        {
            orderedScenes.Add(new EditorBuildSettingsScene(MainMenuScenePath, true));
        }

        orderedScenes.Add(new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = orderedScenes.ToArray();
    }

    private static void EnsureProjectFolders()
    {
        string[] folders =
        {
            "Assets/Art",
            "Assets/Art/Materials",
            "Assets/Art/Models",
            "Assets/Art/Models/Placeholders",
            "Assets/Art/Models/Tripo",
            "Assets/Art/Models/Tripo/GoldenIdol",
            "Assets/Art/Models/Tripo/LaserEmitter",
            "Assets/Art/Models/Tripo/GetawayVan",
            "Assets/Art/Models/Tripo/RomanPillar",
            "Assets/Art/Models/Tripo/ThiefCharacter",
            "Assets/Art/Textures",
            "Assets/Audio",
            "Assets/Audio/Music",
            "Assets/Audio/SFX",
            "Assets/Input",
            "Assets/Prefabs",
            "Assets/Prefabs/Environment",
            "Assets/Prefabs/Gameplay",
            "Assets/Prefabs/Player",
            "Assets/Prefabs/UI",
            "Assets/Scripts",
            "Assets/Scripts/Camera",
            "Assets/Scripts/Core",
            "Assets/Scripts/Gameplay",
            "Assets/Scripts/Player",
            "Assets/Scripts/UI"
        };

        foreach (string folder in folders)
        {
            EnsureFolder(folder);
        }
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
