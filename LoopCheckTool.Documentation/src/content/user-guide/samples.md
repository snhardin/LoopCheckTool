---
title: "Samples"
date: 2020-07-05T21:38:52-04:00
draft: true
weight: 3
---

The Sample Project is provided to demonstrate proper usage of the LoopCheckTool.

Download and unpack the [Sample Project](/projects/sample.zip). The expected generated output is included in the `output/` folder in the Sample Project.

## Wizard Instructions

Start by opening the LoopCheckTool.Wizard application and continuing to the step where it prompts for a spreadsheet.

Browse to the `IO List.xlsx` file and open it. Select the "Data" sheet. Click **Next**.

![choose-io-list.png](/images/user-guide/samples.md/choose-io-list.png)

Select "IO Type" in the dropdown to be the IO Type column.

![select-io-type.png](/images/user-guide/samples.md/select-io-type.png)

Browse to the `templates/` folder and select it. Click **Next**.

![choose-templates.png](/images/user-guide/samples.md/choose-templates.png)

Browse to the location to save the generated document. This can be any valid path desired. Click **Finish**.

![choose-output.png](/images/user-guide/samples.md/choose-output.png)

## Command Line Instructions

Figure out the path that the LoopCheckTool was installed to. Typically, this is `C:\Program Files (x86)\LoopCheckTool\`.

Open a new Command Line or PowerShell terminal. Navigate to the directory that the Sample Project was unpacked to.

```shell
> cd "C:\path\to\extracted\sample\"
```

Run the command to generate a document.

```shell
> "C:\Program Files (x86)\LoopCheckTool\LoopCheckTool.Console.exe" generate -i "IO List.xlsx" -s Southeast
```

## Post-Generation Steps

Examine the newly-generated document. Select everything in the document (CTRL + A) and then right-click and select **Update Field**. Compare it to the included document in the `output/` folder.

Examine the fields of the document. Start making your own changes to the templates and data and regenerate the document!
