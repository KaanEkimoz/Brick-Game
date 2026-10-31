using System;
using System.Collections.Generic;

namespace Levels
{
    /// <summary>One level as authored in Resources/Levels. Tools/levels/LEVEL_SPEC.md defines the fields.</summary>
    [Serializable]
    public class LevelDefinition
    {
        public int id;
        public string name;
        public string hint;
        public string objective;
        public int target;
        public List<string> rows = new List<string>();
        public string pieces;
        public string abilityMode;
        public int abilityEvery;
        public bool noLineClears;
        public int dropLevel = 1;
        public int pieceLimit;
        public int timeLimit;
        public int garbageEvery;
        public bool hideGhost;
        public bool hideNext;
    }

    [Serializable]
    public class LevelFile
    {
        public List<LevelDefinition> levels = new List<LevelDefinition>();
    }
}
