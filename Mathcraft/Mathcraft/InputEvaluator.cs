using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting;
using Microsoft.Xna.Framework;

namespace Mathcraft
{
    public class Wrapper
    {
        public Func<Point3D, bool> visible_func;
        public Func<Point3D, int> material_func;
    }

    public class InputEvaluator
    {
        public static Wrapper wrapper = new Wrapper();

        ScriptEngine engine;
        Game game;

        public InputEvaluator(Game game)
        {
            this.game = game;
            engine = Python.CreateEngine();            
        }

        public EvalResult Evaluate(String input)
        {
            String prelude =
                "import clr\n" +
                "clr.AddReference('Mathcraft')\n" +
                "from Mathcraft import *\n" +
                "clr.AddReference('System.Core')\n" +
                "from System import Func\n";

            input = prelude + input;

            String suffix = "\n" +
                "wrapper.visible_func = Func[Point3D,bool](visible)\n" +
                "wrapper.material_func = Func[Point3D,int](material)\n";

            if (input.Contains("def visible") && input.Contains("def material"))
                input += suffix;

            EvalResult result = new EvalResult();

            Point3D size = new Point3D(20, 20, 20);
            Point3D pos = new Point3D(-50, -50, -200);            
            Func<Point3D, bool> visFunc = null;
            Func<Point3D, int> matFunc = null;

            var scope = engine.CreateScope();
            scope.SetVariable("wrapper", wrapper);
            scope.SetVariable("pos", pos);            
            scope.SetVariable("size", size);
            scope.SetVariable("visibility", visFunc);
            scope.SetVariable("material", matFunc);            

            ScriptSource source = engine.CreateScriptSourceFromString(input, SourceCodeKind.Statements);
            CompiledCode compiled = null;

            wrapper.material_func = null;
            wrapper.visible_func = null;

            try
            {
                compiled = source.Compile();
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                return result;
            }

            // Executes in the scope of Python
            compiled.Execute(scope);

            bool gotPos = scope.TryGetVariable<Point3D>("pos", out pos);
            bool gotSize = scope.TryGetVariable<Point3D>("size", out size);

            if (wrapper.visible_func != null && wrapper.material_func != null)
            {
                var param = new GeometryParameters(pos, size,
                    wrapper.visible_func, wrapper.material_func);

                result.GeometryParameters = param;

                var gm = game.Services.GetService(typeof(GeometryManager)) as GeometryManager;
                gm.Clear();

                try
                {
                    gm.AddGeometry(param);
                }
                catch (Exception ex)
                {
                    result.Message = ex.Message;
                    result.Success = false;
                }
            }

            result.Message = "OK";
            result.Success = true;

            return result;
        }
    }

    public struct EvalResult
    {
        public bool Success;
        public string Message;
        public GeometryParameters GeometryParameters;
    }
}
