using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LodeObj;
using FPX.ComponentModel;
using FPX.Visual;

namespace FPX
{
    public class Graphics : IGameComponent, IDrawable, IDisposable
    {
        public bool Visible { get; set; } = true;

        public int DrawOrder { get { return 0; } }

        public event EventHandler<EventArgs> VisibleChanged;
        public event EventHandler<EventArgs> DrawOrderChanged;

        public static Graphics instance { get; private set; }

        public DeferredRenderer renderer { get; private set; }

        public static RenderMode Mode = RenderMode.Basic;

        private Effect clearDepthShader;

        private Texture2D transparentTexture;

        public PrimitiveType fillMode = PrimitiveType.TriangleList;

        public Graphics()
        {
            instance = this;
        }


        public void Initialize()
        {
            Mode = Settings.GetSetting<RenderMode>("RenderMode");
            Debug.Log("Current Render Mode: {0}", Mode);

            if (Mode == RenderMode.Deferred || Mode == RenderMode.DeferredDebug)
                renderer = new DeferredRenderer();
            clearDepthShader = GameCore.content.Load<Effect>("Shaders\\ClearDepth");
            transparentTexture = new Texture2D(GameCore.graphicsDevice, 1, 1);
            transparentTexture.SetData(new Color[] { Color.Transparent });

            fillMode = Settings.GetSetting<PrimitiveType>("FillMode");

            GameCore.gameInstance.Components.Add(new QuadRenderer());
            GameCore.graphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;
        }

        public static void ClearDepth()
        {
            var device = GameCore.graphicsDevice;
            var blendState = device.BlendState;

            device.BlendState = BlendState.AlphaBlend;
            QuadRenderer.RenderQuad(instance.transparentTexture, new Rectangle(0, 0, GameCore.viewport.Width, GameCore.viewport.Height), instance.clearDepthShader);
            device.BlendState = blendState;
        }

        public static int SortRenderables(IGraphicsObject a, IGraphicsObject b)
        {
            return a.DrawOrder.CompareTo(b.DrawOrder);
        }
        public static int SortRenderables(IDrawable a, IDrawable b)
        {
            return a.DrawOrder.CompareTo(b.DrawOrder);
        }

        public void RenderScene(Camera camera = null)
        {
            renderer.RenderGeometry(camera);
        }

        public void Draw(GameTime gameTime)
        {
            if (GameCore.currentLevel == null || !GameCore.currentLevel.IsLoaded || Camera.Active == null)
            {
                GameCore.graphicsDevice.Clear(Color.Magenta);
                GameCore.graphicsDevice.SetRenderTarget(null);
                return;
            }
            switch (Mode)
            {
                case RenderMode.Basic:
                    {
                        var drawables = Component.g_collection.FindAll(c => c is IGraphicsObject && c.gameObject.Visible).Cast<IGraphicsObject>().ToList();
                        drawables.Sort(SortRenderables);

                        var postProcessor = Camera.Active.GetComponent<PostProcessor>();
                        if (postProcessor != null)
                            postProcessor.Begin();

                        GameCore.graphicsDevice.Clear(Camera.Active.ClearColor);

                        foreach (var drawable in drawables)
                        {
                            if (drawable.Visible)
                                drawable.Draw();
                        }

                        if (postProcessor != null)
                            postProcessor.End();
                    }
                    break;
                case RenderMode.Deferred:
                    renderer.Draw(Time.GameTime);
                    break;
                case RenderMode.DeferredDebug:
                    {
                        renderer.BeginRenderGBuffers();
                        {
                            foreach (var obj in Component.g_collection.FindAll(c => c is IGraphicsObject))
                            {
                                if (!obj.gameObject.Visible)
                                    continue;
                                renderer.RenderObject(obj as MeshRenderer);
                            }
                            var drawables = Component.g_collection.ToList().FindAll(c => c is IDrawable && c.gameObject.Visible && c.GetType() != typeof(MeshRenderer)).Cast<IDrawable>().ToList();
                            drawables.Sort(SortRenderables);
                            foreach (var drawable in drawables)
                            {
                                if (!drawable.Visible)
                                    continue;

                                drawable.Draw(gameTime);
                            }
                        }
                        renderer.EndRenderGBuffers();


                        renderer._debug_renderGBufferResults();
                    }
                    break;
            }

            GameCore.spriteBatch.Begin();
            {
                Component.g_collection.FindAll(c => c != null && c.KnowsMessage("DrawUI") && c.gameObject.Visible).ForEach(c => c.SendMessage("DrawUI", GameCore.spriteBatch));
            }
            GameCore.spriteBatch.End();

            GameCore.graphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        public static void Blit(Material material, Rectangle? rect = null)
        {
            material.shader.CurrentTechnique.Passes[0].Apply();
        }

        public void Dispose()
        {
            renderer = null;
        }

        public enum RenderMode
        {
            Basic,
            Deferred,
            DeferredDebug
        }
    }

}
