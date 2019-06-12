module MarkdownRecords.Builder

open Document

let image name url =
    {
        name = name
        url = url
    }

let listItem style text children = 
    {
        style = style
        text = text
        children = children
    }
   
let list style items =
    items
    |> List.map (fun x -> listItem style x [])

let title text content children = 
    {  
        text = text
        content = content
        children = children
    }


