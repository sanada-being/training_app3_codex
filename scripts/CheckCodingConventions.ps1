Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Test-HasXmlComment {
    param(
        [string[]]$Lines,
        [int]$Index
    )

    $cursor = $Index - 1
    while ($cursor -ge 0 -and [string]::IsNullOrWhiteSpace($Lines[$cursor])) {
        $cursor--
    }

    if ($cursor -lt 0) {
        return $false
    }

    return $Lines[$cursor] -match "^\s*///"
}

function Get-FieldIdentifier {
    param([string]$Line)

    if ($Line -notmatch "^\s*(public|protected|internal|private)\b") {
        return $null
    }
    if ($Line -match "\bevent\b") {
        return $null
    }

    $equalIndex = $Line.IndexOf("=")
    $semicolonIndex = $Line.IndexOf(";")
    $delimiter = -1

    if ($equalIndex -ge 0 -and $semicolonIndex -ge 0) {
        $delimiter = [Math]::Min($equalIndex, $semicolonIndex)
    } elseif ($equalIndex -ge 0) {
        $delimiter = $equalIndex
    } elseif ($semicolonIndex -ge 0) {
        $delimiter = $semicolonIndex
    }

    if ($delimiter -lt 0) {
        return $null
    }

    $prefixPart = $Line.Substring(0, $delimiter)
    if ($prefixPart.Contains("{") -or $prefixPart.Contains("(")) {
        return $null
    }

    $nameMatch = [regex]::Match($prefixPart, "([A-Za-z_][A-Za-z0-9_]*)\s*$")
    if (-not $nameMatch.Success) {
        return $null
    }

    return $nameMatch.Groups[1].Value
}

function Split-Lines {
    param([string]$Text)
    return [regex]::Split($Text, "`r?`n")
}

$root = Get-Location
$targetRoots = @(
    "src",
    "Sales Management App/Sales Management App",
    "tests"
)

$files = New-Object System.Collections.Generic.List[string]
foreach ($targetRoot in $targetRoots) {
    $absolutePath = Join-Path $root $targetRoot
    if (-not (Test-Path $absolutePath)) {
        continue
    }

    Get-ChildItem -Path $absolutePath -Recurse -File -Filter "*.cs" |
        Where-Object { $_.Name -notlike "*.Designer.cs" } |
        ForEach-Object { $files.Add($_.FullName) }
}

$violations = New-Object System.Collections.Generic.List[string]

foreach ($file in $files) {
    $text = [System.IO.File]::ReadAllText($file, [System.Text.Encoding]::UTF8)
    $lines = Split-Lines -Text $text

    for ($index = 0; $index -lt $lines.Length; $index++) {
        $line = $lines[$index]
        $lineNo = $index + 1
        $relativeFile = Resolve-Path -LiteralPath $file -Relative

        if ($line -match "^\s*(//|/\*|\*)" -and $line -match "Add by|TODO|Todo|Hack|Undone|\d{4}/\d{2}/\d{2}") {
            $violations.Add("${relativeFile}:$lineNo prohibited comment keyword detected.")
        }

        if ($line -match "^\s*(public|internal|private|protected)\s+(?:sealed\s+|static\s+|abstract\s+|partial\s+)*class\s+([A-Za-z_][A-Za-z0-9_]*)") {
            if (-not (Test-HasXmlComment -Lines $lines -Index $index)) {
                $violations.Add("${relativeFile}:$lineNo class declaration is missing XML docs.")
            }
        }

        if ($line -match "^\s*(public|protected)\b" -and $line -notmatch "\bclass\b") {
            if (-not (Test-HasXmlComment -Lines $lines -Index $index)) {
                $violations.Add("${relativeFile}:$lineNo public/protected member is missing XML docs.")
            }
        }

        $fieldName = Get-FieldIdentifier -Line $line
        if ($null -ne $fieldName) {
            $access = ([regex]::Match($line, "^\s*(public|protected|internal|private)\b")).Groups[1].Value
            $isConst = $line -match "\bconst\b"
            $isStatic = $line -match "\bstatic\b"

            if ($isConst) {
                if ($fieldName -notmatch "^C_") {
                    $violations.Add("${relativeFile}:$lineNo const field must start with C_.")
                }
                continue
            }

            if ($access -eq "public" -and $isStatic) {
                if ($fieldName -notmatch "^G_") {
                    $violations.Add("${relativeFile}:$lineNo public static field must start with G_.")
                }
                continue
            }

            if ($access -in @("private", "protected", "internal")) {
                if ($fieldName -notmatch "^F[A-Z]") {
                    $violations.Add("${relativeFile}:$lineNo non-public field must start with F.")
                }
            }
        }
    }
}

if ($violations.Count -gt 0) {
    Write-Host "Coding conventions check: FAILED ($($violations.Count) violations)"
    $violations | ForEach-Object { Write-Host "- $_" }
    exit 1
}

Write-Host "Coding conventions check: OK"
