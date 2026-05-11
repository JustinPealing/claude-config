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
    try 
        let info = ProcessStartInfo("git", "branch --show-current")
        info.RedirectStandardOutput <- true
        info.RedirectStandardError <- true
        let p = Process.Start(info)
        if p.WaitForExit(100) then
            Some (p.StandardOutput.ReadToEnd().Trim())
        else None
    with
    | ex ->
        AnsiConsole.WriteException ex
        None

try
    let data = JsonSerializer.Deserialize<ClaudeData> json
    
    AnsiConsole.MarkupInterpolated $"[Gold1]{LEFT}[/]"
    
    let usedPercentage =
        match data.context_window.used_percentage with
        | Some usedPercentage -> usedPercentage
        | None -> 0
    
    let branch = getGitBranch()
    
    AnsiConsole.MarkupInterpolated $"[Black on Gold1] ctx: {usedPercentage}%% [/]"
    AnsiConsole.MarkupInterpolated $"[Gold1 on CornflowerBlue]{SEPARATOR}[/]"
    AnsiConsole.MarkupInterpolated $"[Black on CornflowerBlue] \ue725 {if branch.IsSome then branch.Value else null} [/]"
    AnsiConsole.MarkupInterpolated $"[CornflowerBlue]{RIGHT}[/]"
with
| ex -> AnsiConsole.WriteException ex
