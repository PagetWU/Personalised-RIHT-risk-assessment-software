$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$src = Join-Path $root "RIHT_demo_launcher.cs"
$out = Join-Path $root "RIHT_demo_launcher.exe"

Add-Type `
  -TypeDefinition (Get-Content -LiteralPath $src -Raw) `
  -ReferencedAssemblies "System.Windows.Forms", "System.Drawing" `
  -OutputAssembly $out `
  -OutputType WindowsApplication

Write-Host "Built $out"
