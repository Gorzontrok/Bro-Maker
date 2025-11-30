using BroMakerLib.Cutscenes;
using BroMakerLib.Infos;
using BroMakerLib.Loggers;
using BroMakerLib.Storages;
using BroMakerLib.Unlocks;
using RocketLib.Menus.Core;
using RocketLib.Menus.Elements;
using RocketLib.Menus.Layout;
using UnityEngine;

namespace BroMakerLib.Menus
{
    public class CustomBrosDetailMenu : FlexMenu
    {
        private StoredHero currentHero;
        private CustomBrosGridMenu parentGridMenu;

        private TextElement titleText;
        private ImageElement avatarImage;
        private TextElement statusText;
        private TextElement unlockLevelText;
        private TextElement rescueText;
        private HorizontalLayoutContainer levelContainer;
        private HorizontalLayoutContainer rescueContainer;
        private TextElement authorText;
        private TextElement spawnStatusText;
        private ActionButton spawnButton;
        private ActionButton playLevelButton;
        private ActionButton backButton;

        protected bool isEnabled;
        protected bool isLocked;
        protected int lastSeenStatusCount = 0;

        public override string MenuId => "BroMaker_CustomBrosDetail";
        public override string MenuTitle => "";

        public static CustomBrosDetailMenu Show(StoredHero hero, CustomBrosGridMenu gridMenu)
        {
            CustomBrosDetailMenu detailMenu = FlexMenu.Show<CustomBrosDetailMenu>(
                parentFlex: gridMenu,
                parentGame: null,
                instanceId: hero.name
            );

            detailMenu.currentHero = hero;
            detailMenu.parentGridMenu = gridMenu;
            detailMenu.LoadBroDetails();

            return detailMenu;
        }

        protected override void InitializeContainer()
        {
            base.InitializeContainer();

            var mainContainer = new VerticalLayoutContainer
            {
                Name = "DetailView_MainContainer",
                WidthMode = SizeMode.Fill,
                HeightMode = SizeMode.Fill,
                Padding = 0f,
                Spacing = 5f
            };

            RootContainer.AddChild(mainContainer);

            titleText = new TextElement("DetailTitle")
            {
                Text = "BRO NAME",
                TextColor = Color.white,
                FontSize = 10f,
                WidthMode = SizeMode.Fill,
                HeightMode = SizeMode.Auto,
            };
            mainContainer.AddChild(titleText);

            var mainRow = new HorizontalLayoutContainer
            {
                Name = "Main Row",
                WidthMode = SizeMode.Fill,
                HeightMode = SizeMode.Fixed,
                Height = 210f,
                Padding = 0f,
                HorizontalAlignmentOverride = HorizontalAlignment.Center
            };

            mainContainer.AddChild(mainRow);

            var leftMainRowSpacer = new SpacerElement
            {
                WidthMode = SizeMode.Percentage,
                Width = 3
            };

            mainRow.AddChild(leftMainRowSpacer);

            var avatarContainer = new VerticalLayoutContainer
            {
                Name = "Avatar Panel",
                WidthMode = SizeMode.Fill,
                HeightMode = SizeMode.Fill
            };

            mainRow.AddChild(avatarContainer);

            var avatarSpacer = new SpacerElement
            {
                HeightMode = SizeMode.Fixed,
                Height = -40
            };

            avatarContainer.AddChild(avatarSpacer);

            avatarImage = new ImageElement("AvatarImage")
            {
                WidthMode = SizeMode.Fixed,
                Width = 240,
                HeightMode = SizeMode.Fixed,
                Height = 240,
                VerticalAlignmentOverride = VerticalAlignment.Top
            };

            avatarContainer.AddChild(avatarImage);

            var mainRowMiddleSpacer = new SpacerElement
            {
                WidthMode = SizeMode.Fixed,
                Width = -20
            };

            mainRow.AddChild(mainRowMiddleSpacer);

            var infoPanel = new VerticalLayoutContainer
            {
                Name = "InfoPanel",
                WidthMode = SizeMode.Fill,
                HeightMode = SizeMode.Fill,
                Spacing = 5f,
                Padding = 0f
            };

            mainRow.AddChild(infoPanel);

            var infoPanelTopSpacer = new SpacerElement
            {
                HeightMode = SizeMode.Percentage,
                Height = 14
            };

            infoPanel.AddChild(infoPanelTopSpacer);

            var spawnContainer = CreateInfoRow("SPAWNING:", "");
            spawnStatusText = (spawnContainer.Children[2] as TextElement);
            infoPanel.AddChild(spawnContainer);

            var statusContainer = CreateInfoRow("STATUS:", "");
            statusText = (statusContainer.Children[2] as TextElement);
            infoPanel.AddChild(statusContainer);

            levelContainer = CreateInfoRow("UNLOCK LEVEL:", "");
            unlockLevelText = (levelContainer.Children[2] as TextElement);
            infoPanel.AddChild(levelContainer);

            rescueContainer = CreateInfoRow("UNLOCK REQUIREMENT:", "");
            rescueText = (rescueContainer.Children[2] as TextElement);
            infoPanel.AddChild(rescueContainer);

            var authorContainer = CreateInfoRow("AUTHOR:", "");
            authorText = (authorContainer.Children[2] as TextElement);
            infoPanel.AddChild(authorContainer);

            var bottomSpacer = new SpacerElement
            {
                HeightMode = SizeMode.Percentage,
                Height = 5
            };

            mainContainer.AddChild(bottomSpacer);

            var buttonContainer = new HorizontalLayoutContainer
            {
                Name = "ButtonContainer",
                WidthMode = SizeMode.Fill,
                HeightMode = SizeMode.Fixed,
                Height = 40f,
                Spacing = 15f,
                ChildVerticalAlignment = VerticalAlignment.Center
            };

            mainContainer.AddChild(buttonContainer);

            spawnButton = new ActionButton("SpawnToggleButton")
            {
                Text = "TOGGLE SPAWN",
                FontSize = 4.67f,
                WidthMode = SizeMode.Fixed,
                Width = 140f,
                HeightMode = SizeMode.Fixed,
                Height = 30f,
                OnClick = () => ToggleSpawn()
            };
            buttonContainer.AddChild(spawnButton);

            playLevelButton = new ActionButton("PlayLevelButton")
            {
                Text = "PLAY UNLOCK LEVEL",
                FontSize = 4.67f,
                WidthMode = SizeMode.Fixed,
                Width = 160f,
                HeightMode = SizeMode.Fixed,
                Height = 30f,
                OnClick = () => PlayUnlockLevel(),
                IsVisible = false
            };
            buttonContainer.AddChild(playLevelButton);

            var buttonSpacer = new SpacerElement("ButtonSpacer")
            {
                WidthMode = SizeMode.Fill,
                HeightMode = SizeMode.Fixed,
                Height = 1f
            };
            buttonContainer.AddChild(buttonSpacer);

            backButton = new ActionButton("DetailBackButton")
            {
                Text = "BACK",
                FontSize = 4.67f,
                WidthMode = SizeMode.Fixed,
                Width = 100f,
                HeightMode = SizeMode.Fixed,
                Height = 30f,
                OnClick = () => GoBack()
            };
            buttonContainer.AddChild(backButton);
        }

