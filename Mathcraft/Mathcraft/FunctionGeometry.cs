using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Mathcraft
{    
    public class GeometryParameters
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
        public readonly Func<Point3D, bool> Visible;

        /// <summary>
        /// Function that is called for each voxel and returns its MaterialID
        /// </summary> 
        public readonly Func<Point3D, int> MaterialID;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="size"></param>
        /// <param name="visibleFunc"></param>
        /// <param name="colorFunc"></param>
        public GeometryParameters(Point3D pos, Point3D size, Func<Point3D, bool> visibleFunc, 
            Func<Point3D, int> materialFunc)
        {
            Position = pos;
            Size = size;
            Visible = visibleFunc;
            MaterialID = materialFunc;
        }
    }

    /// <summary>
    /// Geometry defined by a mathematical function or equations
    /// </summary>
    public class MathGeometry
    {
        GraphicsDevice graphics;
        GeometryParameters parameters;
        static IndexBuffer indexBuffer;
        static VertexBuffer geometryBuffer;
        VertexBuffer instanceBuffer;
        Effect effect;
        Game game;
        Texture2D materials;
        int numInstances;

        static bool init = false;

        /// <summary>
        /// Creates a 10x10 large simple cube and stores it in the geometry buffer.
        /// </summary>
        /// <param name="gd">Graphics Device</param>
        static void Init(GraphicsDevice gd)
        {
            init = true;
            Vector3 shapePosition = Vector3.Zero;

            Vector3 shapeSize = new Vector3(5, 5, 5);

            VertexPositionNormalTexture[] shapeVertices = new VertexPositionNormalTexture[36];

            Vector3 topLeftFront = shapePosition +
                new Vector3(-1.0f, 1.0f, -1.0f) * shapeSize;
            Vector3 bottomLeftFront = shapePosition +
                new Vector3(-1.0f, -1.0f, -1.0f) * shapeSize;
            Vector3 topRightFront = shapePosition +
                new Vector3(1.0f, 1.0f, -1.0f) * shapeSize;
            Vector3 bottomRightFront = shapePosition +
                new Vector3(1.0f, -1.0f, -1.0f) * shapeSize;
            Vector3 topLeftBack = shapePosition +
                new Vector3(-1.0f, 1.0f, 1.0f) * shapeSize;
            Vector3 topRightBack = shapePosition +
                new Vector3(1.0f, 1.0f, 1.0f) * shapeSize;
            Vector3 bottomLeftBack = shapePosition +
                new Vector3(-1.0f, -1.0f, 1.0f) * shapeSize;
            Vector3 bottomRightBack = shapePosition +
                new Vector3(1.0f, -1.0f, 1.0f) * shapeSize;

            Vector3 frontNormal = new Vector3(0.0f, 0.0f, 1.0f) * shapeSize;
            Vector3 backNormal = new Vector3(0.0f, 0.0f, -1.0f) * shapeSize;
            Vector3 topNormal = new Vector3(0.0f, 1.0f, 0.0f) * shapeSize;
            Vector3 bottomNormal = new Vector3(0.0f, -1.0f, 0.0f) * shapeSize;
            Vector3 leftNormal = new Vector3(-1.0f, 0.0f, 0.0f) * shapeSize;
            Vector3 rightNormal = new Vector3(1.0f, 0.0f, 0.0f) * shapeSize;

            shapeSize = Vector3.One;
            Vector2 textureTopLeft = new Vector2(0.5f * shapeSize.X, 0.0f * shapeSize.Y);
            Vector2 textureTopRight = new Vector2(0.0f * shapeSize.X, 0.0f * shapeSize.Y);
            Vector2 textureBottomLeft = new Vector2(0.5f * shapeSize.X, 0.5f * shapeSize.Y);
            Vector2 textureBottomRight = new Vector2(0.0f * shapeSize.X, 0.5f * shapeSize.Y);

            // Front face.
            shapeVertices[0] = new VertexPositionNormalTexture(
                topLeftFront, frontNormal, textureTopLeft);
            shapeVertices[1] = new VertexPositionNormalTexture(
                bottomLeftFront, frontNormal, textureBottomLeft);
            shapeVertices[2] = new VertexPositionNormalTexture(
                topRightFront, frontNormal, textureTopRight);
            shapeVertices[3] = new VertexPositionNormalTexture(
                bottomLeftFront, frontNormal, textureBottomLeft);
            shapeVertices[4] = new VertexPositionNormalTexture(
                bottomRightFront, frontNormal, textureBottomRight);
            shapeVertices[5] = new VertexPositionNormalTexture(
                topRightFront, frontNormal, textureTopRight);

            // Back face.
            shapeVertices[6] = new VertexPositionNormalTexture(
                topLeftBack, backNormal, textureTopRight);
            shapeVertices[7] = new VertexPositionNormalTexture(
                topRightBack, backNormal, textureTopLeft);
            shapeVertices[8] = new VertexPositionNormalTexture(
                bottomLeftBack, backNormal, textureBottomRight);
            shapeVertices[9] = new VertexPositionNormalTexture(
                bottomLeftBack, backNormal, textureBottomRight);
            shapeVertices[10] = new VertexPositionNormalTexture(
                topRightBack, backNormal, textureTopLeft);
            shapeVertices[11] = new VertexPositionNormalTexture(
                bottomRightBack, backNormal, textureBottomLeft);

            // Top face.
            shapeVertices[12] = new VertexPositionNormalTexture(
                topLeftFront, topNormal, textureBottomLeft);
            shapeVertices[13] = new VertexPositionNormalTexture(
                topRightBack, topNormal, textureTopRight);
            shapeVertices[14] = new VertexPositionNormalTexture(
                topLeftBack, topNormal, textureTopLeft);
            shapeVertices[15] = new VertexPositionNormalTexture(
                topLeftFront, topNormal, textureBottomLeft);
            shapeVertices[16] = new VertexPositionNormalTexture(
                topRightFront, topNormal, textureBottomRight);
            shapeVertices[17] = new VertexPositionNormalTexture(
                topRightBack, topNormal, textureTopRight);

            // Bottom face.
            shapeVertices[18] = new VertexPositionNormalTexture(
                bottomLeftFront, bottomNormal, textureTopLeft);
            shapeVertices[19] = new VertexPositionNormalTexture(
                bottomLeftBack, bottomNormal, textureBottomLeft);
            shapeVertices[20] = new VertexPositionNormalTexture(
                bottomRightBack, bottomNormal, textureBottomRight);
            shapeVertices[21] = new VertexPositionNormalTexture(
                bottomLeftFront, bottomNormal, textureTopLeft);
            shapeVertices[22] = new VertexPositionNormalTexture(
                bottomRightBack, bottomNormal, textureBottomRight);
            shapeVertices[23] = new VertexPositionNormalTexture(
                bottomRightFront, bottomNormal, textureTopRight);

            // Left face.
            shapeVertices[24] = new VertexPositionNormalTexture(
                topLeftFront, leftNormal, textureTopRight);
            shapeVertices[25] = new VertexPositionNormalTexture(
                bottomLeftBack, leftNormal, textureBottomLeft);
            shapeVertices[26] = new VertexPositionNormalTexture(
                bottomLeftFront, leftNormal, textureBottomRight);
            shapeVertices[27] = new VertexPositionNormalTexture(
                topLeftBack, leftNormal, textureTopLeft);
            shapeVertices[28] = new VertexPositionNormalTexture(
                bottomLeftBack, leftNormal, textureBottomLeft);
            shapeVertices[29] = new VertexPositionNormalTexture(
                topLeftFront, leftNormal, textureTopRight);

            // Right face.
            shapeVertices[30] = new VertexPositionNormalTexture(
                topRightFront, rightNormal, textureTopLeft);
            shapeVertices[31] = new VertexPositionNormalTexture(
                bottomRightFront, rightNormal, textureBottomLeft);
            shapeVertices[32] = new VertexPositionNormalTexture(
                bottomRightBack, rightNormal, textureBottomRight);
            shapeVertices[33] = new VertexPositionNormalTexture(
                topRightBack, rightNormal, textureTopRight);
            shapeVertices[34] = new VertexPositionNormalTexture(
                topRightFront, rightNormal, textureTopLeft);
            shapeVertices[35] = new VertexPositionNormalTexture(
                bottomRightBack, rightNormal, textureBottomRight);

            geometryBuffer = new VertexBuffer(gd, typeof(VertexPositionNormalTexture), 36, BufferUsage.WriteOnly);
            geometryBuffer.SetData(shapeVertices);

            indexBuffer = new IndexBuffer(gd, IndexElementSize.SixteenBits, 36, BufferUsage.WriteOnly);
            short[] indices = (from nr in Enumerable.Range(0, 35) select (short)nr).ToArray();
            indexBuffer.SetData(indices);
        }

        public MathGeometry(Game game, GeometryParameters parameters)
        {
            graphics = game.GraphicsDevice;
            this.game = game;
            this.parameters = parameters;
            effect = game.Content.Load<Effect>("VoxelEffect");
            materials = game.Content.Load<Texture2D>("materials");
            if (!init) Init(game.GraphicsDevice);

            List<VoxelVertex> voxels = new List<VoxelVertex>();

            for (int x = 0; x < parameters.Size.X; x++)
            {
                for (int y = 0; y < parameters.Size.Y; y++)
                {
                    for (int z = 0; z < parameters.Size.Y; z++)
                    {
                        Point3D p = new Point3D(x, y, z);
                        if (!parameters.Visible(p)) continue;

                        int material = parameters.MaterialID(p);

                        VoxelVertex v = new VoxelVertex()
                        {
                            Position = p.ToVector3(),
                            MaterialID = material
                        };

                        voxels.Add(v);
                    }
                }
            }

            instanceBuffer = new VertexBuffer(graphics, typeof(VoxelVertex), voxels.Count, BufferUsage.WriteOnly);
            instanceBuffer.SetData(voxels.ToArray());
            numInstances = voxels.Count;
        }

        public void Draw()
        {
            Camera camera = game.Services.GetService(typeof(Camera)) as Camera;

            graphics.BlendState = BlendState.Opaque;
            graphics.DepthStencilState = DepthStencilState.Default;
            graphics.RasterizerState = RasterizerState.CullNone;

            effect.CurrentTechnique = effect.Techniques["HardwareInstancing"];

            effect.Parameters["World"].SetValue(Matrix.CreateTranslation(parameters.Position.ToVector3()));
            effect.Parameters["View"].SetValue(camera.View);
            effect.Parameters["Projection"].SetValue(camera.Projection);
            effect.Parameters["Materials"].SetValue(materials);

            graphics.Indices = indexBuffer;
            graphics.SetVertexBuffers(new VertexBufferBinding(geometryBuffer, 0, 0),
                new VertexBufferBinding(instanceBuffer, 0, 1));

            // Draw all the instance copies in a single call.
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();                

                graphics.DrawInstancedPrimitives(PrimitiveType.TriangleList, 
                    0, 0, 36, 0, 12, numInstances);
            }
        }
    }    

    struct VoxelVertex : IVertexType
    {
        public Vector3 Position;
        public float MaterialID;        

        private static VertexDeclaration _vertexDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 1),
            new VertexElement(12, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 1)
            );

        public VertexDeclaration VertexDeclaration
        {
            get { return _vertexDeclaration; }
        }
    }
}
