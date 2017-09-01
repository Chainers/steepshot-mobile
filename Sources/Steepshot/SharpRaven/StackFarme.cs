using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace SharpRavenPortable
{
 
    public class StackFrame
    {
       
        public const int OFFSET_UNKNOWN = -1;
        private Exception exception;
        private bool v;

        public StackFrame()
        {
        }

        public StackFrame(Exception exception, bool v)
        {
            this.exception = exception;
            this.v = v;
        }

        //
        // Summary:
        //     Initializes a new instance of the System.Diagnostics.StackFrame class, optionally
        //     capturing source information.
        //
        // Parameters:
        //   fNeedFileInfo:
        //     true to capture the file name, line number, and column number of the stack frame;
        //     otherwise, false.
        public virtual int GetFileColumnNumber()
        {
            return -1;
        }

        //
        // Summary:
        //     Gets the line number in the file that contains the code that is executing. This
        //     information is typically extracted from the debugging symbols for the executable.
        //
        // Returns:
        //     The file line number, or 0 (zero) if the file line number cannot be determined.
        public virtual int GetFileLineNumber()
        {
            int res = -1;
            var trace = this.exception.StackTrace?.Split(':');
            Int32.TryParse(trace?[trace.Length - 1].Trim(), out res);
            return res;
        }

        [SecuritySafeCritical]
        public virtual string GetFileName()
        {
            var trace = this.exception?.StackTrace?.Split(':');
            var file = trace?[trace.Length - 2].Split('\\');
            string res = file?[file.Length - 1];
            return res;
        }

        //
        // Summary:
        //     Gets the offset from the start of the Microsoft intermediate language (MSIL)
        //     code for the method that is executing. This offset might be an approximation
        //     depending on whether or not the just-in-time (JIT) compiler is generating debugging
        //     code. The generation of this debugging information is controlled by the System.Diagnostics.DebuggableAttribute.
        //
        // Returns:
        //     The offset from the start of the MSIL code for the method that is executing.
        public virtual int GetILOffset()
        {
            return -1;
        }

        internal StackFrame[] GetFrames()
        {
            return new StackFrame[] {new StackFrame(exception,v), };
        }

        //
        // Summary:
        //     Gets the method in which the frame is executing.
        //
        // Returns:
        //     The method in which the frame is executing.
        public virtual MethodBase GetMethod()
        {
            var propInfo = exception?.GetType()?.GetRuntimeProperty("TargetSite");
            MethodBase methodBase = (MethodBase)propInfo?.GetValue(exception, null);
            return methodBase;
        }

        //
        // Summary:
        //     Gets the offset from the start of the native just-in-time (JIT)-compiled code
        //     for the method that is being executed. The generation of this debugging information
        //     is controlled by the System.Diagnostics.DebuggableAttribute class.
        //
        // Returns:
        //     The offset from the start of the JIT-compiled code for the method that is being
        //     executed.
        public virtual int GetNativeOffset()
        {
            return -1;
        }

        //
        // Summary:
        //     Builds a readable representation of the stack trace.
        //
        // Returns:
        //     A readable representation of the stack trace.       
        
    }
}
