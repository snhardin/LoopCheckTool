---
title: "User Guide"
date: 2020-05-31T14:38:44-04:00
draft: true
---

## General

Both the CLI application and the Desktop application have equivalent functionality and use the same approach to generating a Loop Check document.

### Prerequisites

Before using, the tool expects the user to have the following set up:

* A `.docx` file for each IO Type to support in the IO List. These `.docx` files should be in a single folder isolated from the other files and should be named the exact same name as the IO Type that it represents.
* An IO List in Excel format.

### Templating Guidelines

As stated above, a template should be made for each IO Type in the IO List. For now, each template must be named exactly the name of the IO Type that it represents (a feature for mapping types to certain filenames is planned). Please see the **Template Mapping Concept** section below for examples.

To pull information from the IO List into the template, `DOCPROPERTY` fields that are named exactly after the Columns that they're supposed to pull from in the IO List must be used. For example, if a field in the template is named **AlarmHi** then it will pull from the **AlarmHi** column in the IO List. Fields that are used so that 100% of the formatting is retained from the template.

* **Note:** It's recommended to show Field Codes in Microsoft Word while creating templates for usage with the LoopCheckTool.

#### Template Mapping Concept

The tool determines what template to use based on the configured key column. The tool will pick the appropriately named template baed on the value of the key column in the IO List. So, for example, if the **IO Type** column is selected as the key column in the LoopCheckTool and a tag in the IO List Excel file has an IO Type of **AO** then the tool will select the `AO.docx` template from the configured Template Directory.

**Full Example:**

Suppose a snippet of an Excel IO List looks like so:

| **Tag Name**                           | **IO Type** | **Description**       | **...** |
| -------------------------------------- | ----------- | --------------------- | ------- |
| AMER.SOUTHEAST.RDU.MACHINE_01.ATTR_01  | AI          | Machine 1 Attribute 1 | ...     |
| AMER.SOUTHEAST.RDU.MACHINE_01.ATTR_02  | AI          | Machine 1 Attribute 2 | ...     |
| AMER.SOUTHEAST.RDU.MACHINE_01.INPUT_01 | DO          | Machine 1 Input 1     | ...     |
| ...                                    | ...         | ...                   | ...     |

If the key column is set to **IO Type** and the user proceeds to generate a document, the tool will use `AI.docx` for the `AMER.SOUTHEAST.RDU.MACHINE_01.ATTR_01` and `AMER.SOUTHEAST.RDU.MACHINE_01.ATTR_02` tags. Then it will use `DO.docx` for the `AMER.SOUTHEAST.RDU.MACHINE_01.INPUT_01` tag.

## Data Loss Prevention

Potential data loss can occur in the following scenarios:

* An IO Type template is not found for a specific row.
* The template expects data that is not present in the row being processed.
* The row has extraneous data that is not utilized in the template.

To prevent accidental omission of data, both LoopCheckTools implement handlers for the first two scenarios. Optional logging is planned for the third scenario.