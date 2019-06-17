module MarkdownRecords.Parser

open System

module private Parsers =
    open Document
    open Builder

    type ContentParser = string[] -> (TitleContent * string[]) option

    let parseListItem input =
        let rec parse level (input:string[]) : (ListItem * string[]) option =
            let headerSpaces =
                String.init level (fun _ -> "   ")
            match Array.tryHead input with
            | Some head when head.StartsWith headerSpaces ->
                let trimed = head.TrimStart()

                try
                    let style,text =
                        match trimed with
                        | x when x.StartsWith "- [ ]" -> Some (Todo false),x.[5..]
                        | x when x.StartsWith "- [x]" -> Some (Todo true),x.[5..]
                        | x when x.StartsWith "*" -> Some Star, x.[1..]
                        | x when x.StartsWith "-" -> Some Minus, x.[1..]
                        | x when x.StartsWith "+" -> Some Plus, x.[1..]
                        | x when x.[..x.IndexOf '.'].ToCharArray() |> Array.forall (fun c -> c >= '0' && c <= '9') -> Some Number,x.[1+x.IndexOf '.'..]
                        | _ -> None,""
                    match style with
                    | None -> None
                    | Some style ->
                        let children,remainder = parseSome (level+1) (Array.tail input)
                        let item = {
                            text = text.Trim()
                            style = style
                            children = children
                        }
                        (item,remainder) |> Some
                with _ -> None
            | _ -> None
        and parseSome level (input:string[]) : ListItem list * string[] =
            match parse level input with
            | Some (item,remainders) ->
                let items,finalRemainders = parseSome level remainders
                item :: items, finalRemainders
            | None -> [],input

        parse 0 input
        |> Option.map (fun (a,b) -> ListItem a,b)

    let parseTable : ContentParser = (fun input ->
        let popLine (line:string) : string[] option =
            try
                let trimed = line.Trim()
                if trimed.StartsWith "|" && trimed.EndsWith "|" then
                    trimed.[1..trimed.Length-2].Split '|'
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
                | Some r ->
                    match  input |> Array.tail |> Array.tail with
                    | [||] -> Some({ header = header;content = [] } |> Table,input |> Array.tail |> Array.tail)
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
            else Some(Image (image head.[2..pos1-1] head.[pos2+1..head.Length-2]) ,Array.tail input)
        | _ -> None)

    let parseText : ContentParser = (fun input ->
        Some(Array.head input |> Text, Array.tail input))

    let contentParsers = [
        parseListItem
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
                        content.[..endingPosition-1]
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
