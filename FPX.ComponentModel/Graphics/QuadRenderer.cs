using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FPX.ComponentModel;

namespace FPX.Visual
{
    public class QuadRenderer : IGameComponent
    {
        public static QuadRenderer Instance { get; private set; }

        VertexPositionTexture[] vertecies;

        BasicEffect basicEffect;

        public QuadRenderer()
        {
            Instance = this;
        }

        public void Initialize()
        {
            vertecies = new VertexPositionTexture[4];
            vertecies[0].Position = -Vector2.One.ToVector3();
            vertecies[0].TextureCoordinate = Vector2.Zero;

            vertecies[1].Position = Vector3.Up - Vector3.Right;
            vertecies[1].TextureCoordinate = Vector2.UnitY;

            vertecies[2].Position = Vector3.Right - Vector3.Up;
            vertecies[2].TextureCoordinate = Vector2.UnitX;

            vertecies[3].Position = Vector3.Right + Vector3.Up;
            vertecies[3].TextureCoordinate = Vector2.One;

            basicEffect = new BasicEffect(GameCore.graphicsDevice);
            basicEffect.TextureEnabled = true;
        }

        public void RenderQuad(Texture2D texture, Vector2 position, Color blendColor)
        {
            GraphicsDevice device = GameCore.graphicsDevice;

            basicEffect.Texture = texture;
            basicEffect.DiffuseColor = blendColor.ToVector3();
            basicEffect.World = Matrix.CreateTranslation(-0.5f, -0.5f, 0.0f) * Matrix.CreateScale(texture.Width, texture.Height, 1.0f) * Matrix.CreateTranslation(position.ToVector3());
            basicEffect.View = Matrix.Identity;
            basicEffect.Projection = Matrix.CreateOrthographic(device.Viewport.Width, device.Viewport.Height, 0.0f, 1.0f);
            basicEffect.CurrentTechnique.Passes[0].Apply();

            device.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertecies, 0, 2);
        }

        public void RenderQuad(Texture2D texture, Vector2 position, Effect effect, bool useWVPPrams = true)
        {
            GraphicsDevice device = GameCore.graphicsDevice;

            device.SamplerStates[0] = SamplerState.AnisotropicWrap;
            device.Textures[0] = texture;
            if (useWVPPrams)
            {
                effect.Parameters["World"].SetValue(Matrix.CreateTranslation(position.ToVector3()));
                effect.Parameters["View"].SetValue(Matrix.Identity);
                effect.Parameters["Projection"].SetValue(Matrix.CreateOrthographic(device.Viewport.Width, device.Viewport.Height, 0.0f, 1.0f));
            }
            effect.CurrentTechnique.Passes[0].Apply();

            device.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertecies, 0, 2);
        }

        public void RenderQuad(Texture2D texture, Rectangle rect, Effect effect, bool useWVPPrams = true)
        {
            GraphicsDevice device = GameCore.graphicsDevice;

            device.SamplerStates[0] = SamplerState.AnisotropicWrap;
            device.Textures[0] = texture;

            if (useWVPPrams)
            {
                effect.Parameters["World"].SetValue(Matrix.CreateScale(rect.Width, rect.Height, 1.0f) * Matrix.CreateTranslation(rect.Location.ToVector2().ToVector3()));
                effect.Parameters["View"].SetValue(Matrix.Identity);
                effect.Parameters["Projection"].SetValue(Matrix.CreateOrthographic(device.Viewport.Width, device.Viewport.Height, -1.0f, 1.0f));
            }
            effect.CurrentTechnique.Passes[0].Apply();

            device.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertecies, 0, 2);
        }

        public void RenderQuad(Texture2D texture, Rectangle rect, Color blendColor)
        {
            GraphicsDevice device = GameCore.graphicsDevice;

            basicEffect.Texture = texture;
            basicEffect.DiffuseColor = blendColor.ToVector3();
            basicEffect.Alpha = blendColor.A / 255.0f;
            basicEffect.World = Matrix.CreateScale(rect.Width / 2.0f, rect.Height / 2.0f, 1.0f) * Matrix.CreateTranslation(rect.Location.ToVector2().ToVector3());
            basicEffect.View = Matrix.Identity;
            basicEffect.Projection = Matrix.CreateOrthographic(device.Viewport.Width, device.Viewport.Height, 0.0f, 1.0f);
            basicEffect.CurrentTechnique.Passes[0].Apply();

            device.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertecies, 0, 2);
        }
    }
}