        private HorizontalLayoutContainer CreateInfoRow(string label, string value)
        {
            var row = new HorizontalLayoutContainer
            {
                HeightMode = SizeMode.Fixed,
                Height = 30f,
                WidthMode = SizeMode.Fill,
                Spacing = 3f
            };

            var labelText = new TextElement($"Label_{label}")
            {
                Text = label,
                TextColor = Color.gray,
                FontSize = 4f,
                WidthMode = SizeMode.Auto,
                HeightMode = SizeMode.Fill
            };

            var middleSpacer = new SpacerElement
            {
                WidthMode = SizeMode.Fill
            };

            var valueText = new TextElement($"Value_{label}")
            {
                Text = value,
                TextColor = Color.white,
                FontSize = 4f,
                WidthMode = SizeMode.Auto,
                HeightMode = SizeMode.Fill
            };

            var rightSpacer = new SpacerElement
            {
                WidthMode = SizeMode.Percentage,
                Width = 8
            };

            row.AddChild(labelText);
            row.AddChild(middleSpacer);
            row.AddChild(valueText);
            row.AddChild(rightSpacer);
            return row;
        }

        private void LoadBroDetails()
        {
            StoredHero storedHero = currentHero;
            if (storedHero.name == null)
            {
                return;
            }

            titleText.Text = storedHero.name.ToUpper();

            GetCutsceneTexture(storedHero);

            if (storedHero.mod != null)
            {
                authorText.Text = !string.IsNullOrEmpty(storedHero.mod.Author) ? storedHero.mod.Author : "Unknown";
            }
            else
            {
                authorText.Text = "Unknown";
            }

            RefreshBroDetails(true);
        }

        private void GetCutsceneTexture(StoredHero storedHero)
        {
            CustomBroInfo broInfo = storedHero.GetInfo() as CustomBroInfo;
            if (broInfo == null)
            {
                return;
            }

            Texture2D tex = null;
            if (broInfo.Cutscene != null && broInfo.Cutscene.Count > 0 && !string.IsNullOrEmpty(broInfo.Cutscene[0].spritePath))
            {
                CustomIntroCutscene cutscene = broInfo.Cutscene[0];
                tex = ResourcesController.GetTexture(broInfo.path, cutscene.spritePath);

                if (tex != null)
                {
                    // If this is an animated cutscene, configure the ImageElement to show just one frame
                    if (cutscene.isAnimated)
                    {
                        int frames = (int)cutscene.spriteAnimRateFramesWidth.y;

                        avatarImage.PixelDimensions = new Vector2(cutscene.spriteRect.width, cutscene.spriteRect.height);

                        avatarImage.LowerLeftPixel = new Vector2((frames - 1) * cutscene.spriteRect.width, cutscene.spriteRect.height);
                    }
                    else
                    {
                        // For non-animated cutscenes, reset to defaults
                        avatarImage.PixelDimensions = null;
                        avatarImage.LowerLeftPixel = null;
                    }

                    avatarImage.SpriteOffset = cutscene.spriteMenuOffset;

                    avatarImage.Texture = tex;
                }
            }
        }

