using System.Collections.Generic;
using System.Linq;

namespace Foosball
{
    public static class PlayerRegistry
    {
        private static readonly List<PlayerToken> s_Tokens = new();

        public static IReadOnlyList<PlayerToken> Tokens => s_Tokens;
        public static int Count => s_Tokens.Count;

        public static void Register(PlayerToken token)
        {
            if (token != null && !s_Tokens.Contains(token))
                s_Tokens.Add(token);
        }

        public static void Unregister(PlayerToken token) => s_Tokens.Remove(token);

        public static PlayerToken GetByIndex(int playerIndex)
            => s_Tokens.FirstOrDefault(t => t.PlayerIndex == playerIndex);

        public static void Clear() => s_Tokens.Clear();
    }
}
