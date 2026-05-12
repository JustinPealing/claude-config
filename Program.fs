open System
open System.Diagnostics
open System.IO
open System.Text.Json
open Spectre.Console
open Statusline.ClaudeData

let logPath = Path.Combine(Path.GetDirectoryName Environment.ProcessPath, "statusline.log")
let log (str : string) = File.WriteAllText(logPath, str)

let SEPARATOR = "\ue0b8"
let LEFT = "\ue0b6"
let RIGHT = "\ue0b4"

// Spectre.Console detection doesn't work with Claude Code
AnsiConsole.Profile.Capabilities.Ansi <- true
let json = Console.In.ReadToEnd()

let getGitBranch() =
    let info = ProcessStartInfo("git", "branch --show-current")
    info.RedirectStandardOutput <- true
    info.RedirectStandardError <- true
    let p = Process.Start(info)
    if p.WaitForExit(1000) then
        let stdout = p.StandardOutput.ReadToEnd().Trim()
        if stdout = "" then "\ue725 <none>" else $"\ue725 {stdout}"
    else
        "\ue725 <timeout>"

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

try
    let data = JsonSerializer.Deserialize<ClaudeData> json 
    let segments : FormattableString list = [
        $"[Gold1]{LEFT}[/]";
        $"[Black on Gold1] {data.model.display_name} [/][Gold1 on Plum2]{SEPARATOR}[/]"
        $"[Black on Plum2] {contextUsed data} [/][Plum2 on CornflowerBlue]{SEPARATOR}[/]";
        $"[Black on CornflowerBlue] {getGitBranch()} [/][CornflowerBlue on Lime]{SEPARATOR}[/]"
        $"[Black on Lime] {rateLimit data.rate_limits.five_hour} \ue0bf[/]"
        $"[Black on Lime] {rateLimit data.rate_limits.seven_day} [/]";
        $"[Lime]{RIGHT}[/]"
    ]
    segments |> List.map AnsiConsole.MarkupInterpolated |> ignore
with
| ex -> AnsiConsole.WriteException(ex, ExceptionFormats.NoStackTrace) 
