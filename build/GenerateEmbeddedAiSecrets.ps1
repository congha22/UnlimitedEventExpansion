param(
    [Parameter(Mandatory = $true)]
    [string]$EnvFile,

    [Parameter(Mandatory = $true)]
    [string]$OutputFile
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Read-DotEnv {
    param([string]$Path)

    $result = @{}
    if (-not (Test-Path -LiteralPath $Path)) {
        return $result
    }

    foreach ($line in Get-Content -LiteralPath $Path) {
        $trimmed = $line.Trim()
        if ([string]::IsNullOrWhiteSpace($trimmed)) {
            continue
        }

        if ($trimmed.StartsWith('#')) {
            continue
        }

        $index = $trimmed.IndexOf('=')
        if ($index -lt 0) {
            continue
        }

        $name = $trimmed.Substring(0, $index).Trim()
        $value = $trimmed.Substring($index + 1).Trim()

        if (($value.StartsWith('"') -and $value.EndsWith('"')) -or ($value.StartsWith("'") -and $value.EndsWith("'"))) {
            if ($value.Length -ge 2) {
                $value = $value.Substring(1, $value.Length - 2)
            }
        }

        if (-not [string]::IsNullOrWhiteSpace($name)) {
            $result[$name] = $value
        }
    }

    return $result
}

function Get-EnvValue {
    param(
        [hashtable]$EnvValues,
        [string[]]$Names
    )

    foreach ($name in $Names) {
        if ($EnvValues.ContainsKey($name)) {
            return [string]$EnvValues[$name]
        }
    }

    return ''
}

function To-CSharpStringLiteral {
    param([string]$Text)

    if ($null -eq $Text) {
        return '""'
    }

    $escaped = $Text.Replace('\\', '\\\\').Replace('"', '\\"')
    return '"' + $escaped + '"'
}

$envValues = Read-DotEnv -Path $EnvFile

$primaryRuntimeKey = Get-EnvValue -EnvValues $envValues -Names @(
    'OPENAI_SHARED_RUNTIME_KEY_PRIMARY',
    'OPENAI_SHARED_RUNTIME_KEY'
)
$primaryAdminKey = Get-EnvValue -EnvValues $envValues -Names @(
    'OPENAI_SHARED_ADMIN_KEY_PRIMARY',
    'OPENAI_SHARED_ADMIN_KEY'
)
$secondaryRuntimeKey = Get-EnvValue -EnvValues $envValues -Names @(
    'OPENAI_SHARED_RUNTIME_KEY_SECONDARY',
    'OPENAI_SHARED_RUNTIME_KEY_2'
)
$secondaryAdminKey = Get-EnvValue -EnvValues $envValues -Names @(
    'OPENAI_SHARED_ADMIN_KEY_SECONDARY',
    'OPENAI_SHARED_ADMIN_KEY_2'
)

$primaryRuntimeLiteral = To-CSharpStringLiteral -Text $primaryRuntimeKey
$primaryAdminLiteral = To-CSharpStringLiteral -Text $primaryAdminKey
$secondaryRuntimeLiteral = To-CSharpStringLiteral -Text $secondaryRuntimeKey
$secondaryAdminLiteral = To-CSharpStringLiteral -Text $secondaryAdminKey

$outputDirectory = Split-Path -Parent $OutputFile
if (-not (Test-Path -LiteralPath $outputDirectory)) {
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}

$generated = @"
namespace UnlimitedEventExpansion
{
    internal static class EmbeddedAiSecrets
    {
        internal const string SharedOpenAiRuntimeKeyPrimary = $primaryRuntimeLiteral;
        internal const string SharedOpenAiAdminKeyPrimary = $primaryAdminLiteral;
        internal const string SharedOpenAiRuntimeKeySecondary = $secondaryRuntimeLiteral;
        internal const string SharedOpenAiAdminKeySecondary = $secondaryAdminLiteral;
    }
}
"@

Set-Content -LiteralPath $OutputFile -Value $generated -NoNewline
