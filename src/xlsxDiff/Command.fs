﻿module Command

open Spectre.Console
open Spectre.Console.Cli
open System.IO
open System.ComponentModel


type Settings() =


    inherit CommandSettings()

    let mutable directMode = false
    let mutable oldXlsxPath : string option = None
    let mutable newXlsxPath : string option = None

    [<CommandArgument(0, "[old XLSX]")>]
    [<Description("Path to the old XLSX file.")>]
    member this.OldXlsxPath with get() = oldXlsxPath
    member this.OldXlsxPath with set(value) = oldXlsxPath <- value
    [<CommandArgument(1, "[new XLSX]")>]
    [<Description("Path to the new XLSX file.")>]
    member this.NewXlsxPath with get() = newXlsxPath
    member this.NewXlsxPath with set(value) = newXlsxPath <- value
    [<CommandOption("-p|--pattern")>]
    member this.SearchPattern with get() = searchPattern
    member this.SearchPattern with set(value) = searchPattern <- value
    [<CommandOption("--hidden")>]
    member this.IncludeHidden with get() = includeHidden
    member this.IncludeHidden with set(value) = includeHidden <- value

type FileSizeCommand() =

    inherit Command<Settings>()

    override this.Execute (context : CommandContext, settings : Settings) =
        let searchOptions = new EnumerationOptions()
        searchOptions.AttributesToSkip <- 
            if settings.IncludeHidden then 
                FileAttributes.Hidden ||| FileAttributes.System
            else
                FileAttributes.System

        let searchPattern = Option.defaultValue "*.*" settings.SearchPattern
        let searchPath = Option.defaultValue (Directory.GetCurrentDirectory()) settings.SearchPath
        let files = DirectoryInfo(searchPath).GetFiles(searchPattern, searchOptions)

        let totalFileSize = files |> Seq.sumBy (fun fileInfo -> fileInfo.Length)

        AnsiConsole.MarkupLine(sprintf "Total file size for [green]%s[/] files in [green]%s[/]: [blue]%i[/] bytes" searchPattern searchPath totalFileSize)
        0

let app = CommandApp<FileSizeCommand>()

//[<AbstractClass; Sealed>]
//type FileSizeCommandSettings =
//    inherit CommandSettings
//    [Description("Path to search. Defaults to current directory.")]
//    member SearchPath with 
    

//[<AbstractClass; Sealed>]
//type FileSizeCommand =
//    inherit Command<FileSizeCommandSettings>

//let app = CommandApp<FileSizeCommand>()
//app.Run(args)