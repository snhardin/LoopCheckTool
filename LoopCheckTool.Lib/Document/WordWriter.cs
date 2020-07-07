using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.CustomProperties;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using OpenXmlPowerTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LoopCheckTool.Lib.Document
{
    public class WordWriter
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly List<CustomDocumentProperty> customProperties;
        private readonly List<Source> sources;

        public WordWriter()
        {
            customProperties = new List<CustomDocumentProperty>();
            sources = new List<Source>();
        }

        private (string, string, string) TransformFieldProperty(string instruction, int suffix)
        {
            string[] explosion = instruction.Split('"')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToArray();
            if (explosion.Length != 3)
            {
                return TransformFieldPropertyUsingWhitespace(instruction, suffix);
            }
            else
            {
                string oldKey = explosion[1];
                explosion[1] = explosion[1] + "_" + suffix;
                return (string.Join("\"", explosion), oldKey, explosion[1]);
            }
        }

        private (string, string, string) TransformFieldPropertyUsingWhitespace(string instruction, int suffix)
        {
            string[] explosion = instruction.Split(null)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToArray();
            if (explosion.Length < 2)
            {
                throw new DocumentWriterException("Unrecognized field instruction format");
            }
            else
            {
                if (explosion.Length != 3)
                    Logger.Warn($"Got unexpected number of tokens from instruction {instruction}. Continuing anyway.");
                string oldKey = explosion[1];
                explosion[1] = explosion[1] + "_" + suffix;
                return (string.Join(" ", explosion), oldKey, explosion[1]);
            }
        }

        private void AddCustomProperty(string name, string value)
        {
            IEnumerable<CustomDocumentProperty> customPropertyResults = customProperties.Where(c => c.Name.Value.Equals(name));
            CustomDocumentProperty customProperty = customPropertyResults.FirstOrDefault();
            if (customProperty == default(CustomDocumentProperty))
            {
                CustomDocumentProperty newProp = new CustomDocumentProperty();
                newProp.VTLPWSTR = new DocumentFormat.OpenXml.VariantTypes.VTLPWSTR(value);
                newProp.FormatId = "{D5CDD505-2E9C-101B-9397-08002B2CF9AE}";
                newProp.Name = name;
                customProperties.Add(newProp);
            }

            if (customPropertyResults.Count() > 1)
            {
                Logger.Warn($"A custom property search for the name {name} turned up more than one result.");
            }
        }

        public void GenerateAndAppendTemplate(MemoryStream template, IDictionary<string, string> values, int idx)
        {
            try
            {
                Logger.Info($"Filling template for row {idx}.");
                using (WordprocessingDocument templateDoc = WordprocessingDocument.Open(template, true))
                {
                    OpenXmlPartRootElement root = templateDoc.MainDocumentPart.RootElement;

                    // TODO: Set text here as well so Update Fields doesn't need to be run.
                    foreach (SimpleField field in root.Descendants<SimpleField>())
                    {
                        Logger.Info($"Handling SimpleField of instruction: {field.Instruction.Value}.");
                        var (newInstruction, oldKey, newPropName) = TransformFieldProperty(field.Instruction.Value, idx);
                        field.Instruction.Value = newInstruction;

                        string value = null;
                        if (!values.TryGetValue(oldKey, out value))
                        {
                            throw new DocumentWriterException($"No key found for {oldKey}");
                        }

                        if (string.IsNullOrWhiteSpace(value))
                        {
                            Logger.Warn($"An entry exists for key {oldKey}, but the resulting value is blank");
                        }

                        AddCustomProperty(newPropName, value);
                    }

                    // TODO: Make more robust... FieldCodes are far more complex than this.
                    // See the ECMA standard for 2.16.18.
                    foreach (FieldCode field in root.Descendants<FieldCode>())
                    {
                        Logger.Info($"Handling FieldCode of text: {field.Text}.");
                        var (newInstruction, oldKey, newPropName) = TransformFieldProperty(field.Text, idx);
                        field.Text = newInstruction;

                        if (!values.TryGetValue(oldKey, out string value))
                        {
                            throw new DocumentWriterException($"No key found for {oldKey}");
                        }

                        if (string.IsNullOrWhiteSpace(value))
                        {
                            Logger.Warn($"An entry exists for key {oldKey}, but the resulting value is blank.");
                        }

                        AddCustomProperty(newPropName, value);
                    }
                }

                sources.Add(new Source(new WmlDocument(template.Length.ToString(), template), true));
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "An error occurred while filling template.");
                throw ex;
            }
        }

        public MemoryStream ExportDocument()
        {
            try
            {
                WmlDocument doc = DocumentBuilder.BuildDocument(sources);
                MemoryStream stream = new MemoryStream();

                doc.WriteByteArray(stream);
                BuildCustomProperties(stream);

                return stream;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "An error occurred while exporting document.");
                throw ex;
            }
        }

        private void BuildCustomProperties(MemoryStream documentData)
        {
            Logger.Info("Building custom properties section for newly-generated document.");
            using (WordprocessingDocument document = WordprocessingDocument.Open(documentData, true))
            {
                CustomFilePropertiesPart customPropertiesPart = document.CustomFilePropertiesPart;

                // Add custom properties part if there is none yet.
                if (customPropertiesPart == null)
                {
                    customPropertiesPart = document.AddCustomFilePropertiesPart();
                    customPropertiesPart.Properties = new Properties();
                }

                // Remove all existing custom properties first.
                Properties customProps = customPropertiesPart.Properties;
                customProps.RemoveAllChildren<CustomDocumentProperty>();

                // Stick all of our generated custom properties in the new document.
                foreach (CustomDocumentProperty customProp in customProperties)
                {
                    customProps.AppendChild(customProp);
                }

                // Re-number the custom properties (advice given by MSDN).
                int pid = 2;
                foreach (CustomDocumentProperty customProp in customProps)
                {
                    customProp.PropertyId = pid++;
                }
            }
        }
    }
}
