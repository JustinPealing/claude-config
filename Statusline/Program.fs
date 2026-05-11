open System
open System.IO
open System.Text.Json
open Spectre.Console
open Statusline.ClaudeData

let logPath = Path.Combine(Path.GetDirectoryName Environment.ProcessPath, "statusline.log")
let log (str : string) = File.WriteAllText(logPath, str)

[<Literal>]
let BRANCH = "\uE0A0"

// Spectre.Console detection doesn't work with Claude Code
AnsiConsole.Profile.Capabilities.Ansi <- true
let json = Console.In.ReadToEnd()

try
    let data = JsonSerializer.Deserialize<ClaudeData> json
    AnsiConsole.MarkupInterpolated $"[Black on Gold1] ctx: {data.context_window.used_percentage}%% [/]"
    AnsiConsole.MarkupInterpolated $"[Black on CornflowerBlue] \uE0A0 Test Branch [/]"
with
| ex ->
    log json
    log (ex.ToString())
    AnsiConsole.WriteException ex
