using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetServerScriptUtility
{
    partial class TemplateClass
    {
        // {0} == Event Name
        // {1} == Parameters as delimited String
        // {2} == Agent Name
        // {3} == Parameters as params object array
        // {4} == Quoted parameter variable names as delimited string

        public static string AfterEvent = @"
        public static void After{0}({1})
        {{
             string [] parameterNames = new string [] {{ {4} }};
             LogInfoMessage(""ScriptMethod Type: {2}"",""Method: After{0}"");
             DumpParameters(parameterNames, {3});
        }}";

    }
}
