module Command

open Spectre.Console.Cli

let args = [|"loL"|]

[<AbstractClass; Sealed>]
type FileSizeCommandSettings =
    inherit CommandSettings
    

[<AbstractClass; Sealed>]
type FileSizeCommand =
    inherit Command<FileSizeCommandSettings>

let app = CommandApp<FileSizeCommand>()
app.Run(args)