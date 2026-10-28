using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace MuseumGetaway
{
    public sealed class MuseumProceduralGenerator : MonoBehaviour
    {
        private const int CameraObstacleLayer = 8;
        private const float RoomSize = 12f;
        private const float WallHeight = 4f;
        private const float DoorWidth = 4f;
        private const int MinimumRooms = 5;
        private const int MaximumRooms = 15;
        private const int MaximumGenerationAttempts = 8;

        [Header("Run Settings")]
        [SerializeField] private bool useFixedSeed;
        [SerializeField] private int fixedSeed = 12345;
        [Tooltip("0 uses the normal weighted random room count. Set 5, 10, or 15 for test runs.")]
        [SerializeField] [Range(0, MaximumRooms)] private int forcedRoomCount;

        [Header("Existing Systems")]
        [SerializeField] private MuseumGameManager gameManager;
        [SerializeField] private GameHUD hud;
        [SerializeField] private EndScreenUI endScreen;
        [SerializeField] private Transform player;
        [SerializeField] private Transform generatedParent;

        [Header("Museum Materials")]
        [SerializeField] private Material floorMaterial;
        [SerializeField] private Material wallMaterial;
        [SerializeField] private Material roofMaterial;
        [SerializeField] private Material pillarMaterial;
        [SerializeField] private Material pedestalMaterial;
        [SerializeField] private Material artifactMaterial;
        [SerializeField] private Material laserMaterial;
        [SerializeField] private Material emitterMaterial;
        [SerializeField] private Material exitActiveMaterial;

        [Header("Optional Tripo Visuals")]
        [SerializeField] private GameObject goldenIdolModel;
        [SerializeField] private GameObject laserEmitterModel;
        [SerializeField] private GameObject getawayVanModel;
        [SerializeField] private GameObject romanPillarModel;

        private readonly Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };

        public int CurrentSeed { get; private set; }
        public int CurrentRoomCount { get; private set; }

        public void Configure(
            MuseumGameManager manager,
            GameHUD gameHud,
            EndScreenUI gameEndScreen,
            Transform playerTransform,
            Transform parent,
            Material floor,
            Material walls,
            Material roof,
            Material pillars,
            Material pedestals,
            Material artifacts,
            Material lasers,
            Material emitters,
            Material activeExit,
            GameObject goldenIdol,
            GameObject laserEmitter,
            GameObject getawayVan,
            GameObject romanPillar)
        {
            gameManager = manager;
            hud = gameHud;
            endScreen = gameEndScreen;
            player = playerTransform;
            generatedParent = parent;
            floorMaterial = floor;
            wallMaterial = walls;
            roofMaterial = roof;
            pillarMaterial = pillars;
            pedestalMaterial = pedestals;
            artifactMaterial = artifacts;
            laserMaterial = lasers;
            emitterMaterial = emitters;
            exitActiveMaterial = activeExit;
            goldenIdolModel = goldenIdol;
            laserEmitterModel = laserEmitter;
            getawayVanModel = getawayVan;
            romanPillarModel = romanPillar;
        }

        private void Awake()
        {
            GenerateRun();
        }

        private void GenerateRun()
        {
            CurrentSeed = useFixedSeed
                ? fixedSeed
                : UnityEngine.Random.Range(1, int.MaxValue);

            Random random = new Random(CurrentSeed);
            CurrentRoomCount = forcedRoomCount >= MinimumRooms
                ? Mathf.Clamp(forcedRoomCount, MinimumRooms, MaximumRooms)
                : ChooseWeightedRoomCount(random);

            List<RoomPlan> plan = null;
            List<LaserSlot> laserPlan = null;
            for (int attempt = 0; attempt < MaximumGenerationAttempts; attempt++)
            {
                int attemptSeed = CurrentSeed + (attempt * 7919);
                Random attemptRandom = new Random(attemptSeed);
                if (TryCreatePlan(CurrentRoomCount, attemptRandom, out plan) &&
                    ValidatePlan(plan) &&
                    TryCreateLaserPlan(plan, attemptRandom, out laserPlan) &&
                    ValidateContentPlan(plan, laserPlan))
                {
                    CurrentSeed = attemptSeed;
                    random = attemptRandom;
                    break;
                }

                plan = null;
            }

            if (plan == null)
            {
                CurrentSeed = useFixedSeed ? fixedSeed : CurrentSeed + 104729;
                random = new Random(CurrentSeed);
                plan = CreateSafeFallbackPlan();
                CurrentRoomCount = plan.Count;
                if (!TryCreateLaserPlan(plan, random, out laserPlan) || !ValidateContentPlan(plan, laserPlan))
                {
                    Debug.LogError("The safe museum fallback failed content validation. Generation stopped before creating an invalid run.");
                    return;
                }
                Debug.LogWarning("Museum generation validation failed repeatedly. Using the safe five-room fallback.");
            }

            Debug.Log($"Museum Run Seed: {CurrentSeed}");
            BuildValidatedPlan(plan, laserPlan, random);
        }

        private int ChooseWeightedRoomCount(Random random)
        {
            int roll = random.Next(100);
            if (roll < 40)
            {
                return random.Next(5, 8);
            }

            if (roll < 85)
            {
                return random.Next(8, 12);
            }

            return random.Next(12, 16);
        }

        private bool TryCreatePlan(int roomCount, Random random, out List<RoomPlan> plan)
        {
            plan = new List<RoomPlan> { new RoomPlan(Vector2Int.zero, MuseumRoomType.SmallGallery) };
            HashSet<Vector2Int> occupied = new HashSet<Vector2Int> { Vector2Int.zero };

            while (plan.Count < roomCount)
            {
                List<Vector2Int> candidates = new List<Vector2Int>();
                foreach (RoomPlan room in plan)
                {
                    foreach (Vector2Int direction in directions)
                    {
                        Vector2Int candidate = room.GridPosition + direction;
                        if (!occupied.Contains(candidate) && !candidates.Contains(candidate))
                        {
                            candidates.Add(candidate);
                        }
                    }
                }

                if (candidates.Count == 0)
                {
                    return false;
                }

                Vector2Int position = candidates[random.Next(candidates.Count)];
                occupied.Add(position);
                MuseumRoomType type = (MuseumRoomType)random.Next(0, 6);
                plan.Add(new RoomPlan(position, type));
            }

            RoomPlan finalRoom = FindFarthestRoom(plan);
            finalRoom.Type = MuseumRoomType.TreasureRoom;
            finalRoom.IsFinal = true;
            plan[0].IsStart = true;
            return true;
        }

        private bool ValidatePlan(List<RoomPlan> plan)
        {
            if (plan == null || plan.Count < MinimumRooms || plan.Count > MaximumRooms)
            {
                return false;
            }

            Dictionary<Vector2Int, RoomPlan> byPosition = BuildPositionMap(plan);
            Queue<Vector2Int> frontier = new Queue<Vector2Int>();
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
            frontier.Enqueue(Vector2Int.zero);
            visited.Add(Vector2Int.zero);

            while (frontier.Count > 0)
            {
                Vector2Int current = frontier.Dequeue();
                foreach (Vector2Int direction in directions)
                {
                    Vector2Int next = current + direction;
                    if (byPosition.ContainsKey(next) && visited.Add(next))
                    {
                        frontier.Enqueue(next);
                    }
                }
            }

            int finalCount = 0;
            foreach (RoomPlan room in plan)
            {
                if (room.IsFinal)
                {
                    finalCount++;
                }
            }

            return visited.Count == plan.Count && finalCount == 1 && byPosition.ContainsKey(Vector2Int.zero);
        }

        private List<RoomPlan> CreateSafeFallbackPlan()
        {
            List<RoomPlan> plan = new List<RoomPlan>();
            for (int index = 0; index < MinimumRooms; index++)
            {
                RoomPlan room = new RoomPlan(
                    new Vector2Int(0, index),
                    index == MinimumRooms - 1 ? MuseumRoomType.TreasureRoom : MuseumRoomType.SmallGallery);
                room.IsStart = index == 0;
                room.IsFinal = index == MinimumRooms - 1;
                plan.Add(room);
            }

            return plan;
        }

        private void BuildValidatedPlan(List<RoomPlan> plan, List<LaserSlot> laserPlan, Random random)
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.13f, 0.14f, 0.18f);
            RenderSettings.ambientIntensity = 1f;

            GameObject runRoot = new GameObject($"PROCEDURAL MUSEUM - Seed {CurrentSeed}");
            runRoot.transform.SetParent(generatedParent, false);
            GameObject roofRoot = new GameObject("ROOF - Disabled For Gameplay Camera");
            roofRoot.transform.SetParent(runRoot.transform, false);

            Dictionary<Vector2Int, RoomPlan> byPosition = BuildPositionMap(plan);
            foreach (RoomPlan room in plan)
            {
                BuildRoom(room, byPosition, runRoot.transform, roofRoot.transform, random);
            }

            roofRoot.SetActive(false);

            RoomPlan startRoom = plan.Find(room => room.IsStart);
            RoomPlan finalRoom = plan.Find(room => room.IsFinal);
            Vector3 startCenter = GridToWorld(startRoom.GridPosition);
            player.position = startCenter + new Vector3(0f, 0.05f, -2.5f);
            player.rotation = Quaternion.identity;

            int artifactCount = Mathf.Clamp(plan.Count, 5, 15);
            BuildArtifacts(plan, artifactCount, runRoot.transform, random);
            BuildLasers(laserPlan, runRoot.transform, random);
            GetawayExit exit = BuildGetaway(finalRoom, runRoot.transform);

            float alarmDuration = Mathf.Lerp(60f, 120f, (plan.Count - 5f) / 10f);
            Debug.Log(
                $"Museum Run: {plan.Count} rooms, {artifactCount} artifacts, " +
                $"{laserPlan.Count} lasers, {alarmDuration:0} seconds. " +
                $"Laser coverage: {plan.Count}/{plan.Count} rooms.");
            gameManager.ConfigureRun(
                CurrentSeed,
                plan.Count,
                artifactCount,
                alarmDuration,
                hud,
                endScreen,
                exit);
        }

        private void BuildRoom(
            RoomPlan room,
            Dictionary<Vector2Int, RoomPlan> occupied,
            Transform parent,
            Transform roofParent,
            Random random)
        {
            MuseumRoomTemplate template = MuseumRoomTemplate.Get(room.Type);
            Vector3 center = GridToWorld(room.GridPosition);
            GameObject roomRoot = new GameObject($"Room {room.GridPosition.x},{room.GridPosition.y} - {template.DisplayName}");
            roomRoot.transform.SetParent(parent, false);

            CreatePrimitive("Marble Floor", PrimitiveType.Cube, center + new Vector3(0f, -0.1f, 0f), new Vector3(RoomSize, 0.2f, RoomSize), floorMaterial, roomRoot.transform, true);
            CreatePrimitive("Flat Ceiling", PrimitiveType.Cube, center + new Vector3(0f, 4.2f, 0f), new Vector3(RoomSize, 0.25f, RoomSize), roofMaterial, roofParent, false);

            bool north = occupied.ContainsKey(room.GridPosition + Vector2Int.up);
            bool east = occupied.ContainsKey(room.GridPosition + Vector2Int.right);
            bool south = occupied.ContainsKey(room.GridPosition + Vector2Int.down);
            bool west = occupied.ContainsKey(room.GridPosition + Vector2Int.left);

            BuildHorizontalWall(center, true, north, roomRoot.transform);
            BuildVerticalWall(center, true, east, roomRoot.transform);
            if (!south)
            {
                BuildHorizontalWall(center, false, false, roomRoot.transform);
            }
            if (!west)
            {
                BuildVerticalWall(center, false, false, roomRoot.transform);
            }

            BuildRoomDecor(template, center, roomRoot.transform);
            CreateRoomLight(center, roomRoot.transform, random);
        }

        private void BuildHorizontalWall(Vector3 center, bool north, bool doorway, Transform parent)
        {
            float z = center.z + (north ? RoomSize * 0.5f : -RoomSize * 0.5f);
            if (!doorway)
            {
                CreatePrimitive("Wall", PrimitiveType.Cube, new Vector3(center.x, WallHeight * 0.5f, z), new Vector3(RoomSize, WallHeight, 0.25f), wallMaterial, parent, true);
                return;
            }

            float segmentLength = (RoomSize - DoorWidth) * 0.5f;
            float offset = (DoorWidth + segmentLength) * 0.5f;
            CreatePrimitive("Wall Left Of Door", PrimitiveType.Cube, new Vector3(center.x - offset, WallHeight * 0.5f, z), new Vector3(segmentLength, WallHeight, 0.25f), wallMaterial, parent, true);
            CreatePrimitive("Wall Right Of Door", PrimitiveType.Cube, new Vector3(center.x + offset, WallHeight * 0.5f, z), new Vector3(segmentLength, WallHeight, 0.25f), wallMaterial, parent, true);
        }

        private void BuildVerticalWall(Vector3 center, bool east, bool doorway, Transform parent)
        {
            float x = center.x + (east ? RoomSize * 0.5f : -RoomSize * 0.5f);
            if (!doorway)
            {
                CreatePrimitive("Wall", PrimitiveType.Cube, new Vector3(x, WallHeight * 0.5f, center.z), new Vector3(0.25f, WallHeight, RoomSize), wallMaterial, parent, true);
                return;
            }

            float segmentLength = (RoomSize - DoorWidth) * 0.5f;
            float offset = (DoorWidth + segmentLength) * 0.5f;
            CreatePrimitive("Wall South Of Door", PrimitiveType.Cube, new Vector3(x, WallHeight * 0.5f, center.z - offset), new Vector3(0.25f, WallHeight, segmentLength), wallMaterial, parent, true);
            CreatePrimitive("Wall North Of Door", PrimitiveType.Cube, new Vector3(x, WallHeight * 0.5f, center.z + offset), new Vector3(0.25f, WallHeight, segmentLength), wallMaterial, parent, true);
        }

        private void BuildRoomDecor(MuseumRoomTemplate template, Vector3 center, Transform parent)
        {
            Vector3[] pillarPositions =
            {
                new Vector3(-4.5f, 0f, -4.5f), new Vector3(4.5f, 0f, -4.5f),
                new Vector3(-4.5f, 0f, 4.5f), new Vector3(4.5f, 0f, 4.5f),
                new Vector3(-4.5f, 0f, 0f), new Vector3(4.5f, 0f, 0f)
            };

            for (int index = 0; index < template.PillarCount && index < pillarPositions.Length; index++)
            {
                CreatePillar(center + pillarPositions[index], parent, index + 1);
            }

            if (template.HasCentralDisplay)
            {
                // Preserve the authored display point without shipping any visible
                // procedural marker mesh, renderer, material, or collider.
                GameObject displayAnchor = new GameObject("Central Display Anchor - Invisible");
                displayAnchor.transform.SetParent(parent, false);
                displayAnchor.transform.position = center;
            }
        }

        private void CreatePillar(Vector3 position, Transform parent, int index)
        {
            GameObject anchor = new GameObject($"Roman Pillar {index:00}");
            anchor.transform.SetParent(parent, false);
            anchor.transform.position = position;
            anchor.layer = CameraObstacleLayer;

            CapsuleCollider cameraCollider = anchor.AddComponent<CapsuleCollider>();
            cameraCollider.center = new Vector3(0f, 1.7f, 0f);
            cameraCollider.radius = 0.58f;
            cameraCollider.height = 3.4f;
            cameraCollider.isTrigger = true;

            if (romanPillarModel != null)
            {
                CreateImportedVisual(romanPillarModel, "RomanPillar_VISUAL", anchor.transform, Vector3.zero, Quaternion.identity, Vector3.one * 1.35f);
                return;
            }

            CreateDecorativePrimitive("Base", PrimitiveType.Cylinder, new Vector3(0f, 0.2f, 0f), new Vector3(0.7f, 0.2f, 0.7f), pillarMaterial, anchor.transform, true);
            CreateDecorativePrimitive("Shaft", PrimitiveType.Cylinder, new Vector3(0f, 1.7f, 0f), new Vector3(0.42f, 1.3f, 0.42f), pillarMaterial, anchor.transform, true);
            CreateDecorativePrimitive("Capital", PrimitiveType.Cube, new Vector3(0f, 3.15f, 0f), new Vector3(1.1f, 0.25f, 1.1f), pillarMaterial, anchor.transform, true);
        }

        private void CreateRoomLight(Vector3 center, Transform parent, Random random)
        {
            GameObject lightObject = new GameObject("Ceiling Museum Light");
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.position = center + new Vector3(0f, 3.4f, 0f);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 9f;
            light.intensity = 7f;
            light.color = random.NextDouble() < 0.5
                ? new Color(1f, 0.82f, 0.62f)
                : new Color(0.58f, 0.72f, 1f);
            light.shadows = LightShadows.None;
        }

        private void BuildArtifacts(List<RoomPlan> rooms, int artifactCount, Transform parent, Random random)
        {
            GameObject root = new GameObject("Procedural Artifacts and Displays");
            root.transform.SetParent(parent, false);

            for (int index = 0; index < artifactCount; index++)
            {
                RoomPlan room = rooms[index % rooms.Count];
                MuseumRoomTemplate template = MuseumRoomTemplate.Get(room.Type);
                Vector3 position = GridToWorld(room.GridPosition) + template.ArtifactOffset;
                int value = ChooseLootValue(rooms.Count, random);

                CreatePrimitive(
                    $"Display Pedestal {index + 1}",
                    PrimitiveType.Cube,
                    position + Vector3.up * 0.5f,
                    new Vector3(1.35f, 1f, 1.35f),
                    pedestalMaterial,
                    root.transform,
                    false);

                GameObject artifact = new GameObject($"Artifact {index + 1} - ${value:N0}");
                artifact.transform.SetParent(root.transform, false);
                artifact.transform.position = position + Vector3.up * 1.55f;
                artifact.transform.localScale = Vector3.one * 0.65f;
                SphereCollider trigger = artifact.AddComponent<SphereCollider>();
                trigger.radius = 1.8f;
                trigger.isTrigger = true;
                Rigidbody body = artifact.AddComponent<Rigidbody>();
                body.isKinematic = true;
                body.useGravity = false;
                artifact.AddComponent<ArtifactCollectible>().Configure(gameManager, value);

                if (goldenIdolModel != null)
                {
                    CreateImportedVisual(goldenIdolModel, "GoldenIdol_VISUAL", artifact.transform, new Vector3(0f, -0.5f, 0f), Quaternion.Euler(0f, 180f, 0f), Vector3.one * 1.7f);
                }
                else
                {
                    CreateDecorativePrimitive("Artifact Primitive Visual", PrimitiveType.Sphere, Vector3.zero, Vector3.one, artifactMaterial, artifact.transform, true);
                }
            }
        }

        private int ChooseLootValue(int roomCount, Random random)
        {
            int roll = random.Next(100);
            if (roomCount <= 7)
            {
                return roll < 65 ? 2500 : roll < 95 ? 5000 : 10000;
            }

            if (roomCount <= 11)
            {
                return roll < 35 ? 2500 : roll < 70 ? 5000 : roll < 90 ? 10000 : roll < 99 ? 20000 : 50000;
            }

            return roll < 20 ? 2500 : roll < 45 ? 5000 : roll < 75 ? 10000 : roll < 95 ? 20000 : 50000;
        }

        private int ChooseTargetLaserCount(int roomCount, Random random)
        {
            if (roomCount <= 7)
            {
                return random.Next(8, 13);
            }

            if (roomCount <= 11)
            {
                return random.Next(12, 19);
            }

            return random.Next(16, 25);
        }

        private bool TryCreateLaserPlan(List<RoomPlan> rooms, Random random, out List<LaserSlot> laserPlan)
        {
            laserPlan = new List<LaserSlot>();
            List<LaserSlot> optionalSlots = new List<LaserSlot>();

            foreach (RoomPlan room in rooms)
            {
                List<Vector3> offsets = GetSafeLaserOffsets(room);
                if (offsets.Count == 0)
                {
                    return false;
                }

                Shuffle(offsets, random);
                laserPlan.Add(new LaserSlot(room, offsets[0]));
                for (int index = 1; index < offsets.Count; index++)
                {
                    optionalSlots.Add(new LaserSlot(room, offsets[index]));
                }
            }

            Shuffle(optionalSlots, random);
            int targetCount = Mathf.Clamp(
                ChooseTargetLaserCount(rooms.Count, random),
                rooms.Count,
                rooms.Count + optionalSlots.Count);
            for (int index = 0; laserPlan.Count < targetCount && index < optionalSlots.Count; index++)
            {
                laserPlan.Add(optionalSlots[index]);
            }

            return laserPlan.Count >= rooms.Count;
        }

        private List<Vector3> GetSafeLaserOffsets(RoomPlan room)
        {
            List<Vector3> offsets = new List<Vector3>();
            if (room.IsStart)
            {
                // The normal Small Gallery points are too close to the player spawn.
                // This dedicated point remains far from both the spawn and artifact.
                Vector3 safeStartOffset = new Vector3(-3.4f, 0f, 2.8f);
                if (IsLaserPlacementSafe(room, safeStartOffset))
                {
                    offsets.Add(safeStartOffset);
                }
                return offsets;
            }

            MuseumRoomTemplate template = MuseumRoomTemplate.Get(room.Type);
            foreach (Vector3 offset in template.LaserOffsets)
            {
                if (IsLaserPlacementSafe(room, offset))
                {
                    offsets.Add(offset);
                }
            }
            return offsets;
        }

        private bool IsLaserPlacementSafe(RoomPlan room, Vector3 offset)
        {
            MuseumRoomTemplate template = MuseumRoomTemplate.Get(room.Type);
            if (HorizontalDistance(offset, template.ArtifactOffset) < 4f)
            {
                return false;
            }

            if (room.IsStart && HorizontalDistance(offset, new Vector3(0f, 0f, -2.5f)) < 4f)
            {
                return false;
            }

            if (room.IsFinal && HorizontalDistance(offset, new Vector3(0f, 0f, 3.8f)) < 4f)
            {
                return false;
            }

            // Hazard centers stay well inside the room. A 4.2-unit beam cannot span
            // the 12-unit room or completely seal any four-unit boundary doorway.
            return Mathf.Abs(offset.x) <= 3.5f && Mathf.Abs(offset.z) <= 3.5f;
        }

        private bool ValidateContentPlan(List<RoomPlan> rooms, List<LaserSlot> laserPlan)
        {
            if (rooms == null || laserPlan == null || rooms.Count < MinimumRooms || rooms.Count > MaximumRooms)
            {
                return false;
            }

            HashSet<RoomPlan> coveredRooms = new HashSet<RoomPlan>();
            foreach (LaserSlot slot in laserPlan)
            {
                if (!rooms.Contains(slot.Room) || !IsLaserPlacementSafe(slot.Room, slot.Offset))
                {
                    return false;
                }
                coveredRooms.Add(slot.Room);
            }

            int artifactCount = rooms.Count;
            return artifactCount >= 5 && artifactCount <= 15 && coveredRooms.Count == rooms.Count;
        }

        private float HorizontalDistance(Vector3 first, Vector3 second)
        {
            Vector2 firstHorizontal = new Vector2(first.x, first.z);
            Vector2 secondHorizontal = new Vector2(second.x, second.z);
            return Vector2.Distance(firstHorizontal, secondHorizontal);
        }

        private void BuildLasers(List<LaserSlot> laserPlan, Transform parent, Random random)
        {
            GameObject root = new GameObject("Procedural Security Lasers");
            root.transform.SetParent(parent, false);
            for (int index = 0; index < laserPlan.Count; index++)
            {
                CreateLaser(index + 1, laserPlan[index], root.transform, random);
            }
        }

        private void CreateLaser(int index, LaserSlot slot, Transform parent, Random random)
        {
            Vector3 position = GridToWorld(slot.Room.GridPosition) + slot.Offset + Vector3.up * 0.75f;
            bool alongX = random.NextDouble() < 0.5;
            Vector3 beamScale = alongX
                ? new Vector3(4.2f, 0.15f, 0.15f)
                : new Vector3(0.15f, 0.15f, 4.2f);

            GameObject assembly = new GameObject($"Moving Laser {index:00}");
            assembly.transform.SetParent(parent, false);
            assembly.transform.position = position;
            assembly.AddComponent<MovingLaser>().Configure(
                0.45f + (float)random.NextDouble() * 0.45f,
                0.45f + (float)random.NextDouble() * 0.45f,
                0.65f + (float)random.NextDouble() * 0.55f,
                0.65f + (float)random.NextDouble() * 0.55f,
                (float)random.NextDouble() * Mathf.PI * 2f,
                (float)random.NextDouble() * Mathf.PI * 2f);

            GameObject beam = CreatePrimitive("Beam", PrimitiveType.Cube, Vector3.zero, beamScale, laserMaterial, assembly.transform, true);
            beam.layer = 0;
            beam.GetComponent<Collider>().isTrigger = true;
            Rigidbody body = beam.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            beam.AddComponent<LaserHazard>();

            Vector3 halfOffset = alongX
                ? new Vector3(beamScale.x * 0.5f + 0.15f, 0f, 0f)
                : new Vector3(0f, 0f, beamScale.z * 0.5f + 0.15f);
            Quaternion firstRotation = Quaternion.Euler(0f, alongX ? 90f : 0f, 0f);
            Quaternion secondRotation = Quaternion.Euler(0f, alongX ? -90f : 180f, 0f);
            CreateEmitter("LaserEmitter_VISUAL_Left", -halfOffset, firstRotation, assembly.transform);
            CreateEmitter("LaserEmitter_VISUAL_Right", halfOffset, secondRotation, assembly.transform);
        }

        private void CreateEmitter(string name, Vector3 position, Quaternion rotation, Transform parent)
        {
            if (laserEmitterModel != null)
            {
                CreateImportedVisual(laserEmitterModel, name, parent, position, rotation, Vector3.one * 0.85f);
                return;
            }

            CreateDecorativePrimitive(name, PrimitiveType.Cube, position, new Vector3(0.55f, 0.65f, 0.55f), emitterMaterial, parent, true);
        }

        private GetawayExit BuildGetaway(RoomPlan finalRoom, Transform parent)
        {
            Vector3 roomCenter = GridToWorld(finalRoom.GridPosition);
            Vector3 exitPosition = roomCenter + new Vector3(0f, 1.5f, 3.8f);

            GameObject exitObject = new GameObject("Getaway Exit Trigger - No Visible Block");
            exitObject.transform.SetParent(parent, false);
            exitObject.transform.position = exitPosition;
            BoxCollider trigger = exitObject.AddComponent<BoxCollider>();
            trigger.size = new Vector3(3f, 3f, 0.65f);
            trigger.isTrigger = true;
            Rigidbody body = exitObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;

            GameObject readyVisual = new GameObject("Getaway Ready Visuals - Hidden Until Active");
            readyVisual.transform.SetParent(parent, false);
            BuildExitOutline(roomCenter + new Vector3(0f, 0f, 3.35f), readyVisual.transform);

            if (getawayVanModel != null)
            {
                CreateImportedVisual(getawayVanModel, "GetawayVan_VISUAL", readyVisual.transform, roomCenter + new Vector3(0f, 0.15f, 3.8f), Quaternion.Euler(0f, -90f, 0f), Vector3.one * 4.25f);
            }

            readyVisual.SetActive(false);
            GetawayExit exit = exitObject.AddComponent<GetawayExit>();
            exit.Configure(gameManager, null, null, exitActiveMaterial, readyVisual);
            return exit;
        }

        private void BuildExitOutline(Vector3 center, Transform parent)
        {
            CreateDecorativePrimitive("Escape Outline North", PrimitiveType.Cube, center + new Vector3(0f, 0.035f, 0.55f), new Vector3(3f, 0.05f, 0.08f), exitActiveMaterial, parent, false);
            CreateDecorativePrimitive("Escape Outline South", PrimitiveType.Cube, center + new Vector3(0f, 0.035f, -0.55f), new Vector3(3f, 0.05f, 0.08f), exitActiveMaterial, parent, false);
            CreateDecorativePrimitive("Escape Outline East", PrimitiveType.Cube, center + new Vector3(1.45f, 0.035f, 0f), new Vector3(0.08f, 0.05f, 1.18f), exitActiveMaterial, parent, false);
            CreateDecorativePrimitive("Escape Outline West", PrimitiveType.Cube, center + new Vector3(-1.45f, 0.035f, 0f), new Vector3(0.08f, 0.05f, 1.18f), exitActiveMaterial, parent, false);

            GameObject glowObject = new GameObject("Subtle Getaway Glow");
            glowObject.transform.SetParent(parent, false);
            glowObject.transform.position = center + Vector3.up * 0.2f;
            Light glow = glowObject.AddComponent<Light>();
            glow.type = LightType.Point;
            glow.color = new Color(0.1f, 1f, 0.28f);
            glow.range = 3f;
            glow.intensity = 2f;
            glow.shadows = LightShadows.None;
        }

        private GameObject CreatePrimitive(
            string name,
            PrimitiveType type,
            Vector3 position,
            Vector3 scale,
            Material material,
            Transform parent,
            bool local)
        {
            GameObject created = GameObject.CreatePrimitive(type);
            created.name = name;
            created.transform.SetParent(parent, false);
            if (local)
            {
                created.transform.localPosition = position;
            }
            else
            {
                created.transform.position = position;
            }
            created.transform.localScale = scale;
            created.GetComponent<Renderer>().sharedMaterial = material;
            created.layer = CameraObstacleLayer;
            return created;
        }

        private GameObject CreateDecorativePrimitive(
            string name,
            PrimitiveType type,
            Vector3 position,
            Vector3 scale,
            Material material,
            Transform parent,
            bool local)
        {
            GameObject created = CreatePrimitive(name, type, position, scale, material, parent, local);
            Collider collider = created.GetComponent<Collider>();
            if (collider != null)
            {
                Object.Destroy(collider);
            }
            created.layer = 0;
            return created;
        }

        private GameObject CreateImportedVisual(
            GameObject source,
            string name,
            Transform parent,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale)
        {
            GameObject visual = Instantiate(source, parent);
            visual.name = name;
            visual.transform.localPosition = position;
            visual.transform.localRotation = rotation;
            visual.transform.localScale = scale;
            foreach (Collider collider in visual.GetComponentsInChildren<Collider>(true))
            {
                Object.Destroy(collider);
            }
            foreach (Rigidbody body in visual.GetComponentsInChildren<Rigidbody>(true))
            {
                Object.Destroy(body);
            }
            return visual;
        }

        private RoomPlan FindFarthestRoom(List<RoomPlan> plan)
        {
            RoomPlan farthest = plan[0];
            int bestDistance = -1;
            foreach (RoomPlan room in plan)
            {
                int distance = Mathf.Abs(room.GridPosition.x) + Mathf.Abs(room.GridPosition.y);
                if (distance > bestDistance)
                {
                    bestDistance = distance;
                    farthest = room;
                }
            }
            return farthest;
        }

        private Dictionary<Vector2Int, RoomPlan> BuildPositionMap(List<RoomPlan> plan)
        {
            Dictionary<Vector2Int, RoomPlan> map = new Dictionary<Vector2Int, RoomPlan>();
            foreach (RoomPlan room in plan)
            {
                map.Add(room.GridPosition, room);
            }
            return map;
        }

        private Vector3 GridToWorld(Vector2Int gridPosition)
        {
            return new Vector3(gridPosition.x * RoomSize, 0f, gridPosition.y * RoomSize);
        }

        private void Shuffle<T>(List<T> list, Random random)
        {
            for (int index = list.Count - 1; index > 0; index--)
            {
                int swapIndex = random.Next(index + 1);
                T value = list[index];
                list[index] = list[swapIndex];
                list[swapIndex] = value;
            }
        }

        private sealed class RoomPlan
        {
            public Vector2Int GridPosition { get; }
            public MuseumRoomType Type { get; set; }
            public bool IsStart { get; set; }
            public bool IsFinal { get; set; }

            public RoomPlan(Vector2Int gridPosition, MuseumRoomType type)
            {
                GridPosition = gridPosition;
                Type = type;
            }
        }

        private readonly struct LaserSlot
        {
            public RoomPlan Room { get; }
            public Vector3 Offset { get; }

            public LaserSlot(RoomPlan room, Vector3 offset)
            {
                Room = room;
                Offset = offset;
            }
        }
    }
}
