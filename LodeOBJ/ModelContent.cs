using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LodeObj
{
    public class ModelContent
    {
        public VertexPositionNormalTexture[] vertecies;
        public Vector3[] binormals;

        public int[] indicies;

        public ModelContent(int[] indicies, VertexPositionNormalTexture[] vertecies, Vector3[] binormals)
        {
            this.indicies = indicies;
            this.vertecies = vertecies;
            this.binormals = binormals;
            Array.Reverse(this.vertecies);
        }
    }
}
