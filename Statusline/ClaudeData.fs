module Statusline.ClaudeData

// https://code.claude.com/docs/en/statusline#available-data

type RateLimit = {
    used_percentage: int
    resets_at: int
}

type ClaudeData = {
    cwd: string
    effort: {|
        level: string
    |}
    model: {|
        id: string
        display_name: string
    |}
    workspace: {|
        current_dir: string
        project_dir: string
        added_dirs: string list
    |}
    context_window: {|
        total_input_tokens: int
        total_output_tokens: int
        context_window_size: int
        used_percentage: int
        remaining_percentage: int
    |}
    rate_limits: {|
        five_hour: RateLimit
        seven_day: RateLimit
    |}
}
