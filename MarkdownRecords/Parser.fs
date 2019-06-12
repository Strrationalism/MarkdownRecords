module MarkdownRecords.Parser

open System

module private Parsers =
    open Document

    let parseContent (content:string[]) : TitleContent list =
        content
        |> Array.filter (String.IsNullOrWhiteSpace >> not)
        |> function
        | [||] -> []
        | content ->
            content
            |> Array.reduce (fun a b -> a + "\n" + b)
            |> Text
            |> List.singleton

    let rec parseTitle level (contents:string[]) : Title list =
        let titleHead = 
            " "
            |> (+) (String.init (1 + int level) (fun _ -> "#"))
            

        let childTitleHead =
            " "
            |> (+) (String.init (2 + int level) (fun _ -> "#"))

        let titleLine =
            contents
            |> Array.tryFindIndex (fun x -> x.StartsWith titleHead)

        match titleLine with
        | None -> []
        | Some titleLine ->
            let content,remained = 
                let withoutHeader = Array.append [|""|] contents.[titleLine+1..]
                withoutHeader
                |> Array.tryFindIndex (fun x -> x.StartsWith titleHead)
                |> function
                | Some nextCurrentLevelTitle -> 
                    withoutHeader.[..nextCurrentLevelTitle-1]
                    ,withoutHeader.[nextCurrentLevelTitle..]
                | None -> withoutHeader,[||]

            {
                text = contents.[titleLine].[titleHead.Length..]
                content = 
                    let endingPosition = 
                        content
                        |> Array.tryFindIndex (fun x -> x.StartsWith childTitleHead)
                    match endingPosition with
                    | Some endingPosition ->
                        content.[..endingPosition]
                    | None -> content
                    |> parseContent
                    
                children = 
                    parseTitle (level+1u) content
            }
            :: parseTitle level remained
        

let parse (str:string) =
    let txt =
        str.Replace("\r","").Split '\n'
        
    Array.append txt [|""|]
    |> Parsers.parseTitle 0u
