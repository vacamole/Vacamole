using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Team6.Engine.Input
{
    public class InputMapping
    {
        public InputMapping(Func<InputFrame, bool> filter, Action<InputFrame> action)
        {
            this.Filter = filter;
            this.Action = action;
        }

        public Func<InputFrame, bool> Filter { get; }
        public Action<InputFrame> Action { get; }

        public void Execute(ref InputFrame frame)
        {
            bool execute = Filter?.Invoke(frame) ?? true;
            if (execute)
                Action(frame);
        }
    }
}
