using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Mathcraft
{    
    class GeometryParameters
    {
        public Point3D Position;

        // determines whether a point should be visible
        public Func<bool, Point3D> Visible;

        // determines the color of the point
        public Func<Color, Point3D> Color;
    }

    /// <summary>
    /// Geometry defined by a mathematical function or equations
    /// </summary>
    class MathGeometry
    {

    }    
}
