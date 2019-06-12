module MarkdownRecords.Document

open FSharp.Data

type Json = {
    content : JsonValue
}

type Base64 = {
    content : byte[]
}

type Image = 
| Base64Image of byte[]
| URLImage of string

type Text = {
    content : string
}

type Table = {
    header : string []
    content : string [][]
}

type ListItemStyle =
| Star
| Minus
| Plus
| Number of int
| Todo of bool

type ListItem = {
    style : ListItemStyle
    text : string
    children : ListItem list
}

type TitleContent =
| ListItem of ListItem
| Table of Table
| Text of Text
| Image of Image
| Json of Json
| Base64 of Base64

type Title = {
    text : string
    content : TitleContent list
    children : Title list
}

type Document = Title list
