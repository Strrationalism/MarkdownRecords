module MarkdownRecords.Parser

open System

module private Parsers =
    open Document
    open Builder

    type ContentParser = string[] -> (TitleContent * string[]) option

    let parseTableListItem : ContentParser = (fun x -> None)

    let parseTable : ContentParser = (fun input ->
        let popLine (line:string) : string[] option =
            try
                let trimed = line.Trim()
                if trimed.StartsWith "|" && trimed.EndsWith "|" then
                    trimed.[1..trimed.Length-1].Split '|'
                    |> Array.map (fun x -> x.Trim())
                    |> Some
                else None
            with _ -> None

        let rec popContent (content:string[]) : string[] list *string[] =
            if Array.isEmpty content then [],content
            else
                match content |> Array.head |> popLine with
                | Some cells -> 
                     let nextList,remainder = 
                        popContent (Array.tail content)
                     cells::nextList,remainder
                | None -> [],content
        
        match Array.tryHead input with
        | None -> None
        | Some head ->
            match popLine head with
            | None -> None
            | Some header ->
                match Array.tryHead (Array.tail input) with
                | None -> None
                | Some _ ->
                    match  input |> Array.tail |> Array.tail with
                    | [||] -> None
                    | remainder ->
                        let content,afterTable = popContent remainder
                        ({
                            header = header
                            content = content
                        } |> Table,afterTable) |> Some)
                        
    let parseCode (languageId:string) (processor:string -> TitleContent) (input:string[]) =
        input
        |> Array.tryHead
        |> function
        | Some head when head.StartsWith ("```"+languageId) ->
            let tail =
                Array.tail input
            let endingPos =
                input
                |> Array.tryFindIndex (fun x -> x.Trim() = "```")

            match endingPos with
            | None -> None
            | Some endingPos ->
                let remainder = input.[endingPos..] |> Array.tail
                let body = 
                    input.[..endingPos-1]
                    |> Array.tail
                    |> Array.reduce (fun a b -> a + "\n" + b)
                    |> processor
                (body,remainder)
                |> Some
                    
        | _ -> None

    let parseImage : ContentParser = (fun input ->
        input
        |> Array.tryHead
        |> function
        | Some head when head.StartsWith "![" && head.EndsWith ")" ->
            let pos1 = head.IndexOf ']'
            let pos2 = head.IndexOf '('

            if pos1 = -1 || pos2 = -1 then None
            else Some(Image (image head.[2..pos1] head.[pos2+1..head.Length-1]) ,Array.tail input)
        | _ -> None)

    let parseText : ContentParser = (fun input ->
        Some(Array.head input |> Text, Array.tail input))

    let contentParsers = [
        parseTableListItem
        parseTable
        parseCode "json" (Json.Parse >> Json)
        parseCode "base64" (Convert.FromBase64String >> Base64)
        parseImage
        parseText
    ]

    let rec parseContent (content:string[]) : TitleContent list =
        content
        |> Array.filter (String.IsNullOrWhiteSpace >> not)
        |> function
        | [||] -> []
        | content ->
            contentParsers
            |> List.fold (fun state c ->
                match state with
                | Some _ -> state
                | None -> c content)
                None
            |> Option.get
            |> (fun (a,b) ->
                a :: parseContent b)


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
                    |> List.filter ((<>) (Text ""))
                    
                children = 
                    parseTitle (level+1u) content
            }
            :: parseTitle level remained
        

let parse (str:string) =
    let txt =
        str.Replace("\r","").Split '\n'
        
    Array.append txt [|""|]
    |> Parsers.parseTitle 0u
