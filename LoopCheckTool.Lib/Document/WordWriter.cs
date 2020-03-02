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
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private List<CustomDocumentProperty> customProperties;
        private List<Source> sources;

        public WordWriter()
        {
            customProperties = new List<CustomDocumentProperty>();
            sources = new List<Source>();
        }

        private (string, string, string) TransformFieldProperty(string instruction, int suffix)
        {
            string[] explosion = instruction.Split('"');
            if (explosion.Length != 3)
            {
                throw new WordWriterException("Unrecognized field instruction format.");
            }
            else
            {
                string oldKey = explosion[1];
                explosion[1] = explosion[1] + "_" + suffix;
                return (string.Join("\"", explosion), oldKey, explosion[1]);
            }
        }

        private void AddCustomProperty(string name, string value)
        {
            var customPropertyResults = customProperties.Where(c => c.Name.Value.Equals(name));
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
                logger.Warn($"A custom property search for the name {name} turned up more than one result.");
            }
        }

        public void FillTemplate_Safe(MemoryStream template, IDictionary<string, string> values, int idx)
        {
            logger.Info($"Filling template for row {idx}.");
            using (WordprocessingDocument templateDoc = WordprocessingDocument.Open(template, true))
            {
                OpenXmlPartRootElement root = templateDoc.MainDocumentPart.RootElement;

                foreach (SimpleField field in root.Descendants<SimpleField>())
                {
                    logger.Info($"Handling SimpleField of instruction: {field.Instruction.Value}.");
                    var (newInstruction, oldKey, newPropName) = TransformFieldProperty(field.Instruction.Value, idx);
                    field.Instruction.Value = newInstruction;

                    string value = null;
                    if (!values.TryGetValue(oldKey, out value))
                    {
                        throw new WordWriterException($"No key found for {oldKey}.");
                    }

                    if (string.IsNullOrWhiteSpace(value))
                    {
                        logger.Warn($"An entry exists for key {oldKey}, but the resulting value is blank.");
                    }

                    AddCustomProperty(newPropName, value);
                }

                foreach (FieldCode field in root.Descendants<FieldCode>())
                {
                    logger.Info($"Handling FieldCode of text: {field.Text}.");
                    var (newInstruction, oldKey, newPropName) = TransformFieldProperty(field.Text, idx);
                    field.Text = newInstruction;

                    if (!values.TryGetValue(oldKey, out string value))
                    {
                        throw new WordWriterException($"No key found for {oldKey}.");
                    }

                    if (string.IsNullOrWhiteSpace(value))
                    {
                        logger.Warn($"An entry exists for key {oldKey}, but the resulting value is blank.");
                    }

                    AddCustomProperty(newPropName, value);
                }
            }

            sources.Add(new Source(new WmlDocument(template.Length.ToString(), template), true));
        }

        public MemoryStream ExportDocument()
        {
            var doc = DocumentBuilder.BuildDocument(sources);
            MemoryStream stream = new MemoryStream();

            doc.WriteByteArray(stream);
            BuildCustomProperties(stream);

            return stream;
        }

        private void BuildCustomProperties(MemoryStream documentData)
        {
            logger.Info("Building custom properties section for newly-generated document.");
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

        public class WordWriterException : Exception
        {
            public WordWriterException() { }
            public WordWriterException(string message) : base(message) { }
            public WordWriterException(string message, Exception inner) : base(message, inner) { }
        }
    }
}
