using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LodeObj;
using FPX.ComponentModel;

namespace FPX.Visual
{
    public class Terrain : Component, IDrawable
    {
        public int DrawOrder => 0;

        private VertexBuffer VertexBuffer;
        private IndexBuffer indexBuffer;

        public bool Visible => gameObject.Visible;

        public event EventHandler<EventArgs> DrawOrderChanged;
        public event EventHandler<EventArgs> VisibleChanged;

        private Texture2D normalMap;
        private Texture2D heightMap;

        private Model model;

        private Effect terrainShader;

        public void Start()
        {
            model = GameCore.content.Load<Model>("Models//Terrain");
            VertexPositionNormalTexture[] modelVerts = new VertexPositionNormalTexture[0];
            short[] indicies = new short[0];
            int vertexCount = 0;
            foreach (var mesh in model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    modelVerts = new VertexPositionNormalTexture[meshPart.VertexBuffer.VertexCount];
                    indicies = new short[meshPart.IndexBuffer.IndexCount];
                    vertexCount = modelVerts.Length;
                    meshPart.VertexBuffer.GetData(modelVerts);
                    meshPart.IndexBuffer.GetData(indicies);
                }
            }
            indexBuffer = new IndexBuffer(GameCore.graphicsDevice, IndexElementSize.SixteenBits, indicies.Length, BufferUsage.None);
            indexBuffer.SetData(indicies);

            VertexPositionNormalTextureBinormal[] vertecies = new VertexPositionNormalTextureBinormal[vertexCount];
            for (int i = 0; i < vertecies.Length; i++)
            {
                vertecies[i].Position = modelVerts[i].Position.ToVector4(1.0f);
                vertecies[i].TextureCoordinate = modelVerts[i].TextureCoordinate;
                vertecies[i].Normal = modelVerts[i].Normal;
            }
            VertexBuffer = new VertexBuffer(GameCore.graphicsDevice, typeof(VertexPositionNormalTextureBinormal), vertecies.Length, BufferUsage.None);
            VertexBuffer.SetData(vertecies);


            normalMap = GameCore.content.Load<Texture2D>("Textures//DefaultNormalMap");
            heightMap = GameCore.content.Load<Texture2D>("Textures//HeightMap");
            terrainShader = GameCore.content.Load<Effect>("Shaders//TerrainGBuffers");
        }

        public void Draw(GameTime gameTime)
        {
            foreach (var mesh in model.Meshes)
            {
                terrainShader.Parameters["World"].SetValue(transform.worldPose);
                terrainShader.Parameters["ViewProjection"].SetValue(Camera.Active.ViewMatrix * Camera.Active.ProjectionMatrix);
                terrainShader.Parameters["DiffuseMap"].SetValue(Material.DefaultTexture);
                terrainShader.Parameters["NormalMap"].SetValue(Material.DefaultTexture);
                terrainShader.Parameters["iResolution"].SetValue(new Vector2(heightMap.Width, heightMap.Height));
                terrainShader.Parameters["HeightMap"].SetValue(heightMap);
                terrainShader.Parameters["TerrainHeight"].SetValue(100.0f);
                terrainShader.CurrentTechnique.Passes[0].Apply();

                GameCore.graphicsDevice.BlendState = BlendState.Opaque;

                foreach (var meshPart in mesh.MeshParts)
                {
                    GameCore.graphicsDevice.SetVertexBuffer(VertexBuffer);
                    GameCore.graphicsDevice.Indices = indexBuffer;
                    GameCore.graphicsDevice.DrawIndexedPrimitives(Graphics.instance.fillMode, 0, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);
                }
            }
        }
    }
}
