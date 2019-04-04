using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Team6.Game.Scenes;

namespace Team6.Game.Mode
{
    public class PlayerInfo
    {
        public PlayerInfo(int playerIndex, int teamIndex, bool isKeyboardPlayer, int gamepadIndex, PlayerColors color, string name)
        {
            this.PlayerIndex = playerIndex;
            this.TeamIndex = teamIndex;
            this.IsKeyboardPlayer = isKeyboardPlayer;
            this.GamepadIndex = (gamepadIndex == -1) ? 0 : gamepadIndex;
            this.TotalScore = 0;
            this.NumberOfChickens = 0;
            this.NumberOfBoars = 0;
            this.Color = color;
            this.Name = name;
        }
        public int TeamIndex { get; }
        public int PlayerIndex { get; }
        public int TotalScore { get; set; }

        public int NumberOfChickens { get; set; }
        public int NumberOfBoars { get; set; }

        public bool IsKeyboardPlayer { get; }
        public int GamepadIndex { get; }
        public PlayerColors Color { get; }
        public string Name { get; set; }
    }

    public enum PlayerColors { Green, Pink, Purple, Yellow }

    public static class PlayerColorsExtensions
    {
        public static Color GetColor(this PlayerColors playerColors)
        {
            switch (playerColors)
            {
                case PlayerColors.Green:
                    return Color.Blue;
                case PlayerColors.Pink:
                    return Color.Pink;
                case PlayerColors.Purple:
                    return Color.Purple;
                case PlayerColors.Yellow:
                    return Color.Yellow;
                default:
                    throw new ArgumentOutOfRangeException(nameof(playerColors), playerColors, null);
            }
        }
    }

    public class PlayerAndGameInfo
    {
        public List<PlayerInfo> PlayerInfos { get; private set; }

        //public static readonly Color[] PlayerColors = { Color.AliceBlue, Color.Crimson, Color.DarkKhaki, Color.DarkOliveGreen };

        private MainGame game;

        public PlayerAndGameInfo(MainGame game)
        {
            this.game = game;
            this.PlayerInfos = new List<PlayerInfo>();
        }

        public PlayerInfo AddPlayer(bool isKeyboardPlayer, int gamepadIndex, string name)
        {
            var colors = ((PlayerColors[])Enum.GetValues(typeof(PlayerColors)));
            var index = GetFirstFreeIndex();
            var color = colors[(index - 1) % colors.Length];

            int team = 0;
            if (PlayerMode == PlayerMode.Free4All)
                team = index;
            else
                team = (index - 1) % 2 + 1;

            var p = new PlayerInfo(index, team, isKeyboardPlayer, gamepadIndex, color, name);
            PlayerInfos.Add(p);
            return p;
        }

        // Use this to get the first free index.
        // You can't just use PlayerInfos.Count as a new index, as this one can already exist.
        // This happens e.g. if two players join and then the first leaves.
        private int GetFirstFreeIndex()
        {
            int i = 1;
            // [FOREACH PERFORMANCE] ALLOCATES GARBAGE
            var indices = PlayerInfos.Select(p => p.PlayerIndex).OrderBy(ii => ii);
            foreach (var index in indices)
            {
                if (index == i)
                    i++;
            }
            return i;
        }

        public void AddChickenToPlayer(PlayerInfo playerInfo, int points = 1)
        {
            var i = PlayerInfos.FindIndex(p => p == playerInfo);
            if (i != -1)
            {
                PlayerInfos[i].TotalScore += points;
                PlayerInfos[i].NumberOfChickens += 1;
            }
        }

        public void AddBoarToPlayer(PlayerInfo playerInfo, int points = 3)
        {
            var i = PlayerInfos.FindIndex(p => p == playerInfo);
            if (i != -1)
            {
                PlayerInfos[i].TotalScore += points;
                PlayerInfos[i].NumberOfBoars += 1;
            }
        }

        public void ClearPlayers()
        {
            PlayerInfos.Clear();
        }

        public void ClearScore()
        {
            // [FOREACH PERFORMANCE] Should not allocate garbage
            foreach (var playerInfo in PlayerInfos)
            {
                playerInfo.TotalScore = 0;
            }
        }

        public PlayerInfo FindPlayer(bool isKeyboard, int gamepadIndex)
        {
            return PlayerInfos.Find(p => p.IsKeyboardPlayer == isKeyboard && p.GamepadIndex == gamepadIndex);
        }

        public void RemovePlayer(int gamepadIndex, bool isKeyboard)
        {
            PlayerInfos.Remove(FindPlayer(isKeyboard, gamepadIndex));
        }

        public void SetNameForPlayer(string newName, bool isKeyboard, int gamePadIndex)
        {
            var player = FindPlayer(isKeyboard, gamePadIndex);
            if (player != null)
                player.Name = newName;
        }

        public PlayerMode PlayerMode { get; set; }
        public GameMode GameMode { get; set; }

        /// <summary>
        /// For Waves: Number of waves = RuntemeInfo * 2 + 3 
        /// For BigHerd: Number of animals = RuntemeInfo * 10 + 20
        /// </summary>
        public int RuntimeInfo { get; internal set; }
    }


    public enum PlayerMode
    {
        TwoVsTwo,
        Free4All,
    }

    public enum GameMode
    {
        Waves,
        BigHerd,
    }
}