using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Team6.Engine;
using Team6.Engine.Entities;
using Team6.Engine.Components;
using Team6.Engine.Input;
using Team6.Engine.UI;
using Team6.Engine.Audio;
using Team6.Game.Mode;
using Team6.Engine.Graphics2d;
using Team6.Game.Components;
using Team6.Game.Misc;
using Team6.Game.Entities;
using System;
using Team6.Engine.Animations;

namespace Team6.Game.Scenes
{
    public class WinScene : Scene
    {
        private HUDListEntity continueMenuList;
        private HUDComponent continueBackground;
        private bool fadedIn;

        public WinScene(MainGame game) : base(game)
        {

        }

        public override void LoadContent()
        {
            base.LoadContent();

            if (Game.CurrentGameMode.PlayerMode == PlayerMode.Free4All)
                LoadContentFree4All();
            else
                LoadContent2vs2();

            HUDComponent vignetteComponent = new HUDComponent(Game.Debug.DebugRectangle, Vector2.One, layerDepth: -0.8f)
            {
                MaintainAspectRation = false,
                OnVirtualUIScreen = false,
                Color = Color.FromNonPremultiplied(22, 59, 59, 163)
            };
            AddEntity(new Entity(this, EntityType.UI, vignetteComponent));

            var backgroundSize = new Vector2(6f / 3.508f * 1.1f * GameConstants.ScreenHeight, 1.1f * GameConstants.ScreenHeight);
            AddEntity(new Entity(this, EntityType.Game, new SpriteComponent("background", backgroundSize, new Vector2(0.5f, 0.5f), layerDepth: -1.0f)));
            AddEntity(new Entity(this, EntityType.Game, new SpriteComponent("trees_border", backgroundSize, new Vector2(0.5f, 0.5f), layerDepth: 0.9f)));
            var inputEntity = new Entity(this, EntityType.LayerIndependent);

            // [FOREACH PERFORMANCE] Should not allocate garbage
            foreach (var player in Game.CurrentGameMode.PlayerInfos)
            {
                if (player.IsKeyboardPlayer)
                {
                    inputEntity.AddComponent(new InputComponent(player.GamepadIndex,
                        new InputMapping(InputFunctions.KeyboardMenuStart, FadeInMenu)
                        ));
                }
                else
                {
                    inputEntity.AddComponent(new InputComponent(player.GamepadIndex,
                       new InputMapping(InputFunctions.MenuStart, FadeInMenu)
                       ));
                }
            }
            AddEntity(inputEntity);


            HUDTextComponent pressStartText = new HUDTextComponent(MainFont, 0.04f, "press start to continue", offset: new Vector2(0.5f, 0.9f), origin: new Vector2(0.5f, 0.5f), layerDepth: 0f)
            {
                Opacity = 0
            };
            Dispatcher.AddAnimation(Animation.Get(0, 1, 1.5f, true, val => pressStartText.Opacity = val, EasingFunctions.ToLoop(EasingFunctions.QuadIn), delay: 5f));
            AddEntity(new Entity(this, EntityType.UI, pressStartText));

            // Add continue overlay
            AddEntity(new Entity(this, EntityType.UI, continueBackground = new HUDComponent(Game.Debug.DebugRectangle, Vector2.One, layerDepth: 0.99f)
            {
                Color = Color.Black,
                Opacity = 0,
                MaintainAspectRation = false,
                OnVirtualUIScreen = false
            }));

            AddEntity(continueMenuList = new HUDListEntity(this, new Vector2(0.5f, 0.5f), layerDepth: 1f,
 menuEntries: new[] { new HUDListEntity.ListEntry("Rematch", Rematch),new HUDListEntity.ListEntry("Rejoin", Rejoin)
                ,new HUDListEntity.ListEntry("Back to main menu", BackToMainMenu)})
            {
                Enabled = false,
                Opacity = 0
            });

            AddEntity(new Entity(this, EntityType.LayerIndependent, new CenterCameraComponent(Game.Camera)));

            this.TransitionIn();
        }

