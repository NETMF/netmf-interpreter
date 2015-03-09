
var templateFile = WScript.Arguments.Named.Item("TemplateFile");
var propertiesString = WScript.Arguments.Named.Item("Properties");
var outputFileName = WScript.Arguments.Named.Item("OutputFile");


var clCommand = "cl /EP /C "  + templateFile;

var properties = propertiesString.split(";");

for (i=0; i<properties.length;++i) 
{
    var property = properties[i].split("=");
    var pname = property[0];
    var pvalue = property[1];
    if ( pvalue.charAt(0) == '#' && pvalue.charAt(pvalue.length-1) == '#' )
    {
        pvalue = "\\\"\"" + pvalue.substr(1,pvalue.length-2) + "\"\\\"";
    }
    else
    {
        pvalue = "\"" + pvalue + "\"";
    }
    
    clCommand += " /D" + pname + "=" + pvalue;
}

var WshShell = new ActiveXObject("WScript.Shell");
var oExec = WshShell.Exec(clCommand);

var outputText = oExec.StdOut.ReadAll();

if (oExec.ExitCode != 0)
{
     WScript.Echo("Warning: Non-zero exit code");
}
else
{
    var same = false;

    var fso = new ActiveXObject("Scripting.FileSystemObject");

    if(fso.FileExists(outputFileName))
    {
        var fileInfo = fso.GetFile(outputFileName);
        if(fileInfo.Size > 0)
        {
            var inputFile = fso.OpenTextFile(outputFileName, 1);
            var inputText = inputFile.ReadAll();

            if(inputText == outputText)
            {
                same = true;
            }

            inputFile.Close();
        }
    }

    // let's check if the new file is the same as the old one
    if(!same)        
    {
        var outputFile = fso.CreateTextFile(outputFileName, true);

        outputFile.Write(outputText);
        outputFile.Close();    
    }
}


