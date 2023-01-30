module Command

open Spectre.Console
open Spectre.Console.Cli
open System.IO
open System.ComponentModel
open FSharpAux

/// Checks whether a given file is an XLSX file.
let isXlsx path = (FileInfo path).Extension = ".xlsx"

/// Prints a message to the console that the given path corresponds to no XLSX file.
let printNotSatisfying path = printfn $"Path to an inproper file given. {path} is no XLSX file"

type FileAge =
    | Old
    | New

let matchFa fileAge =
    match fileAge with
    | Old -> "old"
    | New -> "new"

/// Asks the user to give the path to the an old or new (depending on fileAge) XLSX file to compare.
let rec getPath fileAge =
    let fa = matchFa fileAge
    printfn $"Please give the path to the {fa} XLSX file:"
    let input = 
        System.Console.ReadLine()
        |> String.replace "\"" ""
    if isXlsx input |> not then
        printNotSatisfying input
        getPath fileAge
    else input


type Settings() =

    inherit CommandSettings()

    // multiple option types as CommandArguments do not work due to "Error: TypeConverter cannot convert from System.String."
    //let mutable oldXlsxPath : string option = None 
    //let mutable newXlsxPath : string option = None
    let mutable oldXlsxPath = ""
    let mutable newXlsxPath = ""

    [<CommandArgument(0, "[old XLSX]")>]
    [<Description("Path to the old XLSX file.")>]
    member this.OldXlsxPath with get() = oldXlsxPath
    member this.OldXlsxPath with set(value) = oldXlsxPath <- value
    [<CommandArgument(1, "[new XLSX]")>]
    [<Description("Path to the new XLSX file.")>]
    member this.NewXlsxPath with get() = newXlsxPath
    member this.NewXlsxPath with set(value) = newXlsxPath <- value


/// Takes a FileAge and a Settings and returns the path the the approbriate XLSX file.
let returnPath fileAge (settings : Settings) =
    let settingsFile = 
        match fileAge with
        | Old -> settings.OldXlsxPath
        | New -> settings.NewXlsxPath
    // multiple option types as CommandArguments do not work due to "Error: TypeConverter cannot convert from System.String."
    //let input = Option.defaultValue (getPath fileAge) settingsFile
    let input =
        match settingsFile with
        | ""    -> getPath fileAge
        | _     -> settingsFile
    if isXlsx input |> not then
        printNotSatisfying input
        getPath fileAge
    else input


type FileSizeCommand() =

    inherit Command<Settings>()

    override this.Execute (context : CommandContext, settings : Settings) =

        let oxp = returnPath Old settings
        let nxp = returnPath New settings

        CellifyDiff.parse oxp nxp

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