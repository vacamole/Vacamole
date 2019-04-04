using System;
using Microsoft.Xna.Framework;
using Team6.Engine.Components;
using Team6.Engine.Entities;
using Team6.Game.Components.AI;
using Team6.Game.Mode;
using Team6.Game.Scenes;
using System.Linq;
using FarseerPhysics.Dynamics.Contacts;
using Team6.Engine.Graphics2d;
using Team6.Game.Entities;
using FarseerPhysics.Dynamics;
using Team6.Engine.Audio;
using Team6.Game.Misc;

namespace Team6.Game.Components
{
    public class BarnComponent : Component
    {
        private PlayerInfo owner;

        public BarnComponent(PlayerInfo playerInfo)
        {
            this.owner = playerInfo;
        }

        public override void Initialize()
        {
            var enclosireSensor = Entity.GetComponentByName<SensorComponent>("enclosure");
            enclosireSensor.Fixture.CollisionCategories = GameConstants.CollisionCategoryBarn;
            enclosireSensor.OnContact += OnContact;
        }

        private void OnContact(string sensorName, Entity obj, Contact contact)
        {
            if (obj.IsAnimal() && obj.HasComponent<IAIComponent>())
            {
                // remove AI
                obj.RemoveAllComponents<IAIComponent>();
                obj.Body.Mass = 0;
                obj.AddComponent(new StayInBarnComponent(Entity.Body.Position));

                if (obj is ChickenEntity)
                {
                    this.Entity.GetComponent<AudioSourceComponent>().PlaySound("Audio/countDownLong");
                    this.Entity.Game.CurrentGameMode.AddChickenToPlayer(owner);
                }
                else
                {
                    this.Entity.GetComponent<AudioSourceComponent>().PlaySound("Audio/PigHappy");
                    this.Entity.Game.CurrentGameMode.AddBoarToPlayer(owner);
                }
                Entity.GetComponent<TextDisplayComponent>().Text = "" + owner.TotalScore;

                AnimalAdded?.Invoke(owner, obj);
            }
        }

        public event Action<PlayerInfo, Entity> AnimalAdded;
    }
}
