using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Mathcraft
{
    public class GeometryManager : DrawableGameComponent
    {
        List<MathGeometry> geometries = new List<MathGeometry>();

        public GeometryManager(Game game)
            : base(game)
        {
            DrawOrder = 1;
        }

        public override void Initialize()
        {
            Game.Services.AddService(typeof(GeometryManager), this);
            
            base.Initialize();
        }

        public void Clear()
        {
            geometries.Clear();
        }

        public void AddGeometry(GeometryParameters parameters)
        {
            geometries.Add(new MathGeometry(Game, parameters));
        }

        public override void Draw(GameTime gameTime)
        {
            foreach (var geom in geometries)
                geom.Draw();
            
            base.Draw(gameTime);
        }
    }
}
