using BroMakerLib.Loggers;
using HarmonyLib;
using RocketLib;
using System;
using UnityEngine;

namespace BroMakerLib.Cutscenes
{
     [HarmonyPatch(typeof(CutsceneIntroRoot), "OnLoadComplete", typeof(string), typeof(object))]
     static class CutsceneIntroRoot_StartCutscene_Patch
     {

        static bool Prefix(CutsceneIntroRoot __instance, ref string resourceName, ref object asset, ref CutsceneIntroData ____curIntroData, ref Texture2D ____oldTex)
         {
            try
            {
                if (!CustomCutsceneController.willLoadCustomCutscene)
                    return true;

                asset = CustomCutsceneController.cutsceneToLoad.ToCutsceneIntroData(__instance);

                CutsceneIntroData _curIntroData = (CutsceneIntroData)asset;
                ____curIntroData = _curIntroData;

                CutsceneIntroDataExtra dataExtra = _curIntroData as CutsceneIntroDataExtra;
                bool hasExtraData = dataExtra != null;

                if (_curIntroData == null)
                {
                    __instance.EndCutscene();
                    return false;
                }

                // Heading
                UpdateText3D(__instance.headingMesh, _curIntroData.heading, _curIntroData.headingScale,
                    hasExtraData ? dataExtra.headingColor : CustomIntroCutscene.HeadingDefaultColor
                );

                // Subtitle 1
                UpdateText3D(__instance.subtitle1Mesh,
                    _curIntroData.subtitle1.IsNullOrEmpty() ? CustomIntroCutscene.Subtitle1DefaultText : _curIntroData.subtitle1,
                    _curIntroData.subtitleScale,
                    hasExtraData ? dataExtra.subtitle1Color : CustomIntroCutscene.SubtitleDefaultColor
                );

                // Create Subtitle 2
                if (__instance.subtitle2Mesh == null)
                {
                    const float SUBTITLE2_LOCALPOSITION_Y = -0.48f;

                    __instance.subtitle2Mesh = UnityEngine.Object.Instantiate<Text3D>(__instance.subtitle1Mesh, __instance.subtitle1Mesh.transform);
                    __instance.subtitle2Mesh.gameObject.SetActive(false);

                    __instance.subtitle2Mesh.Anchor = Text3D.AnchorPoint.TopMiddle;

                    Vector3 subtitle2LocalPosition = Vector2.zero;
                    // To make sure the 2 subtitles touch, we should multiply the Y position with the scale.
                    // So apparently to be aligned the position is -8 + 1/4 of the scale
                    subtitle2LocalPosition.y = SUBTITLE2_LOCALPOSITION_Y * (hasExtraData ? dataExtra.subtitle2Scale : 1);

                    __instance.subtitle2Mesh.transform.localPosition = subtitle2LocalPosition;
                    __instance.subtitle2Mesh.transform.localRotation = Quaternion.identity;

                    Renderer renderer = __instance.subtitle2Mesh.GetComponent<Renderer>();
                    renderer.material = new Material(renderer.sharedMaterial);
                }
                // Subtitle 2
                if (_curIntroData.subtitle2.IsNullOrEmpty())
                {
                    __instance.subtitle2Mesh.gameObject.SetActive(false);
                }
                else
                {
                    __instance.subtitle2Mesh.gameObject.SetActive(true);
                    UpdateText3D(__instance.subtitle2Mesh,
                        _curIntroData.subtitle2,
                        hasExtraData ? dataExtra.subtitle2Scale : _curIntroData.subtitleScale,
                        hasExtraData ? dataExtra.subtitle2Color : CustomIntroCutscene.SubtitleDefaultColor
                    );
                }

                // Sprite
                ____oldTex = __instance.spriteRenderer.material.mainTexture as Texture2D;
                __instance.spriteRenderer.material.mainTexture = _curIntroData.spriteTexture;

                SpriteSM spriteSM = __instance.spriteRenderer.gameObject.GetComponent<SpriteSM>();
                UpdateSpriteSM(spriteSM, _curIntroData);

                // Animation Clip
                __instance.anim.AddClip(_curIntroData.animClip, _curIntroData.animClip.name);
                __instance.anim.clip = _curIntroData.animClip;

                // Sounds
                __instance.barkSource.clip = _curIntroData.bark;
                if (__instance.fanfareSource != null && _curIntroData.introFanfare != null)
                {
                    __instance.fanfareSource.clip = _curIntroData.introFanfare;
                }

                __instance.cutsceneRoot.SetActive(true);

                // Animation
                UpdateAnimatedTexture(_curIntroData, __instance.spriteRenderer);
                return false;
            }
            catch (Exception ex)
            {
                BMLogger.ExceptionLog(ex);
            }
            return true;
        }

