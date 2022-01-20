//using Microsoft.CodeAnalysis.CSharp.Scripting;
//using Microsoft.CodeAnalysis.Scripting;
//using System;
//using System.Diagnostics;
//using System.Threading.Tasks;

namespace WebCore
{
    /// <summary>
    /// Eval Script
    /// </summary>
    public class Script
    {
        ///// <summary>
        ///// Eval CSharp https://github.com/dotnet/roslyn
        ///// </summary>
        ///// <param name="code"></param>
        ///// <param name="globals"></param>
        ///// <param name="globalsType"></param>
        ///// <param name="imports"></param>
        ///// <returns></returns>
        //public static async Task<object> EvalCSharp(string code, object globals = null, Type globalsType = null, params string[] imports)
        //{
        //    object result = null;
        //    try
        //    {
        //        result = await CSharpScript.EvaluateAsync(code, ScriptOptions.Default.WithImports(imports), globals, globalsType);
        //    }
        //    catch (CompilationErrorException e)
        //    {
        //        Debug.WriteLine(string.Join(Environment.NewLine, e.Diagnostics));
        //    }
        //    return result;
        //}
    }
}
