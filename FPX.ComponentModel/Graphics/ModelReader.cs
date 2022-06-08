using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LodeObj;

namespace FPX
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content
    /// Pipeline to read the specified data type from binary .xnb format.
    /// 
    /// Unlike the other Content Pipeline support classes, this should
    /// be a part of your main game project, and not the Content Pipeline
    /// Extension Library project.
    /// </summary>
    public class ModelReader : ContentTypeReader<ModelContent>
    {
        protected override ModelContent Read(ContentReader input, ModelContent existingInstance)
        {
            int indexCount = input.ReadInt32();

            int[] indicies = new int[indexCount];
            for (int i = 0; i < indexCount; i++)
                indicies[i] = input.ReadInt32();

            int vertexCount = input.ReadInt32();

            VertexPositionNormalTexture[] vertecies = new VertexPositionNormalTexture[vertexCount];
            Vector3[] binormals = new Vector3[vertexCount];
            for (int i = 0; i < vertexCount; i++)
            {
                Vector3 position = input.ReadVector3();
                Vector3 normal = input.ReadVector3();
                Vector2 uv = input.ReadVector2();
                Vector3 binormal = input.ReadVector3();

                vertecies[i] = new VertexPositionNormalTexture(position, normal, uv);
                binormals[i] = binormal;
            }

            return new ModelContent(indicies, vertecies, binormals);
        }
    }
}
