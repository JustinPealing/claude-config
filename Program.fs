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
            let stdout = p.StandardOutput.ReadToEnd().Trim()
            if stdout = "" then "<none>" else stdout
        else
            "<timeout>"
    with
    | ex -> "<error>"

try
    let data = JsonSerializer.Deserialize<ClaudeData> json
    
    let usedPercentage =
        match data.context_window.used_percentage with
        | Some usedPercentage -> usedPercentage
        | None -> 0
    
    let branch = getGitBranch()
    
    let segments = [
        $"[Gold1]{LEFT}[/]";
        $"[Black on Gold1] ctx: {usedPercentage}%% [/]";
        $"[Gold1 on CornflowerBlue]{SEPARATOR}[/]";
        $"[Black on CornflowerBlue] \ue725 {branch} [/]";
        $"[CornflowerBlue]{RIGHT}[/]"
    ]
    
    AnsiConsole.Markup (segments |> String.concat "")
with
| ex -> AnsiConsole.WriteException(ex, ExceptionFormats.NoStackTrace) 
