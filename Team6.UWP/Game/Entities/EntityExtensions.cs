using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Team6.Engine.Entities;

namespace Team6.Game.Entities
{
    public static class EntityExtensions
    {
        public static bool IsAnimal(this Entity entity)
        {
            return entity is BoarEntity || entity is ChickenEntity;
        }
    }
}