        private void LoadContent2vs2()
        {
            PlayerAndGameInfo currentGameMode = Game.CurrentGameMode;
            var teams = currentGameMode.PlayerInfos.ToLookup(p => p.TeamIndex);
            var scoreTeam1 = Tuple.Create(teams[1].Sum(p => p.TotalScore), teams[1].Sum(p => p.NumberOfBoars), teams[1].Sum(p => p.NumberOfChickens));
            var scoreTeam2 = Tuple.Create(teams[2].Sum(p => p.TotalScore), teams[2].Sum(p => p.NumberOfBoars), teams[2].Sum(p => p.NumberOfChickens));

            bool isDraw = scoreTeam2.Item1 == scoreTeam1.Item1;
            string winningSuffix = isDraw ? "" : " wins";

            if (scoreTeam1.Item1 >= scoreTeam2.Item1)
                CreateWinningImage("greenPurpleWins", "Team 1" + winningSuffix, scoreTeam1.Item1, scoreTeam1.Item2, scoreTeam1.Item3, new Vector2(0.25f, 0.5f), 0.5f, 0);
            else
                CreateLoosingImage("greenPurplelost", true, "Team 1", scoreTeam1.Item1, scoreTeam1.Item2, scoreTeam1.Item3, new Vector2(0.25f, 0.5f), 0.5f, 0);


            if (scoreTeam2.Item1 >= scoreTeam1.Item1)
                CreateWinningImage("yellowRedWins", "Team 2" + winningSuffix, scoreTeam2.Item1, scoreTeam2.Item2, scoreTeam2.Item3, new Vector2(0.75f, 0.5f), 0.5f, 0);
            else
                CreateLoosingImage("redyellowlost", true, "Team 2", scoreTeam2.Item1, scoreTeam2.Item2, scoreTeam2.Item3, new Vector2(0.75f, 0.5f), 0.5f, 0);

            if (isDraw)
                ShowMidText("It's a DRAW!");
        }

        private void ShowMidText(string value)
        {
            HUDTextComponent midText = new HUDTextComponent(MainFont, 0.06f, value, offset: new Vector2(0.5f, 0.1f), origin: new Vector2(0.5f, 0.5f), layerDepth: 0f);
            AddEntity(new Entity(this, EntityType.UI, midText));
        }

        private void LoadContentFree4All()
        {
            PlayerInfo[] inWinningOrder = Game.CurrentGameMode.PlayerInfos.OrderByDescending(p => p.TotalScore).ToArray();

            int winningScore = inWinningOrder[0].TotalScore;

            int winners = inWinningOrder.TakeWhile(p => p.TotalScore == winningScore).Count();
            int loosers = inWinningOrder.Length - winners;

            string winningSuffix = loosers == 0 ? "" : " wins";

            float spaceBetween = 1f / (winners + 1);

            float winnerScale = 0.4f;

            switch (winners)
            {
                case 1:
                    winnerScale = 0.65f;
                    break;
                case 2:
                    winnerScale = 0.5f;
                    break;
                case 3:
                    winnerScale = loosers > 0 ? 0.45f : 0.4f;
                    break;
            }

            for (int i = 0; i < winners; i++)
            {
                Vector2 pos;
                if (loosers > 0)
                    pos = Vector2.Lerp(new Vector2(0.4f, 0), Vector2.One, spaceBetween * (i + 1));
                else
                    pos = Vector2.Lerp(new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), spaceBetween * (i + 1));

                string winnerAsset = GetWinningAssetFree4All(inWinningOrder[i]);

                CreateWinningImage(winnerAsset, inWinningOrder[i].Name + winningSuffix, inWinningOrder[i].TotalScore, inWinningOrder[i].NumberOfBoars, inWinningOrder[i].NumberOfChickens, pos, winnerScale, (i - 1) * 0.2f);
            }

            spaceBetween = 1f / (loosers + 1);