        private static void UpdateText3D(Text3D text3D, string text, float scale, Color color)
        {
            if (text3D == null)
                return;

            text3D.UpdateText(text);
            text3D.transform.localScale = new Vector3(scale, scale, scale);

            Renderer renderer = text3D.GetComponent<Renderer>();
            if (renderer == null)
                return;
            if (renderer.material == null)
                return;
            renderer.material.color = color;
        }

        private static void UpdateSpriteSM(SpriteSM spriteSM, CutsceneIntroData introData)
        {
            int lowerLeftPixelX = 0;
            int lowerLeftPixelY = introData.spriteTexture.height;
            int spriteWidth = introData.spriteTexture.width;
            int spriteHeight = introData.spriteTexture.height;
            if (introData.spriteRect.height > 0f)
            {
                lowerLeftPixelX = (int)introData.spriteRect.x;
                lowerLeftPixelY = (int)introData.spriteRect.y;
                spriteWidth = (int)introData.spriteRect.width;
                spriteHeight = (int)introData.spriteRect.height;
            }
            spriteSM.SetLowerLeftPixel((float)lowerLeftPixelX, (float)lowerLeftPixelY);
            spriteSM.SetPixelDimensions(spriteWidth, spriteHeight);
            if (introData.spriteSize.x > 0f)
            {
                spriteSM.SetSize(introData.spriteSize.x, introData.spriteSize.y);
            }
            spriteSM.RecalcTexture();
            spriteSM.CalcUVs();
            spriteSM.UpdateUVs();
        }

        private static void UpdateAnimatedTexture(CutsceneIntroData introData, Renderer renderer)
        {
            CutsceneIntroDataExtra dataExtra = introData as CutsceneIntroDataExtra;
            bool hasExtraData = dataExtra != null;

            AnimatedTexture animatedTexture = renderer.gameObject.GetOrAddComponent<AnimatedTexture>();

            if (animatedTexture == null)
                return;

            if (hasExtraData && !dataExtra.isAnimated)
            {
                animatedTexture.enabled = false;
                return;
            }

            if ( (hasExtraData && dataExtra.isAnimated) || introData.spriteAnimRateFramesWidth.x > 0f)
            {
                animatedTexture.frameRate = introData.spriteAnimRateFramesWidth.x;
                animatedTexture.frames = (int)introData.spriteAnimRateFramesWidth.y;
                animatedTexture.frameSpacingWidth = (int)introData.spriteAnimRateFramesWidth.z;
                animatedTexture.enabled = true;
                animatedTexture.Recalc();
                animatedTexture.Restart(); // Make sure that the animation replay each time
                animatedTexture.SetFrame();// Make sure the Sprite appears

                if (hasExtraData)
                {
                    animatedTexture.startDelay = dataExtra.animationStartDelay;
                    animatedTexture.loop = dataExtra.animationLoop;
                    animatedTexture.pingPong = dataExtra.animationPingPong;
                }
            }
            else
            {
                animatedTexture.enabled = false;
            }
        }
     }
}
