using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetServerScriptUtility
{
    partial class TemplateClass
    {
        public static string FullClass = @"//$FullClass
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
        /// Write an Information message to the log file and debug output. 
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
        public static void DumpParameters(string[] parameterNames, params object[] args)
        {{
            if(args != null) 
            {{
                var currentArg = """";
                try 
                {{
			        System.Diagnostics.Debug.WriteLine(""ScriptMethod"" + new string('*', 100));

			        for(int i = 0; i < args.Length; i++)
			        {{
                        currentArg = parameterNames[i];
                        System.Diagnostics.Debug.WriteLine(""ScriptMethod"" + new string('-', 100));
                        System.Diagnostics.Debug.WriteLine(""ScriptMethod Parameter: "" + currentArg);
                        ObjectDumper.Dump(args[i]);
                    }}
                }}
                catch(Exception e)
                {{
                    System.Diagnostics.Debug.WriteLine(""Exception parsing argument {{0}}, failed with: {{1}}"", currentArg, (e.GetBaseException() ?? e).Message);
                }}
                finally 
                {{
                    System.Diagnostics.Debug.WriteLine(""ScriptMethod"" + new string('*', 100));
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
                        //Write(""Parsing ComplexType: {{0}} "", objectType.FullName);

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
}}";

    }
}
