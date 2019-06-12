module MarkdownRecords.Document

open FSharp.Data

type Json = JsonValue

type Base64 = byte[]

type Image = {
    name : string
    url : string
}

type Text = string

type Table = {
    header : string []
    content : string [] list
}

type ListItemStyle =
| Star
| Minus
| Plus
| Number
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
