using System.Collections.Generic;
using UnityEngine;

namespace Levels
{
    /// <summary>Loads every level file once and keeps the levels sorted by id.</summary>
    public static class LevelCatalog
    {
        private const string ResourceFolder = "Levels";

        private static List<LevelDefinition> _levels;

        public static IReadOnlyList<LevelDefinition> All
        {
            get
            {
                if (_levels == null) Load();
                return _levels;
            }
        }

        public static LevelDefinition Get(int id)
        {
            foreach (LevelDefinition level in All)
                if (level.id == id) return level;
            return null;
        }

        private static void Load()
        {
            _levels = new List<LevelDefinition>();
            foreach (TextAsset file in Resources.LoadAll<TextAsset>(ResourceFolder))
                _levels.AddRange(JsonUtility.FromJson<LevelFile>(file.text).levels);
            _levels.Sort((a, b) => a.id.CompareTo(b.id));
        }
    }
}
