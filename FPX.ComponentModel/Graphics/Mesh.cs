using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LodeObj;
using FPX.ComponentModel;

namespace FPX.Visual
{
    public class Mesh : Component
    {
        public Model mesh;

        public int startIndex { get; private set; }
        public int primitiveCount { get; private set; }

        public VertexPositionNormalTextureBinormal[] Vertecies { get; private set; }

        public Triangle[] Triangles { get; private set; }

        public void LoadXml(XmlElement node)
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
                mesh = GameCore.content.Load<Model>(modelName);
                foreach (var mesh in mesh.Meshes)
                {
                    foreach (var part in mesh.MeshParts)
                    {
                        VertexPositionNormalTexture[] vertecies = new VertexPositionNormalTexture[part.VertexBuffer.VertexCount];
                        part.VertexBuffer.GetData(vertecies);

                        VertexPositionNormalTextureBinormal[] verts = new VertexPositionNormalTextureBinormal[vertecies.Length];
                        Triangles = new Triangle[vertecies.Length / 4 * 2];
                        for (int i = 0; i < vertecies.Length; i++)
                        {
                            verts[i].Position = vertecies[i].Position.ToVector4(0.0f);
                            verts[i].Normal = vertecies[i].Normal;
                            verts[i].TextureCoordinate = vertecies[i].TextureCoordinate;
                        }
                        for (int i = 0; i < vertecies.Length; i+=3)
                        {
                            Vector3 a = vertecies[i].Position;
                            Vector3 b = vertecies[i + 1].Position;
                            Vector3 c = vertecies[i + 2].Position;

                            Vector3 normal = (vertecies[i].Normal + vertecies[i + 1].Normal + vertecies[i + 2].Normal) / 3.0f;
                            normal.Normalize();

                            Triangles[i / 3] = new Triangle(this, a, b, c, normal);
                        }
                        Vertecies = verts;

                        int index = 0;
                        // Graphics.instance.renderer.AppendVertecies(verts, out index);
                        startIndex = index;
                        primitiveCount = verts.Length / 2;
                    }
                }
                mesh.Tag = modelName;
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
            filenameAttr.Value = mesh.Tag.ToString();
            modelNode.Attributes.Append(filenameAttr);
            node.AppendChild(modelNode);
        }

        public struct Triangle
        {
            private Vector3 a, b, c, normal;

            private Mesh mesh;

            public Vector3 A
            {
                get { return Vector3.Transform(a, mesh.transform.localToWorldMatrix); }

                set { a = Vector3.Transform(value, mesh.transform.worldToLocalMatrix); }
            }

            public Vector3 B
            {
                get { return Vector3.Transform(b, mesh.transform.localToWorldMatrix); }

                set { b = Vector3.Transform(value, mesh.transform.worldToLocalMatrix); }
            }

            public Vector3 C
            {
                get { return Vector3.Transform(c, mesh.transform.localToWorldMatrix); }

                set { c = Vector3.Transform(value, mesh.transform.worldToLocalMatrix); }
            }

            public Vector3 Normal
            {
                get { return Vector3.TransformNormal(normal, mesh.transform.localToWorldMatrix); }

                set { normal = Vector3.TransformNormal(normal, mesh.transform.worldToLocalMatrix); }
            }

            public Triangle(Mesh m, Vector3 a, Vector3 b, Vector3 c, Vector3 normal)
            {
                this.mesh = m;
                this.a = a;
                this.b = b;
                this.c = c;
                this.normal = normal;
            }
        }
    }
}
