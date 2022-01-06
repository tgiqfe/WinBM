var shell = WScript.CreateObject("WScript.Shell");
var path = shell.ExpandEnvironmentStrings("%outputDir%") + "\\test03.txt";
var fso = WScript.CreateObject("Scripting.FileSystemObject")
var text = fso.OpenTextFile(path, 2, true);
text.WriteLine("test03");
text.Close();

