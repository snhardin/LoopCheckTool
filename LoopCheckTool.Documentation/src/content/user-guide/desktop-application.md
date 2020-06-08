---
title: "Desktop Application"
date: 2020-05-31T15:19:25-04:00
draft: true
---

The desktop application, dubbed `LoopCheckTool.Wizard` is a Wizard that walks through all of the required steps to generate a LoopCheck `.docx` file.

## Layout

The layout of the desktop application follows the typical layout of many Wizard-type applications. The large panel in the center of the application changes as more steps are completed. At the bottom are five buttons to control the current step.

![layout.png](/images/user-guide/desktop-application.md/layout.png)

* The *Help* button may be used to display this User Guide. If the local copy of documentation was not installed, the online version will be pulled up instead.
* The *About* button may be used to display version information for the LoopCheckTool. This is useful for gathering information about the application when making a bug report.
* The *Back* button navigates to the previous step in the Wizard.
* The *Next* button navigates to the next step in the Wizard. It is disabled until certain conditions are met on the current step.
* The *Finish* button begins generation of the document. Any steps skipped will use default values. It is disabled until the Wizard has the minimum information required to generate the document.

## Pages

### About

The About page is used to display version information about the LoopCheckTool.

![about.png](/images/user-guide/desktop-application.md/about.png)

* *Version* - The release version as tagged in Git.
* *Assembly Version* - The assembly version of LoopCheckTool.Wizard (not LoopCheckTool.Lib). This is used for determining compatibility between different DLLs.
* *Assembly File Version* - The assembly file version of LoopCheckTool.Wizard (not LoopCheckTool.Lib). This is mostly for aesthetics and is displayed in File Explorer.
* *Git Hash* - The Git Hash that this version of LoopCheckTool was built from. It will be marked as `-dirty` if built from uncommitted changes.
* *Git Branch* - The Git Branch that this version of LoopCheckTool was built from.

**Note:** A dirty build or a build not from the master branch may be unstable. Use with caution!

### 0 - Introduction

![introduction.png](/images/user-guide/desktop-application.md/introduction.png)

This page merely introduces the LoopCheckTool.

### 1 - Input File

This page is where the Excel IO List is specified as well as the Sheet to work with.

![input-file.png](/images/user-guide/desktop-application.md/input-file.png)

* The *Browse* button is used to browse for the Excel IO List **Input File**. If an unrecognized file is specified, an error will display and the next step will be unavailable.
* The *Sheet* dropdown is used to select the **Sheet** within the Excel file to use. It populates once the **Input File** has been chosen.
  * If the Excel file specified has no sheets, an error will display and the next step will be unavailable.
  * If the Excel file has no discernable headers, an error will display and the next step will be unavailable. Headers count as cells on the first row of the sheet.

In order to progress to the next step, both **Input File** and **Sheet** must be specified and valid.

### 2 - Key Column

This page is where the Key Column is specified. This is the column that the tool uses to identify which template to use for a row.

![key-column.png](/images/user-guide/desktop-application.md/key-column.png)

### 3 - Template Folder

This page is where the folder containing all of the template files is specified.

![template-folder.png](/images/user-guide/desktop-application.md/template-folder.png)

* The *Browse* button is used to browse for the folder containing all of the templates.
* The *Modify Mappings* button is currently always disabled and is reserved for future use.

In order to progress to the next step, a **Template Folder** must be specified.

### 4 - Output File

This page is where the output file is specified.

![output-file.png](/images/user-guide/desktop-application.md/output-file.png)

* The *Browse* button is used to browse for a location to store the generated document. If the output file already exists, a prompt will displaying asking to overwrite the file.

In order to progress to the next step or to skip directly to the end, the **Output File** must be specified.

### 5 - Extra Options (optional)

This page is where any extra options are specified.

![extra-options.png](/images/user-guide/desktop-application.md/extra-options.png)

* Check *Ignore Errors* to treat some errors like warnings if they're expected. By default, this is unchecked.

### 6 - Document Generation

Once the *Finish* button has been clicked, the tool will begin to generate the document. A small dialog with a progress bar will display, showing the current operation taking place (such as the row being processed or data being written to a file).

![progress.png](/images/user-guide/desktop-application.md/progress.png)

Once the file is finished generating, a message box will display indicating the number of errors that occurred.

![complete.png](/images/user-guide/desktop-application.md/complete.png)

The resulting document will be saved along with a log file for review.

## Data Loss Prevention

### No Template Found

If a template is not found for a specific row, by default, the desktop application will halt execution. A dialog will display, prompting to halt execution or continue.

![error-dialog.png](/images/user-guide/desktop-application.md/error-dialog.png)

If these errors are expected, the `Ignore Errors` checkbox may be used to bypass this dialog automatically. The errors will still be logged to the execution log file.

### Missing Data for Field Code

Field codes that have no matching data or are left blank are treated as warnings and are logged to the execution log file.
