namespace MarkdownRecordsTest

open System
open Microsoft.VisualStudio.TestTools.UnitTesting

 open MarkdownRecords.Builder
 open MarkdownRecords.Document

[<TestClass>]
type Test () =

    [<TestMethod>]
    member this.TestGenerate () =
        let doc =
            let base64 = 
                "I am fucking your ass."
                |> System.Text.Encoding.Default.GetBytes

            let text = "母猪的产后护理是一门高深的学问。"
            let img = image "母猪" "https://ss2.baidu.com/6ONYsjip0QIZ8tyhnq/it/u=2010853061,277482689&fm=85&app=61&f=JPEG?w=121&h=75&s=F804D91845C2ECE85AE11CC80300A0B1"

            let table = {
                header = [|"母猪";"状态"|]
                content = [
                    [|"小明";"产后护理"|]
                    [|"小红";"产后护理"|]
                    [|"小白";"螺旋升天"|]
                ]
            }

            let title1 =
                let title1_1 =
                    let title1_1_1 =
                        let item =
                            listItem Minus "Item 1" 
                                ([
                                    "Item 1.1"
                                    "Item 1.2"
                                    "Item 1.3"]
                                |> list (Todo false))
                        title "1.1.1" [Base64 base64;Text text;Image img;ListItem item;ListItem (listItem Minus "Item 2" []);Table table] []
                    let title1_1_2 =
                        title "1.1.2" [] []
                    title "1.1" [] [title1_1_1;title1_1_2]
                let title1_2 =
                    title "1.2" [] []
                title "1" [] [title1_1;title1_2]
            let title2 =
                title "2" [] []
            [title1;title2]

        let strDoc =
            MarkdownRecords.Generator.generate doc

        printfn "== Orginal AST =="
        printfn "%A" doc
            
        printfn "== Parsed AST =="
        let parsed = 
            strDoc
            |> MarkdownRecords.Parser.parse
        parsed
        |> printfn "%A"

        printfn "== Generated =="
        printfn "%s" strDoc
           
        printfn "== Regenerated =="
        let regenerated =
            parsed
            |> MarkdownRecords.Generator.generate
        regenerated
        |> printfn "%s"

        let c = parsed = doc
        let c2 = regenerated = strDoc
        Assert.IsTrue c
        Assert.IsTrue c2

