$env:PATH = "C:\Program Files\nodejs;" + $env:PATH
Set-Location "C:\Users\Alex\source\repos\Hakaton\front"
& "C:\Program Files\nodejs\npm.cmd" install
"DONE" | Out-File "install_done.txt"
