using System.Collections.Generic;
using BroMakerLib.Infos;
using BroMakerLib.Storages;
using BroMakerLib.Unlocks;
using Newtonsoft.Json.Linq;
using RocketLib.Menus.Core;
using RocketLib.Menus.Elements;
using RocketLib.Menus.Layout;
using UnityEngine;

namespace BroMakerLib.Menus
{
    public class CustomBrosGridMenu : FlexMenu
    {
        private PaginatedGridContainer paginatedGrid;
        private TextElement titleText;
        private ActionButton backButton;
        private ActionButton previousButton;
        private ActionButton nextButton;
        private readonly List<BroCard> broCards = new List<BroCard>();

        protected int lastSeenStatusCount = 0;

        public override string MenuId => "BroMaker_CustomBrosGrid";
        public override string MenuTitle => "CUSTOM BROS";

        public static CustomBrosGridMenu Show(Menu parentMenu = null)
        {
            return FlexMenu.Show<CustomBrosGridMenu>(parentGame: parentMenu);
        }

        protected override void InitializeContainer()
        {
            base.InitializeContainer();

            EnableTransition = true;

            // Use LastFocused mode to remember position including navigation buttons
            FocusRestoreMode = FocusRestoreMode.LastFocused;

            // Main vertical container - minimal padding to use 95% of screen
            var mainContainer = new VerticalLayoutContainer
            {
                Name = "CustomBrosGrid_MainContainer",
                WidthMode = SizeMode.Fill,
                HeightMode = SizeMode.Fill,
                Padding = 5f,  // Minimal padding
                Spacing = 5f   // Minimal spacing to maximize grid space
            };

            // Title - larger font but compact height
            titleText = new TextElement("GridTitle")
            {
                Text = "CUSTOM BROS",
                TextColor = Color.white,
                FontSize = 9f,
                WidthMode = SizeMode.Fill,
                HeightMode = SizeMode.Fixed,
                Height = 35f  // Slightly bigger to fit larger font
            };
            mainContainer.AddChild(titleText);

            // Check if we have any bros first
            var bros = BroMakerStorage.Bros;
            bool hasBros = bros != null && bros.Count > 0;

            if (hasBros)
            {
                // Create paginated grid container for bros
                paginatedGrid = new PaginatedGridContainer
                {
                    Name = "CustomBrosGrid_PaginatedGrid",
                    Columns = 5,  // 5 columns (requirement)
                    Rows = 2,     // 2 rows (requirement)
                    GridSpacing = 50f,  // More spacing between cards
                    GridPadding = 10f,  // Reduced padding to use more space for cells
                    WidthMode = SizeMode.Fill,
                    HeightMode = SizeMode.Fill,
                    TransitionDuration = 0.6f,  // Smooth, slightly slower animation
                    EnableTransitions = true
                };
            }
            else
            {
                var topSpacer = new SpacerElement("TopSpacer")
                {
                    HeightMode = SizeMode.Fixed,
                    Height = 10f,
                };

                var noBrosText = new TextElement("NoBrosMessage")
                {
                    Text = "NO CUSTOM BROS INSTALLED",
                    TextColor = Color.gray,
                    FontSize = 4f,
                    WidthMode = SizeMode.Fixed,
                    Width = 100f,
                    HeightMode = SizeMode.Fixed,
                    Height = 40f,
                    HorizontalAlignmentOverride = HorizontalAlignment.Left
                };

                var middleSpacer = new SpacerElement("MiddleSpacer")
                {
                    HeightMode = SizeMode.Fill
                };

                backButton = new ActionButton("BackButton")
                {
                    Text = "BACK",
                    FontSize = 4.67f,
                    WidthMode = SizeMode.Fixed,
                    Width = 130f,
                    HeightMode = SizeMode.Fixed,
                    Height = 35f,
                    HorizontalAlignmentOverride = HorizontalAlignment.Center,
                    OnClick = () => GoBack()
                };

                mainContainer.AddChild(topSpacer);

                mainContainer.AddChild(noBrosText);

                mainContainer.AddChild(middleSpacer);

                mainContainer.AddChild(backButton);

                // Add main container and return early
                RootContainer.AddChild(mainContainer);
                return;
            }

            // Create navigation buttons for the paginated grid - smaller to save space
            previousButton = new ActionButton("PreviousPageButton")
            {
                Text = "<",
                FontSize = 4f,
                WidthMode = SizeMode.Fixed,
                Width = 40f,  // Smaller button
                HeightMode = SizeMode.Fixed,
                Height = 30f,  // Smaller button
                IsVisible = false, // Initially hidden
                IsFocusable = false // Initially not focusable
            };

            nextButton = new ActionButton("NextPageButton")
            {
                Text = ">",
                FontSize = 4f,
                WidthMode = SizeMode.Fixed,
                Width = 40f,  // Smaller button
                HeightMode = SizeMode.Fixed,
                Height = 30f,  // Smaller button
                IsVisible = false, // Initially hidden
                IsFocusable = false // Initially not focusable
            };

            // Setup navigation buttons with the paginated grid
            paginatedGrid.SetNavigationButtons(previousButton, nextButton);

            // Set parent menu reference for navigation refresh
            paginatedGrid.SetParentMenu(this);

            // Populate the grid with bro cards
            PopulateGrid();

            mainContainer.AddChild(paginatedGrid);

            // Back button container (centered at bottom) - smaller height
            var buttonContainer = new HorizontalLayoutContainer
            {
                Name = "ButtonContainer",
                WidthMode = SizeMode.Fill,
                HeightMode = SizeMode.Fixed,
                Height = 35f,  // Reduced from 60f
                ChildVerticalAlignment = VerticalAlignment.Center
            };

            // Spacer to center the back button
            var spacer = new SpacerElement("Spacer1")
            {
                WidthMode = SizeMode.Fill,
                HeightMode = SizeMode.Fixed,
                Height = 1f
            };
            buttonContainer.AddChild(spacer);

            // Back button - larger font
            backButton = new ActionButton("BackButton")
            {
                Text = "BACK",
                FontSize = 4.67f,  // Was FontSize 28 / 6
                WidthMode = SizeMode.Fixed,
                Width = 130f,
                HeightMode = SizeMode.Fixed,
                Height = 35f,
                OnClick = () => GoBack()
            };
            buttonContainer.AddChild(backButton);

            // Another spacer
            var spacer2 = new SpacerElement("Spacer2")
            {
                WidthMode = SizeMode.Fill,
                HeightMode = SizeMode.Fixed,
                Height = 1f
            };
            buttonContainer.AddChild(spacer2);

            mainContainer.AddChild(buttonContainer);

            RootContainer.AddChild(mainContainer);
        }

