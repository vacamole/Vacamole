using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using Team6.Engine.Entities;
using Team6.Engine.Misc;
using Microsoft.Xna.Framework;
using Team6.Game.Misc;

namespace Team6.Engine.Components
{
    public class SensorComponent : PhysicsComponent
    {
        private readonly Dictionary<Entity, int> currentlyVisibleEntities = new Dictionary<Entity, int>();
        private Func<Entity, bool> sensorFilter;

        public event Action<string, Entity, Contact> OnContact;
        public event Action<string, Entity> OnSeperation;


        public SensorComponent(Shape sensorShape, Action<string, Entity, Contact> onContact = null, Action<string, Entity> onSeperation = null) :
            this("default", sensorShape, e => true, onContact, onSeperation)
        {
        }

        public SensorComponent(string name, Shape sensorShape, Func<Entity, bool> filter, Action<string, Entity, Contact> onContact = null, Action<string, Entity> onSeperation = null) : base(sensorShape)
        {
            this.Name = name;
            this.OnContact += onContact;
            this.OnSeperation += onSeperation;
            this.sensorFilter = filter;
        }

        public override void Initialize()
        {
            base.Initialize();
            Fixture.IsSensor = true;
            Fixture.OnCollision += OnSensorCollision;
            Fixture.OnSeparation += OnSensorSeperation;
            Fixture.CollisionCategories = GameConstants.CollisionCategorySensor;
            Fixture.CollidesWith = Category.All & ~GameConstants.CollisionCategorySensor;
        }

        private void OnSensorSeperation(Fixture fixtureA, Fixture fixtureB)
        {
            System.Diagnostics.Debug.Assert(fixtureA.Body.UserData == this.Entity);
            var entity2 = (Entity)fixtureB.Body.UserData;

            if (currentlyVisibleEntities.ContainsKey(entity2))
            {
                int value = currentlyVisibleEntities[entity2] - 1;
                currentlyVisibleEntities[entity2] = value;

                if (value == 0)
                {
                    OnSeperation?.Invoke(Name, entity2);
                    currentlyVisibleEntities.Remove(entity2);
                }
            }

        }

        public override void DebugDraw(DebugRenderer renderer, float elapsedSeconds, float totalSeconds)
        {
            debugShapeColor = SensedEntities.Count() > 0 ? Color.Aqua : Color.FloralWhite;
            base.DebugDraw(renderer, elapsedSeconds, totalSeconds);
        }

        private bool OnSensorCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            // we assume that fixtureA is always our self
            System.Diagnostics.Debug.Assert(fixtureA.Body.UserData == this.Entity);
            var entity2 = (Entity)fixtureB.Body.UserData;

            if (currentlyVisibleEntities.ContainsKey(entity2))
                    currentlyVisibleEntities[entity2]++;
            else
            {
                currentlyVisibleEntities.Add(entity2, 1);
                OnContact?.Invoke(Name, entity2, contact);
            }

            return true;
        }


        /// <summary>
        /// Filtered entities within the sensor
        /// </summary>
        public IEnumerable<Entity> SensedEntities
        {
          get { return currentlyVisibleEntities.Keys.Where(sensorFilter); }
        }

        /// <summary>
        /// All entities within the sensor
        /// </summary>
        public IEnumerable<Entity> AllEntities
        {
            get { return currentlyVisibleEntities.Keys; }
        }
    }
}
