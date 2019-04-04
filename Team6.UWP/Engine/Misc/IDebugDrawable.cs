using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Team6.Engine.Misc
{
    public interface IDebugDrawable
    {
        void DebugDraw(DebugRenderer renderer, float elapsedSeconds, float totalSeconds);
    }
}
