using System;

namespace Foosball.Gameplay
{
    [Serializable]
    public class Formation
    {
        public string label = "3-3-3";
        public int[] rodCounts = { 3, 3, 3 };
    }
}