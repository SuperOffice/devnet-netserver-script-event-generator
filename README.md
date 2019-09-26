# devnet-netserver-script-event-generator
.NET console app to generate all NetServer web service scripts.

This application uses SuperOffice nuget packages that must be restored before compiling/using the app to generate the script files.

Compile, then run:

```txt
NetServerScriptUtility.exe "C:\temp\webscripts"
```

## Script Output:
The output is a FullClass style file that contains of all Before, After and AfterAsync methods for each web service Agent. It supports logging output to debug listeners, which makes it easy to observe what methods are invoked

![Output](/assets/images/NetServerScriptGeneratorOutput.PNG)

#### Example from the ContactAgent.cs file.

```C#
//$FullClass
//$ReferencedAssembly:System.Drawing.dll
//$ReferencedAssembly:C:\Program Files\SuperOffice\SuperOffice SM Web\SuperOffice84R08\bin\SuperOffice.Plugins.dll
//$ReferencedAssembly:C:\Program Files\SuperOffice\SuperOffice SM Web\SuperOffice84R08\bin\SuperOffice.Contracts.dll

// The two SuperOffice referenced assemblies above may be removed with SuperOffice v8.5 and higher.
// Otherwise, ensure the full path points to the web site, or netserver web services, bin directory

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
{
    public static class ContactAgentScript
    {
        static void Main() { }

        public static void BeforeCreateDefaultContactEntity(ref object eventState)
        {
            // Log Message
            LogInfoMessage("ScriptMethod Type: ContactAgent","Method BeforeCreateDefaultContactEntity / Method Params: ");
            string [] mParameters = new string [] { "eventState" };
            DumpParameters(mParameters, eventState);
        }

        public static void AfterCreateDefaultContactEntity(ref SuperOffice.CRM.Services.ContactEntity returnValue, ref object eventState)
        {
            // Log Message
            LogInfoMessage("ScriptMethod Type: ContactAgent","Method: AfterCreateDefaultContactEntity / Method Params: ref SuperOffice.CRM.Services.ContactEntity returnValue, ");
            string [] mParameters = new string [] { "returnValue","eventState" };
            DumpParameters(mParameters, returnValue, eventState);
        }

        public static void AfterCreateDefaultContactEntityAsync(ref SuperOffice.CRM.Services.ContactEntity returnValue, ref object eventState)
        {
            // LogMessageHere
        }

        ...
    }
}
```

## Output in a Viewer

Applications like [DebugView](https://docs.microsoft.com/en-us/sysinternals/downloads/debugview) (Run As Administrator) can be used to monitor the real-time output.

![Output](/assets/images/FilterHighlights.PNG)

---

![Output](/assets/images/DebugViewCaptureMenu.png)


---

![Output](/assets/images/NetServerScriptDebugOutput.PNG)