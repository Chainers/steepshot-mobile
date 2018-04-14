using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Steepshot.Core.Sentry.Models
{
    public class ExceptionFrame
    {
        [JsonProperty(PropertyName = "abs_path")]
        public string AbsolutePath { get; set; }

        [JsonProperty(PropertyName = "colno")]
        public int ColumnNumber { get; set; }

        [JsonProperty(PropertyName = "filename")]
        public string Filename { get; set; }

        [JsonProperty(PropertyName = "function")]
        public string Function { get; private set; }

        [JsonProperty(PropertyName = "in_app")]
        public bool InApp { get; set; }

        [JsonProperty(PropertyName = "lineno")]
        public int LineNumber { get; set; }

        [JsonProperty(PropertyName = "module")]
        public string Module { get; private set; }

        [JsonProperty(PropertyName = "post_context")]
        public List<string> PostContext { get; set; }

        [JsonProperty(PropertyName = "pre_context")]
        public List<string> PreContext { get; set; }

        [JsonProperty(PropertyName = "context_line")]
        public string Source { get; set; }

        [JsonProperty(PropertyName = "vars")]
        public Dictionary<string, string> Vars { get; set; }

        public ExceptionFrame(StackFrame frame)
        {
            if (frame == null)
                return;

            int lineNo = frame.GetFileLineNumber();

            if (lineNo == 0)
            {
                //The pdb files aren't currently available
                lineNo = frame.GetILOffset();
            }

            var method = frame.GetMethod();
            if (method != null)
            {
                Module = (method.DeclaringType != null) ? method.DeclaringType.FullName : null;
                Function = method.Name;
                Source = method.ToString();
            }
            else
            {
                // on some platforms (e.g. on mono), StackFrame.GetMethod() may return null
                // e.g. for this stack frame:
                //   at (wrapper dynamic-method) System.Object:lambda_method (System.Runtime.CompilerServices.Closure,object,object))

                Module = "(unknown)";
                Function = "(unknown)";
                Source = "(unknown)";
            }

            Filename = frame.GetFileName();
            LineNumber = lineNo;
            ColumnNumber = frame.GetFileColumnNumber();
            InApp = !IsSystemModuleName(Module);
            DemangleAsyncFunctionName();
            DemangleAnonymousFunction();
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (Module != null)
            {
                sb.Append(Module);
                sb.Append('.');
            }

            if (Function != null)
            {
                sb.Append(Function);
                sb.Append("()");
            }

            if (Filename != null)
            {
                sb.Append(" in ");
                sb.Append(Filename);
            }

            if (LineNumber > -1)
            {
                sb.Append(":line ");
                sb.Append(LineNumber);
            }

            return sb.ToString();
        }

        private static bool IsSystemModuleName(string moduleName)
        {
            return !string.IsNullOrEmpty(moduleName) &&
                (moduleName.StartsWith("System.", System.StringComparison.Ordinal) ||
                 moduleName.StartsWith("Microsoft.", System.StringComparison.Ordinal));
        }

        /// <summary>
        /// Clean up function and module names produced from `async` state machine calls.
        /// </summary>
        /// <para>
        /// When the Microsoft cs.exe compiler compiles some modern C# features,
        /// such as async/await calls, it can create synthetic function names that
        /// do not match the function names in the original source code. Here we
        /// reverse some of these transformations, so that the function and module
        /// names that appears in the Sentry UI will match the function and module
        /// names in the original source-code.
        /// </para>
        private void DemangleAsyncFunctionName()
        {
            if (Module == null || Function != "MoveNext")
            {
                return;
            }

            //  Search for the function name in angle brackets followed by d__<digits>.
            //
            // Change:
            //   RemotePrinterService+<UpdateNotification>d__24 in MoveNext at line 457:13
            // to:
            //   RemotePrinterService in UpdateNotification at line 457:13

            var mangled = @"^(.*)\+<(\w*)>d__\d*$";
            var match = Regex.Match(Module, mangled);
            if (match.Success && match.Groups.Count == 3)
            {
                Module = match.Groups[1].Value;
                Function = match.Groups[2].Value;
            }
        }

        /// <summary>
        /// Clean up function names for anonymous lambda calls.
        /// </summary>
        private void DemangleAnonymousFunction()
        {
            if (Function == null)
            {
                return;
            }

            // Search for the function name in angle brackets followed by b__<digits/letters>.
            //
            // Change:
            //   <BeginInvokeAsynchronousActionMethod>b__36
            // to:
            //   BeginInvokeAsynchronousActionMethod { <lambda> }

            var mangled = @"^<(\w*)>b__\w+$";
            var match = Regex.Match(Function, mangled);
            if (match.Success && match.Groups.Count == 2)
            {
                Function = match.Groups[1].Value + " { <lambda> }";
            }
        }
    }
}
