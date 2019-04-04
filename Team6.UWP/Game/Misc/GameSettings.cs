using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Team6.Game.Misc
{
    public class GameSettings
    {
        public float SoundVolume { get; set; } = 1;
        public float MusicVolume { get; set; } = 1;
        public float MasterVolume { get; set; } = 1;

        public bool IsFullscreen { get; set; } = true;
    }
}
