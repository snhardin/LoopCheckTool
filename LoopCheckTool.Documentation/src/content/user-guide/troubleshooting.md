---
title: "Troubleshooting"
date: 2020-07-02T18:06:12-04:00
draft: true
weight: 4
---

## The Values in the Generated Document Aren't There...

Yes they are! Select all text, right-click to bring up the context menu, and select **Update Field**.

## Number Formatting

Sometimes, numbers read from Excel and printed in the final generated document can look strange. Notably, they can appear with ugly scientific notation or odd precision issues. While the LoopCheckTool will attempt on its own to fix this, sometimes it requires manual intervention. The best way around this issue is to format troublesome cells as text. Cells formatted as text will be printed verbatim in the final generated document.

Please note that saving cells as text is not as simple as selecting the troublesome cells and formatting as text. Unless the cell is annotated with a warning that a number has been saved as text, the text will still continue to be printed incorrectly in the generated document.

![number-warning.png](/images/user-guide/troubleshooting.md/number-warning.png)

The best way around this issue is prepend an apostrophe (') before each troublesome cell's text. This can easily be done for several cells using Formulas and Copy + Paste as Value. Manually re-entering each cell while formatted as text will work as well, but this will take longer.

## Headers and Footers

Headers and footers in templates are not guaranteed to work with the way that LoopCheckTool stitches templates together. It's recommended to add any page numbering or other custom headers to the document after the document has been generated.

Simple headers and footers may still work, but the resulting generated report may be exceedingly large. Use with caution!

## Unexpected Field Instruction Format

When manually editing fields, Word will split the fields into several XML elements in an attempt to support "complex" fields. This is currently unsupported by LoopCheckTool and will result in errors while generating documents. To get around this, it's recommended to create dummy custom properties in templates named exactly the same as the columns in the IO List. Remove all existing troublesome fields and replace them with references to the dummy custom properties.
