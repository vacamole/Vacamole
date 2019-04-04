using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Team6.Engine.Entities
{
    public enum EntityType
    {
        /// <summary>
        /// All <see cref="Team6.Engine.Graphics2d.IDrawableComponent"/> are rendered within the world pass
        /// </summary>
        Game,
        /// <summary>
        /// All <see cref="Team6.Engine.Graphics2d.IDrawableComponent"/> are rendered within the UI pass
        /// </summary>
        UI,
        /// <summary>
        /// Layer independent entities such as Input-Only entities. All <see cref="Team6.Engine.Graphics2d.IDrawableComponent"/> are IGNORED
        /// </summary>
        LayerIndependent,
        /// <summary>
        /// All <see cref="Team6.Engine.Graphics2d.IDrawableComponent"/> are rendered within the overlay pass - an additional layer on top 
        /// of <see cref="UI"/> and <see cref="Game"/>
        /// </summary>
        OverlayLayer
    }
}
