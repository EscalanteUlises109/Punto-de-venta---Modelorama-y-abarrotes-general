param(
    [string]$Root = (Resolve-Path "..\"),
    [switch]$DryRun
)

# Purpose: Strip comments from source files safely.
# Supported: .cs, .razor, .xaml, .html, .css, .js
# Excludes: bin, obj, .git, wwwroot/lib

Write-Host "Scanning: $Root"

$extensions = @('*.cs','*.razor','*.xaml','*.html','*.css','*.js')
$excludeDirs = @('bin','obj','.git','.vs','wwwroot\\lib')

function Should-Exclude($path) {
    foreach($ex in $excludeDirs){
        if($path -like "*\$ex\*") { return $true }
    }
    return $false
}

$files = @()
foreach($ext in $extensions){
    $files += Get-ChildItem -Path $Root -Recurse -Include $ext -File -ErrorAction SilentlyContinue |
        Where-Object { -not (Should-Exclude $_.FullName) }
}

Write-Host "Found $($files.Count) files to process"

function Strip-CStyle($text){
    # Remove /* */ comments (non-greedy, dotall)
    $text = [regex]::Replace($text, "/\*.*?\*/", '', 'Singleline')
    # Remove // comments (not inside strings is hard; best effort: remove // to end of line when not preceded by ://)
    $text = [regex]::Replace($text, "(?<!:)//.*$", '', 'Multiline')
    return $text
}

function Strip-HTML($text){
    # Remove <!-- ... -->
    return [regex]::Replace($text, "<!--([\s\S]*?)-->", '')
}

function Strip-CSS($text){
    # Remove /* ... */ and // line comments used in some toolchains
    $text = [regex]::Replace($text, "/\*.*?\*/", '', 'Singleline')
    $text = [regex]::Replace($text, "(?<!:)//.*$", '', 'Multiline')
    return $text
}

function Process-File($file){
    $orig = Get-Content -Raw -Path $file.FullName
    $updated = $orig
    switch -Regex ($file.Extension.ToLower()) {
        '\.cs'    { $updated = Strip-CStyle $updated }
        '\.razor' { $updated = (Strip-HTML (Strip-CStyle $updated)) }
        '\.xaml'  { $updated = Strip-HTML $updated }
        '\.html'  { $updated = Strip-HTML $updated }
        '\.css'   { $updated = Strip-CSS $updated }
        '\.js'    { $updated = Strip-CStyle $updated }
        default    { }
    }
    if($DryRun){
        if($updated -ne $orig){ Write-Host "Would change: $($file.FullName)" }
    } else {
        if($updated -ne $orig){
            Set-Content -Path $file.FullName -Value $updated -Encoding UTF8
            Write-Host "Updated: $($file.FullName)"
        }
    }
}

$files | ForEach-Object { Process-File $_ }

Write-Host "Done."
