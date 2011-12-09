using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Mathcraft
{    
    class GeometryParameters
    {
        /// <summary>
        /// Center position of the geometry
        /// </summary>
        public readonly Point3D Position;

        /// <summary>
        /// Size (WxHxD) of the geometry. Together with position
        /// this composes the hull of the geometry.
        /// </summary>
        public readonly Point3D Size;

        /// <summary>
        /// Function that is called for each voxel and determines 
        /// its visibility. True = voxel is rendered.
        /// </summary>
        public readonly Func<bool, Point3D> Visible;

        /// <summary>
        /// Function that is called for each voxel and returns its color
        /// </summary> 
        public readonly Func<Color, Point3D> Color;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="size"></param>
        /// <param name="visibleFunc"></param>
        /// <param name="colorFunc"></param>
        public GeometryParameters(Point3D pos, Point3D size, Func<bool, Point3D> visibleFunc, 
            Func<Color, Point3D> colorFunc)
        {
            Position = pos;
            Size = size;
            Visible = visibleFunc;
            Color = colorFunc;
        }
    }

    /// <summary>
    /// Geometry defined by a mathematical function or equations
    /// </summary>
    class MathGeometry
    {
        GeometryParameters parameters;

        public MathGeometry(GeometryParameters parameters)
        {
            this.parameters = parameters;
        }
    }    

    struct VoxelVertex : IVertexType
    {
        public Vector3 Position;
        public Color Color;

        public VertexDeclaration VertexDeclaration
        {
            get { throw new NotImplementedException(); }
        }
    }
}
