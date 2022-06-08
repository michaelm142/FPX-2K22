using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LodeObj
{
    public static class ObjLoader
    {
        public static NodeContent Import(FileStream file)
        {
            List<Vector3> positions = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<Vector3> normals = new List<Vector3>();
            List<int> faceIndicies = new List<int>();

            // read file data
            using (StreamReader reader = new StreamReader(file))
            {
                string line = reader.ReadLine();
                //System.Diagnostics.Debugger.Launch();
                while (!reader.EndOfStream || line != null)
                {
                    string[] data = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (data.Length == 0)
                    {
                        line = reader.ReadLine();
                        continue;
                    }

                    if (data[0] == "v")
                    {
                        float x = float.Parse(data[1]);
                        float y = float.Parse(data[2]);
                        float z = float.Parse(data[3]);

                        positions.Add(new Vector3(x, y, z));
                    }
                    else if (data[0] == "vt")
                    {
                        float x = float.Parse(data[1]);
                        float y = float.Parse(data[2]);

                        uvs.Add(new Vector2(x, y));
                    }
                    else if (data[0] == "vn")
                    {
                        float x = float.Parse(data[1]);
                        float y = float.Parse(data[2]);
                        float z = float.Parse(data[3]);

                        normals.Add(-new Vector3(x, y, z));
                    }
                    else if (data[0] == "f")
                    {
                        for (int i = 1; i < data.Length; i++)
                        {
                            string[] subIndicies = data[i].Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string s in subIndicies)
                                faceIndicies.Add(int.Parse(s) - 1);
                        }
                    }

                    line = reader.ReadLine();
                }
            }

            NodeContent outval = new NodeContent();

            outval.OpaqueData.Add("Positions", positions);
            outval.OpaqueData.Add("Uvs", uvs);
            outval.OpaqueData.Add("Normals", normals);
            outval.OpaqueData.Add("FaceIndicies", faceIndicies);

            return outval;
        }

        public static ModelContent Process(NodeContent input)
        {

            List<Vector3> positions = input.OpaqueData["Positions"] as List<Vector3>;
            List<Vector3> normals = input.OpaqueData["Normals"] as List<Vector3>;
            List<Vector2> uvs = input.OpaqueData["Uvs"] as List<Vector2>;
            List<int> faceIndicies = input.OpaqueData["FaceIndicies"] as List<int>;

            VertexPositionNormalTexture[] vertecies = new VertexPositionNormalTexture[faceIndicies.Count / 3];
            for (int i = 0; i < faceIndicies.Count; i += 3)
            {
                vertecies[i / 3].Position = positions[faceIndicies[i]];
                vertecies[i / 3].TextureCoordinate = uvs[faceIndicies[i + 1]];
                vertecies[i / 3].Normal = normals[faceIndicies[i + 2]];
            }

            Vector3[] binormals = new Vector3[vertecies.Length];
            for (int i = 0; i < binormals.Length; i += 3)
            {
                Vector3 edge1 = Vector3.Zero,
                    edge2 = Vector3.Zero,
                    crossProduct = Vector3.Zero,
                    binormal = Vector3.Zero,
                    n = vertecies[i].Normal;


                //$edge1[0] = $v[1][0] - $v[0][0];
                edge1.X = vertecies[i + 1].Position.X - vertecies[i].Position.X;
                //$edge1[1] = $t[1][0] - $t[0][0]; // s-vector - don't need to compute this multiple times
                edge1.Y = vertecies[i + 1].TextureCoordinate.X - vertecies[i].TextureCoordinate.X;
                //$edge1[2] = $t[1][1] - $t[0][1]; // t-vector
                edge1.Z = vertecies[i + 1].TextureCoordinate.Y - vertecies[i].TextureCoordinate.Y;

                //$edge2[0] = $v[2][0] - $v[0][0];
                edge2.X = vertecies[i + 2].Position.X - vertecies[i].Position.X;
                //$edge2[1] = $t[2][0] - $t[0][0]; // another s-vector
                edge2.Y = vertecies[i + 2].TextureCoordinate.X - vertecies[i].TextureCoordinate.X;
                //$edge2[2] = $t[2][1] - $t[0][1]; // another t-vector
                edge2.Z = vertecies[i + 2].TextureCoordinate.Y - vertecies[i].TextureCoordinate.Y;

                //$crossP = crossProduct( $edge1, $edge2 ) ;
                Vector3.Cross(ref edge1, ref edge2, out crossProduct);
                //normalize( $crossP );
                crossProduct.Normalize();
                //bool $degnerateUVTangentPlane = equivalent( $crossP[0], 0.0f );
                //if (degnerateUVTangentPlane)
                if (crossProduct.X == 0)
                    //$crossP[0] = 1.0f;
                    crossProduct.X = 1.0f;

                //float $tanX = -$crossP[1]/$crossP[0];
                binormal.X = -crossProduct.Y / crossProduct.X;

                //$edge1[0] = $v[1][1] - $v[0][1];
                edge1.X = vertecies[i + 1].Position.Y - vertecies[i].Position.Y;
                //$edge2[0] = $v[2][1] - $v[0][1];
                edge2.X = vertecies[i + 2].Position.Y - vertecies[i].Position.Y;
                //$edge2[1] = $t[2][0] - $t[0][0];
                edge2.Y = vertecies[i + 2].TextureCoordinate.X - vertecies[i].TextureCoordinate.X;
                //$edge2[2] = $t[2][1] - $t[0][1];
                edge2.Z = vertecies[i + 2].TextureCoordinate.Y - vertecies[i].TextureCoordinate.Y;
                //$crossP = crossProduct( $edge1, $edge2 );
                Vector3.Cross(ref edge1, ref edge2, out crossProduct);
                //normalize( $crossP );
                crossProduct.Normalize();
                //degnerateUVTangentPlane = equivalent( $crossP[0], 0.0f );
                //if (degnerateUVTangentPlane)
                if (crossProduct.X == 0.0f)
                    //$crossP[0] = 1.0f;
                    crossProduct.X = 1.0f;
                //float $tanY = -$crossP[1]/$crossP[0];
                binormal.Y = -crossProduct.Y / crossProduct.X;

                //$edge1[0] = $v[1][2] - $v[0][2];
                edge1.X = vertecies[i + 1].Position.Z - vertecies[i].Position.Z;
                //$edge2[0] = $v[2][2] - $v[0][2];
                edge2.X = vertecies[i + 2].Position.Z - vertecies[i].Position.Z;
                //$edge2[1] = $t[2][0] - $t[0][0];
                edge2.Y = vertecies[i + 2].TextureCoordinate.X - vertecies[i].TextureCoordinate.X;
                //$edge2[2] = $t[2][1] - $t[0][1];
                edge2.Z = vertecies[i + 2].TextureCoordinate.Y - vertecies[i].TextureCoordinate.Y;
                //$crossP = crossProduct( $edge1 , $edge2 );
                Vector3.Cross(ref edge1, ref edge2, out crossProduct);
                //normalize( $crossP );
                crossProduct.Normalize();
                //degnerateUVTangentPlane = equivalent( $crossP[0], 0.0f );
                //if (degnerateUVTangentPlane)
                if (crossProduct.X == 0.0f)
                    //$crossP[0] = 1.0f;
                    crossProduct.X = 0.0f;
                //float $tanZ = -$crossP[1]/$crossP[0];
                binormal.Z = -crossProduct.Y / crossProduct.X;

                binormals[i] = -vertecies[i].Normal * Vector3.Dot(binormal, -vertecies[i].Normal);
                binormals[i + 1] = -vertecies[i + 1].Normal * Vector3.Dot(binormal, -vertecies[i + 1].Normal);
                binormals[i + 2] = -vertecies[i + 2].Normal * Vector3.Dot(binormal, -vertecies[i + 2].Normal);
            }

            return new ModelContent(new int[] { 0 }, vertecies, binormals);
        }

        public static void LoadVertexBuffer(string filename, ref VertexPositionNormalTextureBinormal[] vBuffer)
        {
            NodeContent modelInfo = Import(new FileStream(filename, FileMode.Open));
            ModelContent model = Process(modelInfo);

            vBuffer = new VertexPositionNormalTextureBinormal[model.vertecies.Length];
            for (int i = 0; i < vBuffer.Length; i++)
            {
                vBuffer[i] = new VertexPositionNormalTextureBinormal(model.vertecies[i].Position,
                    model.vertecies[i].Normal, model.vertecies[i].TextureCoordinate, model.binormals[i]);
            }
        }
    }
}
