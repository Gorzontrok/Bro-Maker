using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BroMakerLib.CustomObjects
{
    [CustomObjectPreset("Halo_Default")]
    public class BroHaloM : BroHalo
    {
        protected SpriteSM sprite;
        protected float frameTimer;
        protected int frame;

        protected virtual void Awake()
        {
            RemoveOtherBrohaloComponents();
            sprite = GetComponent<SpriteSM>();
        }

        protected virtual void Start()
        {
            Hide();
        }

        protected virtual void Update()
        {
            frameTimer += Time.deltaTime;
            if (frameTimer >= frameRate)
            {
                frameTimer -= frameRate;
                frame++;
                if (frame == frames)
                {
                    frame = 0;
                }
                sprite.SetLowerLeftPixel(new Vector2((float)frame * sprite.width, sprite.height));
            }
        }

        public virtual new void Hide()
        {
            this.sprite.meshRender.enabled = false;
        }

        public virtual new void Show()
        {
            this.sprite.meshRender.enabled = true;
        }

        public virtual void Show(bool show)
        {
            this.sprite.meshRender.enabled = show;
        }

        protected void RemoveOtherBrohaloComponents()
        {
            var haloComponents = GetComponents<BroHalo>();
            foreach (var component in haloComponents)
            {
                if (component != this)
                {
                    Destroy(component);
                }
            }
        }
    }
}
