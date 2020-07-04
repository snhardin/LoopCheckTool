---
title: "Console Application"
date: 2020-07-04T01:05:29-04:00
draft: true
---

The command line application, dubbed `LoopCheckTool.Console`, is a utility for generating LoopCheck documents from the command line.

## Verbs

"Verbs" are used to specify the operation for the LoopCheck command line application to perform. Each operation has its own flags and options that can be specified.

```shell
> LoopCheckTool.Console.exe <verb> <options and flags>
```

### list-sheets

The "list-sheets" verb lists all sheets accessible in the Spreadsheet. This operation is particularly useful if using the tool in an environment without Office installed.

**Usage:**

```shell
> LoopCheckTool.Console.exe list-sheets "IO List.xlsx"
```

### generate

### help

The "help" verb is a generated help text for using the tool. It contains usage examples as well as small descriptions for each flag and verb. In-depth help can always be looked up in this User Guide.

**Usage:**

```shell
> LoopCheckTool.Console.exe help

LoopCheckTool.Console 1.0.0
Copyright (c) Scott Hardin 2020

  list-sheets    Lists all sheets available in the given Spreadsheet.

  generate       Generates a Document from the provided Spreadsheet.
...
```

### version

The "version" verb displays generated version information for the tool. This is useful to include when making bug reports.

**Usage:**

```shell
> LoopCheckTool.Console.exe version

        Tagged Version: 1.0.0
      Assembly Version: 0.0.1.0
 Assembly File Version: 0.0.1.0
              Git Hash: 663e06e
            Git Branch: master
```
