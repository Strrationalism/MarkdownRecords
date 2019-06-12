module MarkdownRecords.Generator

open System.Text
open Document

module private Generators =

    let genBase64 (builder:StringBuilder) (base64:Base64) =
        builder
            .AppendLine("```base64")
            .AppendLine(System.Convert.ToBase64String base64)
            .AppendLine("```")
            |>ignore

    let genJson (builder:StringBuilder) (json:Json) =
        builder
            .AppendLine("```json")
            .AppendLine(string json)
            .AppendLine("```")
            |> ignore

    let genImage (builder:StringBuilder) image =
        builder
            .Append('!')
            .Append('[')
            .Append(image.name)
            .Append(']')
            .Append('(')
            .Append(image.url)
            .Append(')')
            .AppendLine()
            |> ignore

    let genText (builder:StringBuilder) (text:Text) =
        builder.AppendLine text
        |> ignore

    let genTable (builder:StringBuilder) table =
        builder
            .AppendLine() 
            .Append('|') 
            |> ignore

        table.header
        |> Array.iter (fun t -> 
            builder
                .Append(' ')
                .Append(t)
                .Append(' ')
                .Append('|')
            |> ignore)

        builder.AppendLine () |> ignore

        builder.Append '|' |> ignore
        table.header
        |> Array.iter (fun t -> 
            builder
                .Append(" --- |")
            |> ignore)

        builder.AppendLine () |> ignore

        table.content
        |> List.iter (fun line ->
            builder.Append '|' |> ignore
            line
            |> Array.iter (fun cell -> 
                builder
                    .Append(' ')
                    .Append(cell)
                    .Append(' ')
                    .Append('|')
                |> ignore)
            builder.AppendLine () |> ignore)

    let rec genListItem (builder:StringBuilder) level listItem =
        for i = 0 to (level-1) do
            builder.Append "   " |> ignore
        
        match listItem.style with
        | Star -> builder.Append '*'
        | Minus -> builder.Append '-'
        | Plus -> builder.Append '+'
        | Number -> builder.Append "1."
        | Todo false -> builder.Append "- [ ]"
        | Todo true -> builder.Append "- [x]"
        |> ignore

        builder
            .Append(' ')
            .Append(listItem.text)
            .AppendLine()
            |> ignore

        listItem.children
        |> List.iter (genListItem builder (level+1))
        

    let genContent (builder:StringBuilder) =
        function
        | ListItem c -> genListItem builder 0 c
        | Table c -> genTable builder c
        | Text c -> genText builder c
        | Image c -> genImage builder c
        | Json c -> genJson builder c
        | Base64 c -> genBase64 builder c

    let rec genTitle level (builder:StringBuilder) title =
        for _ in 0u..level do
            builder.Append '#' |> ignore

        builder
            .Append(' ')
            .Append(title.text)
            .AppendLine()
            |> ignore

        title.content
        |> List.iter (genContent builder)

        title.children
        |> List.iter (genTitle (level+1u) builder)


let generate document =
    let builder =
        new StringBuilder ()

    document
    |> List.iter (Generators.genTitle 0u builder)
    
    string builder
