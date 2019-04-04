using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Team6.Engine;
using Team6.Engine.Animations;
using Team6.Engine.Entities;
using Team6.Engine.Input;
using Team6.Engine.UI;
using Team6.Game.Misc;

namespace Team6.Game.Entities
{
    public class HUDListEntity : Entity
    {
        private readonly List<HUDTextComponent> itemComponents = new List<HUDTextComponent>();
        private readonly ListEntry[] items;

        public HUDListEntity(Scene scene, Vector2 position, float elementSize = 0.05f, float textSize = 0.05f, float spacing = 0.01f, float layerDepth = 1, bool allowAllControllers = false, bool isHorizontal = false, params ListEntry[] menuEntries) : base(scene, EntityType.UI, position)
        {
            items = menuEntries;
            // Layout and create the entries
            float totalFractionOfScreenSpace = (items.Length - 1) * elementSize + spacing * (items.Length - 2);
            float elementSizeWithSpacing = elementSize + spacing;

            float currentPosition = -totalFractionOfScreenSpace / 2;

            // [FOREACH PERFORMANCE] Should not allocate garbage
            foreach (var entry in items)
            {
                Vector2 position2d = isHorizontal ? new Vector2(currentPosition, 0) : new Vector2(0, currentPosition);

                itemComponents.Add(AddComponent(entry.TextComponent = new HUDTextComponent(Scene.MainFont,
                    textSize, entry.Caption, origin: new Vector2(0.5f, 0.5f), layerDepth: layerDepth, offset: position2d)));

                currentPosition += elementSizeWithSpacing;
            }

            // Create the input component
            if (allowAllControllers || Game.CurrentGameMode.PlayerInfos.Any(p => p.IsKeyboardPlayer))
            {
                AddComponent(new InputComponent(
                    new InputMapping(isHorizontal ? (Func<InputFrame, bool>)InputFunctions.KeyboardMenuLeft : InputFunctions.KeyboardMenuUp, GoUp),
                    new InputMapping(isHorizontal ? (Func<InputFrame, bool>)InputFunctions.KeyboardMenuRight : InputFunctions.KeyboardMenuDown, GoDown),
                    new InputMapping(InputFunctions.KeyboardMenuSelect, PerformSelectedAction)));
            }

            IEnumerable<int> gamePadsForControl = allowAllControllers ? Enumerable.Range(0, 3) : Game.CurrentGameMode.PlayerInfos.Where(p => !p.IsKeyboardPlayer).Select(p => p.GamepadIndex);

            // [FOREACH PERFORMANCE] ALLOCATES GARBAGE
            foreach (int gamePadIndex in gamePadsForControl)
            {
                AddComponent(new InputComponent(gamePadIndex,
                    new InputMapping(isHorizontal ? (Func<InputFrame, bool>)InputFunctions.MenuLeft : InputFunctions.MenuUp, GoUp),
                    new InputMapping(isHorizontal ? (Func<InputFrame, bool>)InputFunctions.MenuRight : InputFunctions.MenuDown, GoDown),
                    new InputMapping(InputFunctions.MenuSelect, PerformSelectedAction)));
            }

            SelectItem(0);
        }

        private void SelectItem(int index)
        {
            if (SelectedAction >= 0)
            {
                int old = SelectedAction;
                Scene.Dispatcher.AddAnimation(Animation.Get(1, 0, 0.3f, false, (t) => itemComponents[old].Color = Color.Lerp(Color.BlanchedAlmond, Color.Goldenrod, t), EasingFunctions.ToEaseOut(EasingFunctions.QuadIn)));
            }

            SelectedAction = index;
            Scene.Dispatcher.AddAnimation(Animation.Get(0, 1, 0.3f, false, (t) => itemComponents[index].Color = Color.Lerp(Color.BlanchedAlmond, Color.Goldenrod, t), EasingFunctions.ToEaseOut(EasingFunctions.QuadIn)));
        }

        private void PerformSelectedAction(InputFrame obj)
        {
            if (!Enabled)
                return;

            Scene.NonPositionalAudio.PlaySound("Audio/MenuActionSound");

            if (SelectedAction >= 0)
                items[SelectedAction].OnAction?.Invoke(items[SelectedAction]);
        }

        private void GoDown(InputFrame obj)
        {
            if (!Enabled)
                return;

            Scene.NonPositionalAudio.PlaySound("Audio/MenuActionSound");

            SelectItem((SelectedAction + 1) % items.Length);
        }

        private void GoUp(InputFrame obj)
        {
            if (!Enabled)
                return;

            Scene.NonPositionalAudio.PlaySound("Audio/MenuActionSound");

            SelectItem((SelectedAction - 1 + items.Length) % items.Length);
        }

        public bool Enabled { get; set; } = true;

        public int SelectedAction { get; private set; } = -1;

        public float Opacity
        {
            get { return itemComponents.FirstOrDefault()?.Opacity ?? 0; }
            set
            {
                // [FOREACH PERFORMANCE] Should not allocate garbage
                itemComponents.ForEach(t => t.Opacity = value);
            }
        }

        public class ListEntry
        {

            public ListEntry(string caption, Action<ListEntry> onAction)
            {
                this.Caption = caption;
                this.OnAction = onAction;
            }

            private string caption;

            public Action<ListEntry> OnAction { get; set; }
            public HUDTextComponent TextComponent { get; internal set; }
            public string Caption
            {
                get { return caption; }
                set
                {
                    caption = value;
                    if (TextComponent != null)
                        TextComponent.Text = value;
                }
            }
        }
    }
}
