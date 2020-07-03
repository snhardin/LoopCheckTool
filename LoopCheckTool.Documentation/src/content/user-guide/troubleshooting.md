---
title: "Troubleshooting"
date: 2020-07-02T18:06:12-04:00
draft: true
weight: 20
---

## Number Formatting

Sometimes, numbers read from Excel and printed in the final generated document can look strange. Notably, they can appear with ugly scientific notation or odd precision issues. While the LoopCheckTool will attempt on its own to fix this, sometimes it requires manual intervention. The best way around this issue is to format troublesome cells as text. Cells formatted as text will be printed verbatim in the final generated document.

Please note that saving cells as text is not as simple as selecting the troublesome cells and formatting as text. Unless the cell is annotated with a warning that a number has been saved as text, the text will still continue to be printed incorrectly in the generated document.

![number-warning.png](/images/user-guide/troubleshooting.md/number-warning.png)

The best way around this issue is prepend an apostrophe (') before each troublesome cell's text. This can easily be done for several cells using Formulas and Copy + Paste as Value. Manually re-entering each cell while formatted as text will work as well, but this will take longer.