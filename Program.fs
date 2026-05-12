open System
open System.Diagnostics
open System.IO
open System.Text.Json
open Spectre.Console
open Statusline.ClaudeData

let logPath = Path.Combine(Path.GetDirectoryName Environment.ProcessPath, "statusline.log")
let log (str : string) = File.WriteAllText(logPath, str)

// Must be set before AnsiConsole is initialised, otherwise Spectre captures Console.Out with the default (OEM) encoding
Console.OutputEncoding <- System.Text.Encoding.UTF8
// Spectre.Console detection doesn't work with Claude Code
AnsiConsole.Profile.Capabilities.Ansi <- true

let gitBranchSubprocess() =
    let info = ProcessStartInfo("git", "branch --show-current")
    info.RedirectStandardOutput <- true
    info.RedirectStandardError <- true
    let p = Process.Start(info)
    if p.WaitForExit(1000) then p.StandardOutput.ReadToEnd().Trim() else "<timeout>"

let getGitBranch (data : ClaudeData) =
    let cacheFilePath = Path.Combine(Path.GetTempPath(), $"statusline-git-cache-{data.session_id}")
    let fileInfo = FileInfo cacheFilePath
    if (not fileInfo.Exists) || fileInfo.LastWriteTimeUtc < DateTime.UtcNow.AddSeconds -2 then
        File.WriteAllText(cacheFilePath, gitBranchSubprocess())
    $"\ue725 {File.ReadAllText cacheFilePath}"

let contextUsed (data : ClaudeData) =
     let usedPercentage =
        match data.context_window.used_percentage with
        | Some usedPercentage -> usedPercentage
        | None -> 0
     $"ctx: {usedPercentage}%%"

let rateLimit (limit: RateLimit) =
    let remaining = DateTimeOffset.FromUnixTimeSeconds limit.resets_at - DateTimeOffset.UtcNow
    let remainingStr =
        match remaining with
        | remaining when remaining.Days > 0 -> $"{remaining.Days}d"
        | remaining when remaining.Hours > 0 -> $"{remaining.Hours}h"
        | _ -> $"{remaining.Minutes}m"
    $"{remainingStr}: {100.0 - limit.used_percentage}%%"

let model (data : ClaudeData) =
    $"{data.model.display_name} ({data.effort.level})"

let folder (data : ClaudeData) =
    $"📁 {data.workspace.current_dir}"

try
    let json = Console.In.ReadToEnd()
    let data = JsonSerializer.Deserialize<ClaudeData> json
    let SEPARATOR = "\ue0c0"
    let segments : FormattableString list = [
        $"[MediumPurple1]\ue0b6[/]";
        $"[Black on MediumPurple1] {folder data} [/][MediumPurple1 on DeepSkyBlue1]{SEPARATOR}[/]"
        $"[Black on DeepSkyBlue1] {getGitBranch data} [/][DeepSkyBlue1 on MediumSpringGreen]{SEPARATOR}[/]";
        $"[Black on MediumSpringGreen] {contextUsed data} [/][MediumSpringGreen on Salmon1]{SEPARATOR}[/]"
        $"[Black on Salmon1] {rateLimit data.rate_limits.five_hour} \ue0bf {rateLimit data.rate_limits.seven_day} [/][Salmon1 on Khaki1]{SEPARATOR}[/]"
        $"[Black on Khaki1] {model data} [/]";
        $"[Khaki1]\ue0b4[/]"
    ]
    segments |> List.map AnsiConsole.MarkupInterpolated |> ignore
with
| ex -> AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything) 