        protected override void Update()
        {
            base.Update();

            // Check if an bro status has changed
            if (lastSeenStatusCount != BroSpawnManager.BroStatusCount)
            {
                RefreshBroDetails();
            }
        }

        private void ToggleSpawn()
        {
            if (currentHero.name == null) return;

            // Don't allow toggling for locked bros
            if (isLocked) return;

            isEnabled = !isEnabled;
            BroSpawnManager.SetBroEnabled(currentHero.name, isEnabled);
            Settings.instance?.Save(Main.mod);


            RefreshBroDetails();
        }

        private void RefreshBroDetails(bool firstLoad = false)
        {
            StoredHero storedHero = currentHero;
            if (storedHero.name == null)
            {
                return;
            }
            bool previousUnlocked = isLocked;
            bool previousEnabled = isEnabled;
            isLocked = !BroUnlockManager.IsBroUnlocked(storedHero.name);
            isEnabled = BroSpawnManager.IsBroEnabled(storedHero.name);
            if (firstLoad || previousUnlocked != isLocked || previousEnabled != isEnabled)
            {
                statusText.Text = isLocked ? "LOCKED" : "UNLOCKED";
                statusText.TextColor = isLocked ? Color.red : Color.green;

                var unlockState = BroUnlockManager.GetBroUnlockState(storedHero.name);
                if (unlockState != null)
                {
                    // Show unlock level if the bro has an unlock level, regardless of lock status
                    bool hasUnlockLevel = (unlockState.ConfiguredMethod == UnlockMethod.UnlockLevel ||
                                          unlockState.ConfiguredMethod == UnlockMethod.RescueOrLevel) &&
                                          (!string.IsNullOrEmpty(unlockState.UnlockLevelName) ||
                                           !string.IsNullOrEmpty(unlockState.UnlockLevelPath));

                    if (hasUnlockLevel)
                    {
                        unlockLevelText.Text = !string.IsNullOrEmpty(unlockState.UnlockLevelName) ?
                            unlockState.UnlockLevelName : "Custom Level";
                        levelContainer.IsVisibleAndPositioned = true;
                    }
                    else
                    {
                        levelContainer.IsVisibleAndPositioned = false;
                    }

                    // Only show requirements if bro is locked and has rescue requirements with more than 0 rescues remaining
                    if (isLocked &&
                        (unlockState.ConfiguredMethod == UnlockMethod.RescueCount ||
                         unlockState.ConfiguredMethod == UnlockMethod.RescueOrLevel))
                    {
                        int remaining = BroUnlockManager.GetRemainingRescues(storedHero.name);
                        if (remaining > 1)
                        {
                            rescueText.Text = $"Rescue {remaining} more bros" + (unlockState.ConfiguredMethod == UnlockMethod.RescueOrLevel ? "\nOr beat the unlock level" : "");
                            rescueContainer.IsVisibleAndPositioned = true;
                        }
                        else if (remaining == 1)
                        {
                            rescueText.Text = "Rescue 1 more bro" + (unlockState.ConfiguredMethod == UnlockMethod.RescueOrLevel ? "\nOr beat the unlock level" : "");
                            rescueContainer.IsVisibleAndPositioned = true;
                        }
                        else
                        {
                            rescueContainer.IsVisibleAndPositioned = false;
                        }
                    }
                    else
                    {
                        rescueContainer.IsVisibleAndPositioned = false;
                    }

                    playLevelButton.IsVisible = hasUnlockLevel;
                    playLevelButton.IsFocusable = playLevelButton.IsVisible;
                }
                else
                {
                    levelContainer.IsVisibleAndPositioned = false;
                    rescueContainer.IsVisibleAndPositioned = false;
                    playLevelButton.IsVisible = false;
                    playLevelButton.IsFocusable = false;
                }

                isEnabled = BroSpawnManager.IsBroEnabled(storedHero.name);
                spawnStatusText.Text = isEnabled ? "ENABLED" : "DISABLED";
                spawnStatusText.TextColor = isEnabled ? Color.green : Color.red;

                // Update button text and state based on lock status
                if (isLocked)
                {
                    spawnButton.Text = "LOCKED";
                    spawnButton.IsEnabled = false;
                    spawnButton.IsFocusable = false;
                }
                else
                {
                    spawnButton.Text = isEnabled ? "DISABLE SPAWN" : "ENABLE SPAWN";
                    spawnButton.IsEnabled = true;
                    spawnButton.IsFocusable = true;
                }

                this.RefreshLayout();
            }

            lastSeenStatusCount = BroSpawnManager.BroStatusCount;
        }

        private void PlayUnlockLevel()
        {
            FlexMenu.SetReturnTarget(parentGridMenu);

            if (!BroUnlockManager.LoadUnlockLevel(currentHero.name))
            {
                BMLogger.Error($"Failed to load unlock level for {currentHero.name}");
            }
        }

        protected override void OnDestroy()
        {
            // Clear references
            parentGridMenu = null;

            base.OnDestroy();
        }
    }
}
