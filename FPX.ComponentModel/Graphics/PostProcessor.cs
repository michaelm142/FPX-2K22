using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FPX.ComponentModel;

namespace FPX.Visual
{
    public class PostProcessor : Component
    {
        public List<Effect> ProcessLayers = new List<Effect>();

        private RenderTarget2D renderTarget;

        public override void LoadXml(XmlElement node)
        {
            foreach (XmlElement layer in node.SelectNodes("Layer"))
                ProcessLayers.Add(LoadLayer(layer));
        }

        public void Start()
        {
            renderTarget = new RenderTarget2D(GameCore.graphicsDevice, Screen.Width, Screen.Height, true, SurfaceFormat.Color, DepthFormat.Depth24);
        }

        public void Begin()
        {
            GameCore.graphicsDevice.SetRenderTarget(renderTarget);
        }

        public void End()
        {
            for (int i = 0; i < 4; i++)
                GameCore.graphicsDevice.SetRenderTarget((RenderTarget2D)null, 0);

            GameCore.graphicsDevice.Clear(Color.LightBlue);
            var prevRasterState = GameCore.graphicsDevice.RasterizerState;
            var prevBlendState = GameCore.graphicsDevice.BlendState;

            GameCore.graphicsDevice.BlendState = BlendState.Opaque;
            GameCore.graphicsDevice.RasterizerState = RasterizerState.CullNone;

            foreach (var layer in ProcessLayers)
            {
                layer.Parameters["Scene"].SetValue(renderTarget);
                var imageSizeParam = layer.Parameters.ToList().Find(p => p.Name == "iResolution");
                if (imageSizeParam != null)
                    imageSizeParam.SetValue(new Vector2(renderTarget.Width, renderTarget.Height));

                QuadRenderer.RenderQuad(renderTarget, new Rectangle(0, 0, Screen.Width / 2, -Screen.Height / 2), layer);
            }

            GameCore.graphicsDevice.RasterizerState = prevRasterState;
            GameCore.graphicsDevice.BlendState = prevBlendState;
        }

        private Effect LoadLayer(XmlElement node)
        {
            var filename = node.GetAttribute("FileName");
            if (string.IsNullOrEmpty(filename))
            {
                Debug.LogWarning("Post processing layer does not have filename attribute");
                return null;
            }

            Effect outval = GameCore.content.Load<Effect>(filename);

            foreach (XmlElement attrNode in node.SelectNodes("Parameter"))
            {
                var name = attrNode.GetAttribute("Name");
                var typeAttr = attrNode.GetAttribute("Type");
                if (string.IsNullOrEmpty(name))
                {
                    Debug.LogWarning("Effect parameter does not have name attribute");
                    return null;
                }

                var effectParameter = outval.Parameters[name];

                if (string.IsNullOrEmpty(typeAttr))
                {
                    int intValue = 0;
                    float floatValue = 0.0f;
                    string data = attrNode.InnerText;
                    if (int.TryParse(data, out intValue))
                        effectParameter.SetValue(intValue);
                    else if (float.TryParse(data, out floatValue))
                        effectParameter.SetValue(floatValue);
                }
                else
                {
                    if (typeAttr == "Texture")
                        effectParameter.SetValue(GameCore.content.Load<Texture2D>(attrNode.InnerText));
                    else
                        Debug.LogWarning("Unrecognized type attribute");
                }
            }

            return outval;
        }
    }
}