        private void PopulateGrid()
        {
            broCards.Clear();

            // Get all custom bros from BroMaker
            var bros = BroMakerStorage.Bros;

            // Create list of layout elements for the paginated grid
            var cardElements = new List<LayoutElement>();

            if (bros != null && bros.Count > 0)
            {
                // Create a BroCard for each actual bro
                foreach (var storedHero in bros)
                {
                    var broCard = CreateBroCard(storedHero);
                    broCards.Add(broCard);
                    cardElements.Add(broCard);
                }
            }

            // Set items in paginated grid container
            paginatedGrid.SetItems(cardElements);

            // Focus first element on the first page
            paginatedGrid.FocusFirstElement();

            lastSeenStatusCount = BroSpawnManager.BroStatusCount;
        }

        private BroCard CreateBroCard(StoredHero storedHero)
        {
            var card = new BroCard(storedHero.name)
            {
                BroName = storedHero.name,

                // Load avatar texture using BroMaker's ResourcesController
                AvatarTexture = GetAvatarTexture(storedHero),

                IsLocked = !BroUnlockManager.IsBroUnlocked(storedHero.name),

                IsSpawnEnabled = BroSpawnManager.IsBroEnabled(storedHero.name),

                // Visual properties
                LockedTintColor = new Color(0.15f, 0.15f, 0.15f, 1f),  // Much darker for locked

                // Size in grid - make cards bigger
                WidthMode = SizeMode.Percentage,
                Width = 100f,  // Use full cell width
                HeightMode = SizeMode.Percentage,
                Height = 100f,  // Use full cell height

                // Click handler - opens detail view
                OnClick = () => OpenDetailView(storedHero),

                // Store reference for refresh
                Tag = storedHero
            };

            return card;
        }

        private Texture2D GetAvatarTexture(StoredHero storedHero)
        {
            CustomBroInfo broInfo = storedHero.GetInfo() as CustomBroInfo;
            if (broInfo == null) return null;

            if (broInfo.parameters.ContainsKey("Avatar"))
            {
                string avatarPath = string.Empty;
                var value = broInfo.parameters["Avatar"];

                if (value is JArray array)
                {
                    avatarPath = array[0].ToObject<string>();
                }
                else if (value is string[] stringArray)
                {
                    avatarPath = stringArray[0];
                }
                else if (value is string valueString)
                {
                    avatarPath = valueString;
                }

                if (!string.IsNullOrEmpty(avatarPath))
                {
                    return ResourcesController.GetTexture(broInfo.path, avatarPath);
                }
            }

            return null;
        }

        protected override void Update()
        {
            base.Update();

            if (lastSeenStatusCount != BroSpawnManager.BroStatusCount)
            {
                this.RefreshBroCards();
            }
        }

        private void OpenDetailView(StoredHero storedHero)
        {
            CustomBrosDetailMenu.Show(storedHero, this);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            RefreshBroCards();
        }

        private void RefreshBroCards()
        {
            foreach (var broCard in broCards)
            {
                if (broCard.Tag is StoredHero storedHero)
                {
                    broCard.IsLocked = !BroUnlockManager.IsBroUnlocked(storedHero.name);
                    broCard.IsSpawnEnabled = BroSpawnManager.IsBroEnabled(storedHero.name);
                }
            }
        }

        protected override void OnDestroy()
        {
            broCards.Clear();
            base.OnDestroy();
        }
    }
}
