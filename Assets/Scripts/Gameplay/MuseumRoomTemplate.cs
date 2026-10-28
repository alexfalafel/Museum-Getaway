using UnityEngine;

namespace MuseumGetaway
{
    public enum MuseumRoomType
    {
        SmallGallery,
        LongGallery,
        ArtifactHall,
        CentralRotunda,
        PillarGallery,
        LaserSecurityHall,
        TreasureRoom
    }

    public sealed class MuseumRoomTemplate
    {
        public MuseumRoomType Type { get; }
        public string DisplayName { get; }
        public Vector3 ArtifactOffset { get; }
        public Vector3[] LaserOffsets { get; }
        public int PillarCount { get; }
        public bool HasCentralDisplay { get; }

        private MuseumRoomTemplate(
            MuseumRoomType type,
            string displayName,
            Vector3 artifactOffset,
            Vector3[] laserOffsets,
            int pillarCount,
            bool hasCentralDisplay)
        {
            Type = type;
            DisplayName = displayName;
            ArtifactOffset = artifactOffset;
            LaserOffsets = laserOffsets;
            PillarCount = pillarCount;
            HasCentralDisplay = hasCentralDisplay;
        }

        public static MuseumRoomTemplate Get(MuseumRoomType type)
        {
            switch (type)
            {
                case MuseumRoomType.LongGallery:
                    return new MuseumRoomTemplate(
                        type,
                        "Long Gallery",
                        new Vector3(3.4f, 0f, 2.8f),
                        new[] { new Vector3(0f, 0f, -2.2f), new Vector3(-2.2f, 0f, 0f) },
                        2,
                        false);
                case MuseumRoomType.ArtifactHall:
                    return new MuseumRoomTemplate(
                        type,
                        "Large Artifact Hall",
                        new Vector3(-3.2f, 0f, 2.7f),
                        new[] { new Vector3(1.5f, 0f, -2.4f), new Vector3(2.5f, 0f, 0.8f) },
                        4,
                        true);
                case MuseumRoomType.CentralRotunda:
                    return new MuseumRoomTemplate(
                        type,
                        "Central Rotunda",
                        new Vector3(0f, 0f, 3.2f),
                        new[] { new Vector3(-2.6f, 0f, -1.8f), new Vector3(2.6f, 0f, -1.8f) },
                        4,
                        true);
                case MuseumRoomType.PillarGallery:
                    return new MuseumRoomTemplate(
                        type,
                        "Pillar Gallery",
                        new Vector3(3.2f, 0f, -2.7f),
                        new[] { new Vector3(-2.2f, 0f, 1.8f), new Vector3(1.6f, 0f, 1.2f) },
                        6,
                        false);
                case MuseumRoomType.LaserSecurityHall:
                    return new MuseumRoomTemplate(
                        type,
                        "Laser Security Hall",
                        new Vector3(-3.3f, 0f, -2.8f),
                        new[] { new Vector3(0f, 0f, 2.1f), new Vector3(2.2f, 0f, -0.8f) },
                        2,
                        false);
                case MuseumRoomType.TreasureRoom:
                    return new MuseumRoomTemplate(
                        type,
                        "Treasure Room",
                        new Vector3(-3.2f, 0f, 1.2f),
                        new[] { new Vector3(2.4f, 0f, -2.2f), new Vector3(-1.2f, 0f, -2.7f) },
                        4,
                        true);
                default:
                    return new MuseumRoomTemplate(
                        MuseumRoomType.SmallGallery,
                        "Small Gallery",
                        new Vector3(3.2f, 0f, 2.6f),
                        new[] { new Vector3(-2.2f, 0f, -1.8f), new Vector3(1.8f, 0f, -2.2f) },
                        2,
                        false);
            }
        }
    }
}
