---
title: "Desktop Application"
date: 2020-05-31T15:19:25-04:00
draft: true
---

The desktop application, dubbed `LoopCheckTool.Wizard` is a Wizard that walks through all of the required steps to generate a LoopCheck `.docx` file.

## Pages

## Data Loss Prevention

### No Template Found

If a template is not found for a specific row, by default, the desktop application will halt execution. A dialog will display, prompting to halt execution or continue.

![error-dialog.png](/images/user-guide/desktop-application.md/error-dialog.png)

If these errors are expected, the `Ignore Errors` checkbox may be used to bypass this dialog automatically. The errors will still be logged to the execution log file.

### Missing Data for Field Code

Field codes that have no matching data or are left blank are treated as warning and are logged to the execution log file.
