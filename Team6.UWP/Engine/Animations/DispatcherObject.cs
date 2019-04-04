using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Team6.Engine.Animations
{
    public abstract class DispatcherObject
    {
        public bool IsFinished { get; protected set; }

        public abstract void Update(float elapsedSeconds, float totalSeconds);

        public bool IsRunning { get; set; } = true;

    }
}
