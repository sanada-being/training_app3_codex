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

function Split-Parameters {
    param([string]$ParameterList)

    $trimmed = $ParameterList.Trim()
    if ([string]::IsNullOrWhiteSpace($trimmed)) {
        return @()
    }

    $parts = $trimmed -split ","
    return $parts | ForEach-Object { $_.Trim() } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
}

function Get-ParameterIdentifier {
    param([string]$ParameterText)

    $working = $ParameterText
    $working = $working -replace "^\s*\[[^\]]+\]\s*", ""
    $working = $working -replace "^\s*params\s+", ""
    $working = $working -replace "^\s*this\s+", ""

    if ($working.Contains("=")) {
        $working = ($working -split "=")[0].Trim()
    }

    $tokens = $working -split "\s+"
    if ($tokens.Length -lt 2) {
        return $null
    }

    return $tokens[$tokens.Length - 1]
}

function Test-IsMethodOrCtorParameterNameValid {
    param([string]$Name)

    if ([string]::IsNullOrWhiteSpace($Name)) {
        return $true
    }

    if ($Name -in @("sender", "e")) {
        return $true
    }

    return $Name -match "^v[A-Z]"
}

function Test-IsLocalVariableNameValid {
    param([string]$Name)

    if ([string]::IsNullOrWhiteSpace($Name)) {
        return $true
    }

    if ($Name -in @("i", "j", "k")) {
        return $true
    }

    return $Name -match "^w[A-Z]"
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

    $relativeFile = Resolve-Path -LiteralPath $file -Relative

    for ($index = 0; $index -lt $lines.Length; $index++) {
        $line = $lines[$index]
        $lineNo = $index + 1

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

    $signatureMatches = [regex]::Matches(
        $text,
        "(?ms)^\s*(?:public|private|protected|internal)\s+[^\n;{}]*?\((?<params>[^)]*)\)"
    )
    foreach ($signatureMatch in $signatureMatches) {
        $paramsGroup = $signatureMatch.Groups["params"].Value
        if ([string]::IsNullOrWhiteSpace($paramsGroup)) {
            continue
        }

        $lineNo = 1 + ($text.Substring(0, $signatureMatch.Index).Split("`n").Length - 1)
        foreach ($param in Split-Parameters -ParameterList $paramsGroup) {
            $name = Get-ParameterIdentifier -ParameterText $param
            if ($null -eq $name) {
                continue
            }

            if (-not (Test-IsMethodOrCtorParameterNameValid -Name $name)) {
                $violations.Add("${relativeFile}:$lineNo method parameter '$name' must start with v.")
            }
        }
    }

    for ($index = 0; $index -lt $lines.Length; $index++) {
        $line = $lines[$index]
        $lineNo = $index + 1

        if ($line -match "^\s*(?:public|private|protected|internal)?\s*(?:sealed\s+|static\s+|abstract\s+|partial\s+)*enum\s+([A-Za-z_][A-Za-z0-9_]*)") {
            $enumName = $Matches[1]
            if ($enumName -notmatch "Enum$") {
                $violations.Add("${relativeFile}:$lineNo enum '$enumName' must end with Enum.")
            }
        }

        if ($line -match "^\s*var\s+([A-Za-z_][A-Za-z0-9_]*)\b") {
            $localName = $Matches[1]
            if (-not (Test-IsLocalVariableNameValid -Name $localName)) {
                $violations.Add("${relativeFile}:$lineNo local variable '$localName' must start with w.")
            }
        }

        if ($line -match "^\s*foreach\s*\([^)]*\s+([A-Za-z_][A-Za-z0-9_]*)\s+in\s+") {
            $localName = $Matches[1]
            if (-not (Test-IsLocalVariableNameValid -Name $localName)) {
                $violations.Add("${relativeFile}:$lineNo foreach variable '$localName' must start with w.")
            }
        }

        if ($line -match "^\s*catch\s*\([^)]*\s+([A-Za-z_][A-Za-z0-9_]*)\s*\)") {
            $localName = $Matches[1]
            if (-not (Test-IsLocalVariableNameValid -Name $localName)) {
                $violations.Add("${relativeFile}:$lineNo catch variable '$localName' must start with w.")
            }
        }

        if ($line -match "\bout\s+var\s+([A-Za-z_][A-Za-z0-9_]*)\b") {
            $localName = $Matches[1]
            if (-not (Test-IsLocalVariableNameValid -Name $localName)) {
                $violations.Add("${relativeFile}:$lineNo out var '$localName' must start with w.")
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