            for (int i = winners; i < inWinningOrder.Length; i++)
            {
                Vector2 pos;
                pos = Vector2.Lerp(new Vector2(0.25f, 0), new Vector2(0.25f, 1f), spaceBetween * (i - winners + 1));

                string looserAsset = GetLoosingAssetFree4All(inWinningOrder[i]);

                CreateLoosingImage(looserAsset, false, inWinningOrder[i].Name, inWinningOrder[i].TotalScore, inWinningOrder[i].NumberOfBoars, inWinningOrder[i].NumberOfChickens, pos, 0.45f, (i - 1) * 0.2f);
            }

            if (loosers == 0 && winners > 1)
                ShowMidText("It's a DRAW!");
        }

        private void CreateLoosingImage(string asset, bool isWide, string name, int totalScore, int boars, int chickens, Vector2 position, float scale, float baseLayerDepth)
        {

            Vector2 imageSize;
            Vector2 textPosition;
            Vector2 totalPosition;
            Vector2 leftPosition;
            Vector2 rightPosition;
            if (isWide) // team looser graphics are wider to accomodate both players
            {
                imageSize = new Vector2(1f, 0.3864090606262492f);
                textPosition = (new Vector2(0.352f, 0.275f) - Vector2.One / 2f) * imageSize * scale;
                totalPosition = (new Vector2(0.355f, 0.7f) - Vector2.One / 2f) * imageSize * scale;
                leftPosition = (new Vector2(0.575f, 0.81f) - Vector2.One / 2f) * imageSize * scale;
                rightPosition = (new Vector2(0.675f, 0.81f) - Vector2.One / 2f) * imageSize * scale;
            }
            else
            {
                imageSize = new Vector2(1f, 0.4531f);
                textPosition = (new Vector2(0.362f, 0.275f) - Vector2.One / 2f) * imageSize * scale;
                totalPosition = (new Vector2(0.362f, 0.7f) - Vector2.One / 2f) * imageSize * scale;
                leftPosition = (new Vector2(0.575f, 0.81f) - Vector2.One / 2f) * imageSize * scale;
                rightPosition = (new Vector2(0.72f, 0.81f) - Vector2.One / 2f) * imageSize * scale;
            }

            HUDComponent looserGraphic = new HUDComponent(asset, imageSize * scale, new Vector2(0.5f, 0.5f), layerDepth: baseLayerDepth);
            Entity looserEntity = new Entity(this, EntityType.UI, position,
                looserGraphic,
                new HUDTextComponent(MainFont, 0.08f * scale, name, new Vector2(0.5f, 0.5f), layerDepth: baseLayerDepth + 0.02f, offset: textPosition),
                new HUDTextComponent(MainFont, 0.17f * scale, totalScore.ToString(), new Vector2(0.5f, 0.5f), layerDepth: baseLayerDepth + 0.02f, offset: totalPosition),
                new HUDTextComponent(MainFont, 0.05f * scale, boars.ToString(), new Vector2(0.5f, 0.5f), layerDepth: baseLayerDepth + 0.02f, offset: leftPosition),
                new HUDTextComponent(MainFont, 0.05f * scale, chickens.ToString(), new Vector2(0.5f, 0.5f), layerDepth: baseLayerDepth + 0.02f, offset: rightPosition)
                );
            AddEntity(looserEntity);
        }

        private static string GetLoosingAssetFree4All(PlayerInfo playerInfo)
        {
            string looserGraphicName;

            switch (playerInfo.Color)
            {
                case PlayerColors.Green:
                    looserGraphicName = "greenlost";
                    break;
                case PlayerColors.Purple:
                    looserGraphicName = "purplelost";
                    break;
                case PlayerColors.Pink:
                    looserGraphicName = "redlost";
                    break;
                case PlayerColors.Yellow:
                    looserGraphicName = "yellowlost";
                    break;
                default:
                    throw new NotSupportedException();
            }

            return looserGraphicName;
        }

