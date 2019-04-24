using SuperOffice.CRM.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NetServerScriptUtility
{
    class Program
    {
        static void Main(string[] args)
        {
            //**************************************************************************
            //CHANGE THE FOLDER PATH

            string fullPath = Environment.CurrentDirectory;

            if (args != null && !string.IsNullOrWhiteSpace(args[0]))
            {
                fullPath = Path.GetFullPath(args[0]);
            }

            if (!Directory.Exists(fullPath))
            {
                try
                {
                    Directory.CreateDirectory(fullPath);
                }
                catch (Exception ex)
                {
                    string error = ex.Message;

                    while (ex.InnerException != null)
                    {
                        ex = ex.InnerException;
                        error = ex.Message;
                    }

                    Console.WriteLine(string.Format("Error trying to write to folder {0}\r\n{1}", fullPath, error));
                }
            }

            fullPath += "\\";

            //**************************************************************************

            string classStructure = @"//$FullClass
//$ReferencedAssembly:System.Drawing.dll
//$ReferencedAssembly:C:\Program Files\SuperOffice\SuperOffice SM Web\SuperOffice84R08\bin\SuperOffice.Plugins.dll
//$ReferencedAssembly:C:\Program Files\SuperOffice\SuperOffice SM Web\SuperOffice84R08\bin\SuperOffice.Contracts.dll
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing; //for ProjectAgent && PersonAgent
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using SuperOffice.CRM.Services;

namespace NetServerScriptClass
{{
    public static class {0}Script
    {{
        static void Main() {{ }}        
{1}
        /// <summary>
        /// Script compilation and runtime errors are logged using normal NetServer logging features. 
        /// The script itself may contain an OnError( errorDetails ) event handler that can report errors to the script author.
        /// </summary>
        public static void OnError(string message)
        {{
            //Log to your System or NetServer
            SuperOffice.Diagnostics.LogEntryInfo logError = new SuperOffice.Diagnostics.LogEntryInfo(System.Diagnostics.EventLogEntryType.Error);
            logError.AddElement(typeof({0}Script), message, ""No Details"");
            SuperOffice.Diagnostics.SoLogger.Logger.LogEntry(logError, true);
        }}

        /// <summary>
        /// Write an Information message to the log file. 
        /// </summary>
        public static void LogInfoMessage(string message, string details)
        {{
            //Log to your System or NetServer
            SuperOffice.Diagnostics.LogEntryInfo logInfo = new SuperOffice.Diagnostics.LogEntryInfo(System.Diagnostics.EventLogEntryType.Information);
            logInfo.AddElement(typeof({0}), message, details);
            SuperOffice.Diagnostics.SoLogger.Logger.LogEntry(logInfo, true);
            System.Diagnostics.Debug.WriteLine(message + "": "" + details);
        }}

        /// <summary>
        /// Gets a readable view of an object. 
        /// </summary>
        public static void DumpParameters(params object[] args)
        {{
            DumpParameters(null, args);
        }}      

        /// <summary>
        /// Gets a readable view of an object. 
        /// </summary>
        public static void DumpParameters(string[] paramList, params object[] args)
        {{
            if(args != null) 
            {{
                var currentArg = """";
                try 
                {{
			        System.Diagnostics.Debug.WriteLine(""ScriptMethod"" + new string('*', 50));

			        for(int i = 0; i < args.Length; i++)
			        {{
                        currentArg = paramList[i];
                        System.Diagnostics.Debug.WriteLine(""ScriptMethod"" + new string('-', 50));
                        System.Diagnostics.Debug.WriteLine(""ScriptMethod Parameter: "" + paramList[i]);
                        ObjectDumper.Dump(args[i]);
                    }}
                }}
                catch(Exception e)
                {{
                    System.Diagnostics.Debug.WriteLine(""Exception parsing argument {{0}}, failed with: {{1}}"", currentArg, (e.GetBaseException() ?? e).Message);
                }}
                finally 
                {{
                    System.Diagnostics.Debug.WriteLine(""ScriptMethod"" + new string('*', 50));
                }}
            }}
        }}        
    }}

    //http://stackoverflow.com/questions/852181/c-printing-all-properties-of-an-object
    public class ObjectDumper
    {{
        private int _currentIndent;
        private readonly int _indentSize;
        private readonly StringBuilder _stringBuilder;
        private readonly Dictionary<object,int> _hashListOfFoundElements;
        private readonly char _indentChar;
        private readonly int _depth;
        private int _currentLine;

        private ObjectDumper(int depth, int indentSize, char indentChar)
        {{
            _depth = depth;
            _indentSize = indentSize;
            _indentChar = indentChar;
            _stringBuilder = new StringBuilder();
            _hashListOfFoundElements = new Dictionary<object,int>();
        }}

        public static string Dump(object element, int depth = 4,int indentSize=2,char indentChar=' ')
        {{
            var instance = new ObjectDumper(depth, indentSize, indentChar);
            return instance.DumpElement(element, true);
        }}

        private string DumpElement(object element, bool isTopOfTree = false)
        {{
            if (_currentIndent > _depth) {{ return null; }}
            if (element == null || element is string)
            {{
                Write(FormatValue(element));
            }}
            else if (element is ValueType)
            {{
                Type objectType = element.GetType();
                Write(""ValueType: {{0}} "", objectType.FullName);

                bool isWritten = false;
                if (objectType.IsGenericType)
                {{
                    Type baseType = objectType.GetGenericTypeDefinition();
                    if (baseType == typeof(KeyValuePair<,>))
                    {{
                        isWritten = true;
                        Write(""Key: "");
                        _currentIndent++;
                        DumpElement(objectType.GetProperty(""Key"").GetValue(element, null));
                        _currentIndent--;
                        Write(""Value:"");
                        _currentIndent++;
                        DumpElement(objectType.GetProperty(""Value"").GetValue(element, null));
                        _currentIndent--;
                    }}
                }}

                if (!isWritten)
                {{
                    Write(FormatValue(element));
                }}
            }}
            else
            {{
                Type objectType = element.GetType();
                //Write(""ComplexType: {{0}} "", objectType.FullName);

                IEnumerable enumerableElement = element as IEnumerable;
                if (enumerableElement != null)
                {{
                    foreach (object item in enumerableElement)
                    {{
                        if (item is IEnumerable && !(item is string))
                        {{
                            _currentIndent++;
                            DumpElement(item);
                            _currentIndent--;
                        }}
                        else
                        {{
                            DumpElement(item);
                        }}
                    }}
                }}
                else
                {{

                    Write(""{{0}}(HashCode:{{1}})"", objectType.FullName, element.GetHashCode());
                    if (!AlreadyDumped(element))
                    {{
                        Write(""Parsing ComplexType: {{0}} "", objectType.FullName);

                        _currentIndent++;
                        MemberInfo[] members = objectType.GetMembers(BindingFlags.Public | BindingFlags.Instance);
                        foreach (var memberInfo in members)
                        {{


                            // do not list out all repetitive entity properties 
                            if(memberInfo.Name == ""FieldProperties"") continue;

                            var fieldInfo = memberInfo as FieldInfo;
                            var propertyInfo = memberInfo as PropertyInfo;

                            if (fieldInfo == null && (propertyInfo == null || !propertyInfo.CanRead || propertyInfo.GetIndexParameters().Length > 0))
                                continue;

                            var type = fieldInfo != null ? fieldInfo.FieldType : propertyInfo.PropertyType;
                            
                            object value;
                            try
                            {{
                                value = fieldInfo != null
                                                   ? fieldInfo.GetValue(element)
                                                   : propertyInfo.GetValue(element, null);
                            }}
                            catch (Exception e)
                            {{
                                Write(""{{0}} failed with:{{1}}"", memberInfo.Name, (e.GetBaseException() ?? e).Message);
                                continue;
                            }}

                            if (type.IsValueType || type == typeof(string))
                            {{
                                Write(""{{0}}: {{1}}"", memberInfo.Name, FormatValue(value));
                            }}
                            else
                            {{
                                var isEnumerable = typeof(IEnumerable).IsAssignableFrom(type);
                                Write(""{{0}}: {{1}}"", memberInfo.Name, isEnumerable? ""..."" : ""{{ }}"");

                                _currentIndent++;
                                //Write(""Dumping ComplexType: {{0}} "", type.FullName);
                                DumpElement(value);
                                _currentIndent--;
                            }}
                        }}
                        _currentIndent--;
                    }}
                }}




            }}

            return isTopOfTree? _stringBuilder.ToString():null;
        }}

        private bool AlreadyDumped(object value)
        {{
            int lineNo;

            if (value == null)
                return false;
            
            if (_hashListOfFoundElements.TryGetValue(value, out lineNo))
            {{
                Write(""( reference already dumped - line:{{0}} )"", lineNo);
                return true;
            }}

            _hashListOfFoundElements.Add(value, _currentLine);

            return false;
        }}

        private void Write(string value, params object[] args)
        {{
            var space = new string(_indentChar, _currentIndent * _indentSize);

            if (args != null)
                value = string.Format(value, args);

            System.Diagnostics.Debug.WriteLine(""ScriptMethod    "" + space + value);
            
            _stringBuilder.AppendLine(space + value);
            _currentLine++;
        }}

        private string FormatValue(object o)
        {{
            if (o == null)
                return (""null"");

            if (o is DateTime)
                return (((DateTime)o).ToShortDateString());

            if (o is string)
                return string.Format(""\""{{0}}\"""",(string)o);

            if (o is char)
            {{
                if (o.Equals('\0'))
                {{
                    return ""''"";
                }}
                else
                {{
                    return ""'"" + (char)o + ""'"";
                }}
            }}

            if (o is ValueType)
                return (o.ToString());

            if (o is IEnumerable)
                return (""..."");

            return (""{{ }}"");
        }}
    }}
}}
";

            string beforeEventFormat = "\t\tpublic static void Before{0}({1}ref object eventState)\n\t\t{{\n\t\t\t// Log Message\n\t\t\tLogInfoMessage(\"ScriptMethod Type: {2}\",\"Method: Before{0} / Method Params: {1}\");\n\t\t\tstring [] mParameters = new string [] {{ {4} }};\n\t\t\tDumpParameters(mParameters, {3});\n\t\t}}\n\n";
            string afterEventFormat = "\t\tpublic static void After{0}({1}ref object eventState)\n\t\t{{\n\t\t\t// Log Message\n\t\t\tLogInfoMessage(\"ScriptMethod Type: {2}\",\"Method: After{0} / Method Params: {1}\");\n\t\t\tstring [] mParameters = new string [] {{ {4} }};\n\t\t\tDumpParameters(mParameters, {3});\n\t\t}}\n\n";
            string afterEventAsyncFormat = "\t\tpublic static void After{0}Async({1}ref object eventState)\n\t\t{{\n\t\t\t// LogMessageHere\n\t\t}}\n\n";

            //Define the SuperOffice IAgent type
            var type = typeof(IAgent);

            //Get all Agents in the SuperOffice.CRM.Services namespace 
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p =>
                    type.IsAssignableFrom(p) &&
                    p.IsClass &&
                    string.Equals(p.Namespace, "SuperOffice.CRM.Services", StringComparison.InvariantCultureIgnoreCase));


            //Avoid these methods
            var avoid = new[] { "Dispose", "get_InnerAgent" };


            //Loop over each agent and build up the full class file
            foreach (var agent in types)
            {
                StringBuilder classStringBuilder = new StringBuilder();
                StringBuilder methodStringBuilder = new StringBuilder();

                //Get all of the Agent Methods, not object methods and not in the avoids string array (above).
                var methods =
                    agent.GetMethods()
                    .Where(m =>
                    !avoid.Contains(m.Name) &&
                    !typeof(object).GetMethods().Select(me => me.Name).Contains(m.Name));

                //Loops over the methods and build up each script prototype.
                foreach (var method in methods)
                {
                    var parameters = method.GetParameters();
                    var paramString = "";
                    var paramArray = "";
                    var paramList = new List<string>();

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        paramList.Add(parameters[i].Name);
                        paramString += parameters[i].ParameterType + " " + parameters[i].Name + ", ";
                        paramArray += "\"" + parameters[i].Name + "\",";
                    }

                    paramList.Add("eventState");

                    var beforeMethodProtoType = string.Format(beforeEventFormat, method.Name, paramString, agent.Name, string.Join(", ", paramList), paramArray + "\"eventState\"");

                    // Prepare the return value that can be manupulated in the After events...
                    var returnType = (method.ReturnParameter != null) ? method.ReturnParameter : null;

                    if (returnType != null && !string.Equals("Void", returnType.ParameterType.Name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        paramString += "ref " + returnType.ParameterType.FullName + " " + "returnValue, ";
                        paramList.Insert(paramList.Count - 1, "returnValue");
                        paramArray += "\"returnValue\",";
                    }


                    var afterMethodPrototype = string.Format(afterEventFormat, method.Name, paramString, agent.Name, string.Join(", ", paramList), paramArray + "\"eventState\"");

                    var afterAsyncMethodPrototype = string.Format(afterEventAsyncFormat, method.Name, paramString);

                    methodStringBuilder.AppendLine(beforeMethodProtoType);
                    methodStringBuilder.AppendLine(afterMethodPrototype);
                    methodStringBuilder.AppendLine(afterAsyncMethodPrototype);

                }

                //build up the class file with the methods
                classStringBuilder.AppendLine(string.Format(classStructure, agent.Name, methodStringBuilder.ToString()));

                //write the script file out of the save folder
                File.WriteAllText(fullPath + agent.Name + ".cs", classStringBuilder.ToString());

            }
        }
    }
}
