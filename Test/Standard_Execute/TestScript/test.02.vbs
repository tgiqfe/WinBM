Option Explicit
dim fso, shell, text, path
set shell = CreateObject("WScript.Shell")
path = shell.ExpandEnvironmentStrings("%outputDir%") & "\test02.txt"
set fso = CreateObject("Scripting.FileSystemObject")
set text = fso.OpenTextFile(path, 2, true)
text.WriteLine("test02")
text.Close
