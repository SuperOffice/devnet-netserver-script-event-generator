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

        public static string AfterAsyncEvent = @"
        public static void After{0}Async({1})
        {{
            // LogMessageHere
        }}";
    }
}
