using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Mathcraft
{
    public class Point3D
    {
        public int X, Y, Z;

        public int x
        {
            get { return X; }
            set { X = value; }
        }

        public int y
        {
            get { return Y; }
            set { Y = value; }
        }

        public int z
        {
            get { return Z; }
            set { Z = value; }
        }

        public Point3D(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;            
        }

        public Point3D(int[] v)
        {
            if (v == null) return;
            if (v.Length < 3) return;

            X = v[0];
            Y = v[1];
            Z = v[2];
        }

        public Vector3 ToVector3()
        {
            return new Vector3(X, Y, Z);
        }

        public override string ToString()
        {
            return String.Format("Point3D[{0},{1},{2}]", X, Y, Z);
        }
    }
}
