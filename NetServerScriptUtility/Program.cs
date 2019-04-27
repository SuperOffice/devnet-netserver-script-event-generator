using SuperOffice.CRM.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NetServerScriptUtility
{
    class VariableInfo
    {
        public string AgentName { get; set; }
        public string EventMethodName { get; set; }
        public string DataType { get; set; }
        public string Name { get; set; }
        public bool IsReturnDataType { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            GenerateAgentEvents(SetPath(args));
        }

        private static void GenerateAgentEvents(string fullPath)
        {
            // get all Agents in the SuperOffice.CRM.Services namespace 
            // loop over each agent and build up the full class file

            foreach (var agent in GetAllAgentTypes())
            {
                StringBuilder classStringBuilder = new StringBuilder();
                StringBuilder methodStringBuilder = new StringBuilder();

                // get all of the Agent Methods, not object methods and not in the avoids string array (above).
                // loop over each and build up event method prototypes.

                foreach (var method in GetAllAgentMethods(agent))
                {
                    var parameterList = GetParameterList(agent, method);

                    // Build the Before Event

                    methodStringBuilder.AppendLine(GetMethods(TemplateClass.BeforeEvent, agent.Name, method.Name, parameterList));

                    // Prepare the return value that can be manupulated in the After event...

                    PrepareForAfter(agent.Name, method, parameterList);

                    // Build the After and AfterAsync Events

                    methodStringBuilder.AppendLine(GetMethods(TemplateClass.AfterEvent, agent.Name, method.Name, parameterList));
                    methodStringBuilder.AppendLine(GetMethods(TemplateClass.AfterAsyncEvent, agent.Name, method.Name, parameterList));
                }

                // build up the class file with the methods

                classStringBuilder.AppendLine(string.Format(TemplateClass.FullClass, agent.Name, methodStringBuilder.ToString()));

                // write the class file to the save folder

                File.WriteAllText(fullPath + agent.Name + ".cs", classStringBuilder.ToString());

            }
        }

        private static List<VariableInfo> GetParameterList(Type agent, MethodInfo method)
        {
            var variableList = new List<VariableInfo>();

            var parameters = method.GetParameters();

            for (int i = 0; i < parameters.Length; i++)
            {
                variableList.Add(GetVariable(agent, method, parameters[i]));
            }

            // add eventState parameter

            variableList.Add(new VariableInfo() { AgentName = agent.Name, DataType = "object", EventMethodName = method.Name, IsReturnDataType = true, Name = "eventState" });

            return variableList;
        }

        private static IEnumerable<MethodInfo> GetAllAgentMethods(Type agent)
        {
            var avoid = new[] { "Dispose", "get_InnerAgent" };

            return agent.GetMethods()
                    .Where(m =>
                    !avoid.Contains(m.Name) &&
                    !typeof(object).GetMethods().Select(me => me.Name).Contains(m.Name));
        }

        private static IEnumerable<Type> GetAllAgentTypes()
        {
            //Define the SuperOffice IAgent type

            var type = typeof(IAgent);

            //Get all Agents in the SuperOffice.CRM.Services namespace 

            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p =>
                    type.IsAssignableFrom(p) &&
                    p.IsClass &&
                    string.Equals(p.Namespace, "SuperOffice.CRM.Services", StringComparison.InvariantCultureIgnoreCase));
        }

        private static VariableInfo GetVariable(Type agent, MethodInfo method, ParameterInfo parameterInfo)
        {
            return new VariableInfo
            {
                AgentName = agent.Name,
                DataType = parameterInfo.ParameterType.ToString(),
                Name = parameterInfo.Name,
                EventMethodName = method.Name,
                IsReturnDataType = false
            };
        }

        private static string GetMethods(string formatString, string agent, string method, List<VariableInfo> variableList)
        {
            var prototypeParamList = GetParamsPrototype(variableList);
            var rawParamList = GetRawParamsList(variableList);
            var quotedParamList = GetQuotedParamsList(variableList);
            return string.Format(formatString, method, prototypeParamList, agent, rawParamList, quotedParamList);
        }

        private static string GetQuotedParamsList(List<VariableInfo> variableList)
        {
            var sb = new StringBuilder();
            variableList.ForEach((v) => sb.AppendFormat("\"{0}\", ", v.Name));
            return sb.ToString().TrimEnd(',', ' ');
        }

        private static string GetRawParamsList(List<VariableInfo> variableList)
        {
            var sb = new StringBuilder();
            variableList.ForEach((v) => sb.AppendFormat("{0}, ", v.Name));
            return sb.ToString().TrimEnd(',', ' ');
        }

        private static string GetParamsPrototype(List<VariableInfo> variableList)
        {
            var sb = new StringBuilder();
            variableList.ForEach((v) => sb.AppendFormat("{0}{1} {2}, ", v.IsReturnDataType ? "ref " : "", v.DataType, v.Name));
            return sb.ToString().TrimEnd(',', ' ');
        }

        private static void PrepareForAfter(string agent, MethodInfo method, List<VariableInfo> variableList)
        {
            var returnType = (method.ReturnParameter != null) ? method.ReturnParameter : null;

            if (returnType != null && !string.Equals("Void", returnType.ParameterType.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                variableList.Insert(variableList.Count - 1, new VariableInfo()
                {
                    AgentName = agent,
                    DataType = returnType.ParameterType.FullName,
                    EventMethodName = method.Name,
                    IsReturnDataType = true,
                    Name = "returnValue"
                });
            }
        }

        private static string SetPath(string[] args)
        {
            var fullPath = Environment.CurrentDirectory;

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

            return fullPath += "\\";
        }
    }
}
