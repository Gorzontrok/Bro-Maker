using UnityEngine;

namespace BroMakerLib.Cutscenes
{
    public class CutsceneIntroDataExtra : CutsceneIntroData
    {
        public Color headingColor = Color.black;
        public Color subtitle1Color = Color.black;
        public Color subtitle2Color = Color.black;

        public bool isAnimated = false;
        public float animationStartDelay = 0.0f;
        public bool animationPingPong = false;
        public bool animationLoop = false;
        public Texture2D backgroundTexture = null;
    }
}
