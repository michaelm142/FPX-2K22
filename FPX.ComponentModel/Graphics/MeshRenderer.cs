using System;
using System.Linq;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LodeObj;
using FPX.ComponentModel;
using FPX.Editor;

namespace FPX.Visual
{
    [Editor(typeof(MeshRendererEditor))]
    public class MeshRenderer : Component, IDrawable
    {
        public Model model;

        public Material material
        {
            get { return GetComponent<Material>(); }
        }

        public int startIndex { get; private set; }
        public int primitiveCount { get; private set; }
        public int indexCount { get; private set; }

        public VertexPositionNormalTextureBinormal[] Vertecies { get; private set; }

        public int[] Indicies { get; private set; }

        public bool Visible { get; set; } = true;

        public int DrawOrder { get; set; }

        public event EventHandler<EventArgs> VisibleChanged;
        public event EventHandler<EventArgs> DrawOrderChanged;

        public void Draw(GameTime gametime)
        {
            foreach (var mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    Light ambient = (g_collection.Find(c => c is Light && (c as Light).LightType == LightType.Ambient) as Light);
                    effect.AmbientLightColor = (ambient != null ? ambient.DiffuseColor : Light.DefaultColor).ToVector3();
                    effect.DiffuseColor = material.DiffuseColor.ToVector3();
                    effect.SpecularColor = material.SpecularColor.ToVector3();
                    effect.PreferPerPixelLighting = true;
                    if (material.DiffuseMap != null)
                    {
                        effect.TextureEnabled = true;
                        effect.Texture = material.DiffuseMap;
                    }

                    effect.DirectionalLight0.DiffuseColor = (Color.White * 0.1F).ToVector3();
                    effect.DirectionalLight0.Direction = (Vector3.Down + Vector3.Left + Vector3.Forward);
                    effect.DirectionalLight0.Direction.Normalize();
                    effect.DirectionalLight0.Enabled = true;
                    effect.LightingEnabled = true;

                    effect.FogEnabled = true;
                    effect.FogStart = 1.5F;
                    effect.FogEnd = 100.0F;
                    effect.FogColor = Color.CornflowerBlue.ToVector3();

                    effect.World = GetComponent<Transform>().worldPose;
                    effect.View = Camera.Active.ViewMatrix;
                    effect.Projection = Camera.Active.ProjectionMatrix;
                    effect.CurrentTechnique.Passes[0].Apply();

                    foreach (var meshPart in mesh.MeshParts)
                    {
                        GameCore.graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer, meshPart.VertexOffset);
                        GameCore.graphicsDevice.Indices = meshPart.IndexBuffer;
                        GameCore.graphicsDevice.DrawIndexedPrimitives(Graphics.instance.fillMode, 0, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);
                    }
                }
            }
        }

        public override void LoadXml(XmlElement node)
        {
            VertexDeclaration[] decl = new VertexDeclaration[]
            {
                VertexPositionColor.VertexDeclaration,
                VertexPositionColorTexture.VertexDeclaration,
                VertexPositionNormalTexture.VertexDeclaration,
                VertexPositionNormalTextureBinormal.vertexDeclaration,
                VertexPositionTexture.VertexDeclaration,
            };

            string modelName = node.SelectSingleNode("Model").Attributes["Name"].Value;
            try
            {
                model = GameCore.content.Load<Model>(modelName);
                foreach (var mesh in model.Meshes)
                {
                    foreach (var part in mesh.MeshParts)
                    {
                        VertexPositionNormalTexture[] vertecies = new VertexPositionNormalTexture[part.VertexBuffer.VertexCount];
                        UInt16[] indicies = new UInt16[part.IndexBuffer.IndexCount];
                        part.VertexBuffer.GetData(vertecies);
                        part.IndexBuffer.GetData<UInt16>(indicies);

                        VertexPositionNormalTextureBinormal[] verts = new VertexPositionNormalTextureBinormal[vertecies.Length];
                        for (int i = 0; i < vertecies.Length; i++)
                        {
                            verts[i].Position = vertecies[i].Position.ToVector4(1.0f);
                            verts[i].Normal = vertecies[i].Normal;
                            verts[i].TextureCoordinate = vertecies[i].TextureCoordinate;
                        }
                        Vertecies = verts;
                        Indicies = new int[indicies.Length];
                        for (int i = 0; i < Indicies.Length; i++)
                            Indicies[i] = (int)indicies[i];
                        for (int i = 0; i < indicies.Length; i += 3)
                        {
                            ushort index1 = indicies[i];
                            ushort index2 = indicies[i + 1];
                            ushort index3 = indicies[i + 2];

                            var vert1 = Vertecies[index1];
                            var vert2 = Vertecies[index2];
                            var vert3 = Vertecies[index3];

                            Vector3 binormal = (vert3.Position - vert1.Position).ToVector3().Normalized();
                            vert1.Binormal = vert2.Binormal = vert3.Binormal = binormal;
                        }

                        startIndex = 0;
                        primitiveCount = indicies.Length / 3;
                        indexCount = indicies.Length;
                    }
                }
                model.Tag = modelName;
            }
            catch (ContentLoadException)
            {
                Debug.LogError("Models {0} could not be found in content", modelName);
                return;
            }
        }

        public void SaveXml(XmlElement node)
        {
            var modelNode = node.OwnerDocument.CreateElement("Model");
            var filenameAttr = node.OwnerDocument.CreateAttribute("Name");
            filenameAttr.Value = model.Tag.ToString();
            modelNode.Attributes.Append(filenameAttr);
            node.AppendChild(modelNode);
        }
    }

}
