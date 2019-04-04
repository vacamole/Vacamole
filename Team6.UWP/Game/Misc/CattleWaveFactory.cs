using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Team6.Engine.Misc;
using Team6.Game.Scenes;

namespace Team6.Game.Misc
{
    public static class CattleWaveFactory
    {
        public static int SpawnWave(this GameScene scene, WaveType type, Rectangle zone)
        {
            int numberOfPlayers = scene.Game.CurrentGameMode.PlayerInfos.Count;

            int numberOfBoars;
            int numberOfChicken;

            switch (type)
            {
                case WaveType.Fighting:

                    if (RandomExt.GetRandomFloat(0, 1) >= 0.5f)
                    {
                        numberOfBoars = numberOfPlayers - 1;
                        numberOfChicken = 0;
                    }
                    else
                    {
                        numberOfBoars = 0;
                        numberOfChicken = RandomExt.GetRandomInt(5 + numberOfPlayers, 12 + numberOfPlayers);
                    }
                    break;
                default:
                case WaveType.Collecting:
                    numberOfBoars = RandomExt.GetRandomInt(numberOfPlayers, numberOfPlayers * 2);
                    numberOfChicken = RandomExt.GetRandomInt(numberOfPlayers * 3, numberOfPlayers * 5);
                    break;
            }

            scene.SpawnCattleInZone(zone, numberOfBoars, numberOfChicken);

            return numberOfBoars + numberOfChicken;
        }


    }

    public enum WaveType
    {
        Collecting,
        Fighting,
    }
}
