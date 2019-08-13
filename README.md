# MarkdownRecords
对于人类和程序来说都简单易读的Markdown子集。

[![Build status](https://ci.appveyor.com/api/projects/status/mbd34nbtwyuuf1bj?svg=true)](https://ci.appveyor.com/project/SmallLuma/markdownrecords)


## 文档结构

文档为树状结构，其中根节点为Document节点。

## Document
一个Document节点对应一个MarkdownRecords文档。
可以包含的子节点：
- Title


## Title
对应Markdown的标题语法。
属性：
- Text （单行文本，表示标题内容）

可以包含的子节点：
- ListItem
- Table
- Text
- Image
- Base64
- Title （必须在最后）

## ListItem
对应Markdown的列表行语法。
属性：
- Text （单行文本，表示列表行内容文本）
- Style（对应 - * + 三种无序列表语法，一种有序列表语法，两种待办事项语法）
   - Star （使用*号的列表语法）
   - Minus （使用-号的列表语法）
   - Plus （使用+号的列表语法）
   - Number （有序列表语法）
   - Finished （已完成待办事项）
   - NotFinished （未完成待办事项）

可以包含的子节点：
- ListItem（使用三个空格为深度）

## Table
对应Markdown的表格语法。
仅使用完整的表格语法。
属性：
- Header （单行文本列表，表示列表头部标签）
- Records （单行文本二维表，表示每行的数据）

## Text
普通文本。
属性：
- Text （文本内容）

## Image
单行图片，这种图片必须写在单独的一行内，支持基于URL的图片和BASE64内联的图片。
属性：
- Type
   - Base64
   - URL
- Content

## Base64
内联Base64。