        private void CreateWinningImage(string asset, string name, int totalScore, int boars, int chickens, Vector2 position, float scale, float baseLayerDepth)
        {
            HUDComponent winnerComponent = new HUDComponent("WinOverlay", Vector2.One * scale, new Vector2(0.5f, 0.5f), layerDepth: baseLayerDepth + 0.01f);

            // pixel perfect, taken from graphic
            Vector2 textPosition = (new Vector2(0.5f, 0.65f) - Vector2.One / 2f) * scale;
            Vector2 totalPosition = (new Vector2(0.488f, 0.85f) - Vector2.One / 2f) * scale;
            Vector2 leftPosition = (new Vector2(0.455f, 0.955f) - Vector2.One / 2f) * scale;
            Vector2 rightPosition = (new Vector2(0.56f, 0.955f) - Vector2.One / 2f) * scale;

            Entity winnerEntity = new Entity(this, EntityType.UI, position,
                new HUDComponent(asset, Vector2.One * scale, new Vector2(0.5f, 0.5f), layerDepth: baseLayerDepth),
                winnerComponent,
                new HUDTextComponent(MainFont, 0.12f * scale, name, new Vector2(0.5f, 0.5f), layerDepth: baseLayerDepth + 0.02f, offset: textPosition),
                new HUDTextComponent(MainFont, 0.17f * scale, totalScore.ToString(), new Vector2(0.5f, 0.5f), layerDepth: baseLayerDepth + 0.02f, offset: totalPosition),
                new HUDTextComponent(MainFont, 0.05f * scale, boars.ToString(), new Vector2(0.5f, 0.5f), layerDepth: baseLayerDepth + 0.02f, offset: leftPosition),
                new HUDTextComponent(MainFont, 0.05f * scale, chickens.ToString(), new Vector2(0.5f, 0.5f), layerDepth: baseLayerDepth + 0.02f, offset: rightPosition)
                );
            AddEntity(winnerEntity);
        }

        private static string GetWinningAssetFree4All(PlayerInfo playerInfo)
        {
            string winnerGraphic;

            switch (playerInfo.Color)
            {
                case PlayerColors.Green:
                    winnerGraphic = "greenWins";
                    break;
                case PlayerColors.Purple:
                    winnerGraphic = "purpleWins";
                    break;
                case PlayerColors.Pink:
                    winnerGraphic = "redWins";
                    break;
                case PlayerColors.Yellow:
                    winnerGraphic = "yellowWins";
                    break;
                default:
                    throw new NotSupportedException();
            }

            return winnerGraphic;
        }

        private void FadeInMenu(InputFrame obj)
        {
            if (fadedIn)
                return;
            fadedIn = true;


            Dispatcher.AddAnimation(Animation.Get(0, 1, 0.2f, false, (value) =>
            {
                continueMenuList.Opacity = value;
                continueBackground.Opacity = value * 0.7f;
            }, EasingFunctions.CubicIn)).Then(() =>
            {
                continueMenuList.Enabled = true;
            });
        }

        private void BackToMainMenu(HUDListEntity.ListEntry obj)
        {
            this.TransitionOutAndSwitchScene(new MainMenuScene(Game));
        }

        private void Rejoin(HUDListEntity.ListEntry obj)
        {
            this.TransitionOutAndSwitchScene(new JoinScene(Game));
        }

        private void Rematch(HUDListEntity.ListEntry obj)
        {
            if (this.Game.CurrentGameMode.GameMode == GameMode.BigHerd)
            {
                this.TransitionOutAndSwitchScene(new BasicGameScene(this.Game));
            }
            else if (this.Game.CurrentGameMode.GameMode == GameMode.Waves)
            {
                this.TransitionOutAndSwitchScene(new RightToLeftGameScene(this.Game));
            }
        }

        public override void OnShown()
        {
            base.OnShown();

            Game.EngineComponents.Get<AudioManager>().PlaySong("Audio\\joinScreen");
            Dispatcher.Delay(0.5f, () => this.NonPositionalAudio.PlaySound("Audio/cheer"));
        }

    }
}