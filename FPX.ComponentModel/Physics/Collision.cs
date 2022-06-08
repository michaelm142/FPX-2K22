using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace FPX
{
    public class Collision
    {
        public IEnumerable<Collider> colliders
        {
            get
            {
                for (int i = 0; i < 2; i++)
                {
                    if (i == 0)
                        yield return a;
                    else
                        yield return b;
                }
            }
        }

        Collider a;
        Collider b;

        public Vector3 L;
        public Vector3 ContactNormal;

        public float Psudodistance;
        public float PenetrationDistance;

        public Collision(Collider a, Collider b)
        {
            this.a = a;
            this.b = b;

        }

        public Collider this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return a;
                    case 1:
                        return b;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }
    }
}
