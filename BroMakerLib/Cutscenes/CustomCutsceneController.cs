using System;
using BroMakerLib.Extensions;
using BroMakerLib.Loggers;
using UnityEngine;

namespace BroMakerLib.Cutscenes
{
    public static class CustomCutsceneController
    {
        public static bool willLoadCustomCutscene = false;
        public static bool finishCampaignAfterCutscene = false;
        public static CustomIntroCutscene cutsceneToLoad;
        private static bool backgroundApplied;
        private static CutsceneIntroRoot backgroundOwner;
        private static SpriteSM backgroundTarget;
        private static Material backgroundOriginalMaterial;
        private static Material backgroundCustomMaterial;
        private static Vector2 backgroundOriginalLowerLeftPixel;
        private static Vector2 backgroundOriginalPixelDimensions;
        private static GameObject backgroundOverlay;
        private static bool backgroundOverlayWasActive;

        /// <summary>Plays a custom bro's intro cutscene; `setFinishCampaignAfterCutscene` is ignored while the level editor is open.</summary>
        public static void LoadHeroCutscene(CustomIntroCutscene cutscene, float delay = 0.2f, bool setFinishCampaignAfterCutscene = false)
        {
            if (cutscene == null)
            {
                throw new ArgumentNullException(nameof(cutscene));
            }
            // The game's cutscene start ends a frozen pause only from the top pause menu; otherwise the cutscene's own pause drops the frozen level for good
            bool canEndPause = PauseController.pauseStatus == PauseStatus.MenuPause && (PauseMenu.instance == null || PauseMenu.instance.MenuActive);
            if (BroMakerUtilities.IsLevelFrozenByPause() && !canEndPause)
            {
                return;
            }
            cutsceneToLoad = cutscene;
            willLoadCustomCutscene = true;
            // In the level editor the game defers the finish until the scene unloads, so leaving play mode would end the campaign.
            if (setFinishCampaignAfterCutscene && !LevelEditorGUI.IsActive)
            {
                finishCampaignAfterCutscene = true;
            }
            CutsceneController.LoadCutScene(CutsceneName.Rambro, delay);
        }

        internal static void ApplyBackground(CutsceneIntroRoot root, Texture2D texture)
        {
            RestoreBackground(null);
            try
            {
                Transform background = root.cutsceneRoot.transform.Find("00_background");
                SpriteSM backgroundSprite = background != null ? background.GetComponent<SpriteSM>() : null;
                if (backgroundSprite == null)
                {
                    BMLogger.Warning("Couldn't find the intro cutscene's background, showing the flag instead");
                    return;
                }

                Renderer renderer = backgroundSprite.meshRender != null ? backgroundSprite.meshRender : backgroundSprite.GetComponent<Renderer>();
                Transform overlayTransform = background.Find("00_background 1");

                backgroundOwner = root;
                backgroundTarget = backgroundSprite;
                backgroundOriginalMaterial = renderer.sharedMaterial;
                backgroundOriginalLowerLeftPixel = backgroundSprite.lowerLeftPixel;
                backgroundOriginalPixelDimensions = backgroundSprite.pixelDimensions;
                backgroundOverlay = overlayTransform != null ? overlayTransform.gameObject : null;
                backgroundOverlayWasActive = backgroundOverlay != null && backgroundOverlay.activeSelf;
                backgroundApplied = true;

                backgroundCustomMaterial = new Material(backgroundOriginalMaterial) { mainTexture = texture };
                backgroundSprite.SetMaterialAndRefresh(backgroundCustomMaterial);
                SetPixelRect(backgroundSprite, new Vector2(0f, texture.height), new Vector2(texture.width, texture.height));
                if (backgroundOverlay != null)
                {
                    backgroundOverlay.SetActive(false);
                }
            }
            catch (Exception ex)
            {
                BMLogger.Warning("Failed to show the cutscene background, showing the flag instead", ex);
                RestoreBackground(null);
            }
        }

        internal static void RestoreBackground(CutsceneIntroRoot root)
        {
            if (!backgroundApplied || (root != null && backgroundOwner != null && root != backgroundOwner))
            {
                return;
            }

            try
            {
                if (backgroundTarget != null)
                {
                    backgroundTarget.SetMaterialAndRefresh(backgroundOriginalMaterial);
                    SetPixelRect(backgroundTarget, backgroundOriginalLowerLeftPixel, backgroundOriginalPixelDimensions);
                }
                if (backgroundOverlay != null)
                {
                    backgroundOverlay.SetActive(backgroundOverlayWasActive);
                }
            }
            catch (Exception ex)
            {
                BMLogger.Warning("Failed to put the flag back behind the intro cutscene", ex);
            }
            finally
            {
                if (backgroundCustomMaterial != null)
                {
                    UnityEngine.Object.Destroy(backgroundCustomMaterial);
                }
                backgroundApplied = false;
                backgroundOwner = null;
                backgroundTarget = null;
                backgroundOriginalMaterial = null;
                backgroundCustomMaterial = null;
                backgroundOverlay = null;
            }
        }

        private static void SetPixelRect(SpriteSM target, Vector2 lowerLeftPixel, Vector2 pixelDimensions)
        {
            target.SetLowerLeftPixel(lowerLeftPixel);
            target.SetPixelDimensions(pixelDimensions);
            target.CalcUVs();
            target.SetBleedCompensation();
        }
    }
}
