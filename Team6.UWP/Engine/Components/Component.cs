using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Team6.Engine.Entities;

namespace Team6.Engine.Components
{
    public abstract class Component
    {
        public void SetEntity(Entity entity)
        {
            if (this.Entity != null) throw new NotSupportedException("You cannot rebind a component to a new entity.");
            this.Entity = entity;
        }

        public virtual void Initialize()
        {

        }

        public Entity Entity { get; private set; }

        /// <summary>
        /// Gets or sets a name that can be used to identify multiple sensor components in one entity
        /// </summary>
        public string Name { get; set; }
    }
}
