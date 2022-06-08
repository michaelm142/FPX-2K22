using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FPX.ComponentModel;

namespace FPX.Visual
{
    public class SkyboxRenderer : Component, IDrawable
    {
        public bool Visible => false;

        public int DrawOrder { get { return -1000; } }

        public event EventHandler<EventArgs> VisibleChanged;
        public event EventHandler<EventArgs> DrawOrderChanged;

        private TextureCube SkyCube;

        private Effect SkyCubeShader;

        private Model model;

        public void Start()
        {

            model = GameCore.content.Load<Model>("Models\\CubeInverse");
            SkyCubeShader = GameCore.content.Load<Effect>("Shaders\\CubeMapRender");
        }

        public void Draw(GameTime gameTime)
        {
            var device = GameCore.graphicsDevice;
            device.BlendState = BlendState.Opaque;
            device.Textures[4] = SkyCube;
            foreach (var mesh in model.Meshes)
            {
                SkyCubeShader.Parameters["World"].SetValue(Matrix.Identity);
                SkyCubeShader.Parameters["View"].SetValue(Matrix.Invert(Matrix.CreateFromQuaternion(Camera.Active.rotation)));
                SkyCubeShader.Parameters["Projection"].SetValue(Camera.Active.ProjectionMatrix);
                device.SamplerStates[4] = SamplerState.LinearClamp;
                SkyCubeShader.Parameters["SkyboxSampler"].SetValue(SkyCube);
                SkyCubeShader.CurrentTechnique.Passes[0].Apply();
                device.SetVertexBuffer(mesh.MeshParts[0].VertexBuffer);
                device.Indices = mesh.MeshParts[0].IndexBuffer;

                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, mesh.MeshParts[0].NumVertices, 0, mesh.MeshParts[0].PrimitiveCount);
            }
        }

        public override void LoadXml(XmlElement element)
        {
            base.LoadXml(element);

            var textureNode = element.SelectSingleNode("Texture");
            if (textureNode != null)
            {
                var textureAttr = textureNode.Attributes["Name"];

                if (textureAttr != null)
                    SkyCube = GameCore.content.Load<TextureCube>(textureAttr.Value);
            }
        }
    }
}
