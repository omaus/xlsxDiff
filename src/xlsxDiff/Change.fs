module Change

open XlsxCell
open System.Collections.Generic

/// The type of change occuring at a given ChangeSite.
type ChangeType =
/// A content addition to an empty cell.
| Add
/// The deletion of all content of a cell.
| Del
/// A modification of an existing cell.
| Mod
/// No change.
| No

/// The part of a cell where a change occurs.
type ChangeSite =
/// A change to the written content of a cell.
| Content
/// A change to the formatting of the text content of a cell.
| TextFormat
/// A change to the cell format.
| CellFormat
/// A change to a comment (and associated replies) attached to a cell.
| Comment
/// A change to a note attached to a cell.
| Note
/// A change to the formula that creates the content of a cell.
| Formula

type CellChange = {
    CellInformation : XlsxCell * XlsxCell
    Changes         : seq<ChangeSite * ChangeType>
}



/// Takes two cells and compares if they differ.
let hasChange (cell1 : XlsxCell) cell2 = cell1 <> cell2

/// Takes two cells and gets the changes between them.
let getCellChange (cell1 : XlsxCell) (cell2 : XlsxCell) =
    let getChangeType (cell1field : 'a option) (cell2field : 'a option) =
        match (cell1field,cell2field) with
        | (c1,c2) when c1.IsNone && c2.IsSome                       -> Add
        | (c1,c2) when c1.IsSome && c2.IsNone                       -> Del
        | (c1,c2) when c1.IsSome && c2.IsSome && Some c1 <> Some c2 -> Mod
        | _                                                         -> No
    let hasContentChange cell1 cell2 = getChangeType cell1.Content cell2.Content
    let hasCommentChange cell1 cell2 = getChangeType cell1.Comment cell2.Comment
    let hasCellFormatChange cell1 cell2 = getChangeType cell1.CellFormat cell2.CellFormat
    let hasFormulaChange cell1 cell2 = getChangeType cell1.Formula cell2.Formula
    let hasNoteChange cell1 cell2 = getChangeType cell1.Note cell2.Note
    let hasTextFormatChange cell1 cell2 = getChangeType cell1.TextFormat cell2.TextFormat
    let matchCs cs =
        match cs with
        | CellFormat    -> hasCellFormatChange
        | Comment       -> hasCommentChange
        | Content       -> hasContentChange
        | Formula       -> hasFormulaChange
        | Note          -> hasNoteChange
        | TextFormat    -> hasTextFormatChange
    let noChanges = [CellFormat, No; Comment, No; Content, No; Formula, No; Note, No; TextFormat, No]
    let change =
        if hasChange cell1 cell2 then noChanges |> List.map (fun (cs,ct) -> cs, (matchCs cs) cell1 cell2)
        else noChanges
    {
        CellInformation = cell1, cell2
        Changes         = change
    }

/// Takes two Cell matrices (sparse value dictionaries) and returns a 2D array of CellChanges. The row and column index of the 2D array match the row and column index of the matrices (but are zero-based instead of one-based).
let getChangeMatrix (cellMatrix1 : Dictionary<int * int, XlsxCell>) (cellMatrix2 : Dictionary<int * int, XlsxCell>) =
    let emptyCell = {
        Content     = None
        TextFormat  = None
        CellFormat  = None
        Comment     = None
        Note        = None
        Formula     = None
    }
    let keys = Seq.append cellMatrix1.Keys cellMatrix2.Keys
    let noOfRows = keys |> Seq.map fst |> Seq.max
    let noOfCols = keys |> Seq.map snd |> Seq.max
    Array2D.init noOfRows noOfCols (
        fun iR iC ->
            let currkey = (iR + 1, iC + 1)
            match cellMatrix1.ContainsKey(currkey), cellMatrix2.ContainsKey(currkey) with
            | (true ,true ) -> getCellChange cellMatrix1.[currkey]  cellMatrix2.[currkey]
            | (true ,false) -> getCellChange cellMatrix1.[currkey]  emptyCell
            | (false,true ) -> getCellChange emptyCell              cellMatrix2.[currkey]
            | (false,false) -> getCellChange emptyCell              emptyCell
    )