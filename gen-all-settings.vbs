' Create a Wscript.Shell object to run external programs
Set objShell = CreateObject("Wscript.Shell")

' Run PowerShell with:
' -noexit: Keep the window open
' -ExecutionPolicy Bypass: Bypass script execution restrictions
' -File: Run the specified script in the current directory
' Ensure the script is in the same directory or provide the full path.

objShell.run "powershell -noexit -ExecutionPolicy Bypass -File .\gen-all-settings.ps1"
