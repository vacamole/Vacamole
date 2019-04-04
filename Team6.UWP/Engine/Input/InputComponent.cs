using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using Team6.Engine.Components;

namespace Team6.Engine.Input
{
    public class InputComponent : Component
    {
        public List<InputMapping> Mappings { get; } = new List<InputMapping>();


        public InputComponent(params InputMapping[] mappings) : this(0, mappings)
        {
        }

        public InputComponent(int gamePadIndex, params InputMapping[] mappings)
        {
            this.GamePadIndex = gamePadIndex;
            Mappings.AddRange(mappings);
        }

        public virtual int GamePadIndex { get; protected set; }

        public void UpdateInput(ref InputFrame frame)
        {
            // [FOREACH PERFORMANCE] Should not allocate garbage
            foreach (var mapping in Mappings)
                mapping.Execute(ref frame);
        }
    }
}
