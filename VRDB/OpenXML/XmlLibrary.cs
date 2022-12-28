using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace VRDB.OpenXML
{
    public static class XmlLibrary
    {
        public static void ExportToXslx(string sheetName, string exportPath, System.Data.DataTable data, 
            bool useConditionalFormatting, bool excludeMissing, bool excludeSame, 
            string highlightHeader, string highlightMissing, string highlightSame, string highlightDifferent)
        {
            using (var document = SpreadsheetDocument.Create(exportPath, SpreadsheetDocumentType.Workbook))
            {
                // Add a WorkBook section to the document and add a WorkBook
                var workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                // Add a WorkSheet section to the WorkBook section and add a WorkSheet
                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet();

                // Add a StyleSheet section to the WorkBook section and add a StyelSheet
                var stylePart = workbookPart.AddNewPart<WorkbookStylesPart>();
                stylePart.Stylesheet = GenerateStylesheet(highlightHeader, highlightMissing, highlightSame, highlightDifferent);

                // Add column formats to the WorkSheet section
                var columns = new Columns(
                    new Column { BestFit = true, CustomWidth = true, Min = 1, Max = 1, Width = 20 },    // LastName
                    new Column { BestFit = true, CustomWidth = true, Min = 2, Max = 3, Width = 15 },    // FirstName, MiddleName
                    new Column { BestFit = true, CustomWidth = true, Min = 4, Max = 4, Width = 12 },    // Birthdate
                    new Column { BestFit = true, CustomWidth = true, Min = 5, Max = 5, Width = 8 },     // Gender
                    new Column { BestFit = true, CustomWidth = true, Min = 6, Max = 6, Width = 35 },    // Street
                    new Column { BestFit = true, CustomWidth = true, Min = 7, Max = 7, Width = 15 },    // City
                    new Column { BestFit = true, CustomWidth = true, Min = 8, Max = 8, Width = 6 },     // State
                    new Column { BestFit = true, CustomWidth = true, Min = 9, Max = 9, Width = 11 },    // Zip
                    new Column { BestFit = true, CustomWidth = true, Min = 10, Max = 10, Width = 16 },  // RegistrationDate
                    new Column { BestFit = true, CustomWidth = true, Min = 11, Max = 11, Width = 10 },  // LastVoted
                    new Column { BestFit = true, CustomWidth = true, Min = 12, Max = 12, Width = 9 },   // Status
                    new Column { BestFit = true, CustomWidth = true, Min = 13, Max = 13, Width = 9 }    // Compare
                );
                worksheetPart.Worksheet.AppendChild(columns);

                // Add a Sheet collection to the WorkBook
                var sheets = workbookPart.Workbook.AppendChild(new Sheets());

                // Add a Sheet to the Sheet collection
                var sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = sheetName };
                sheets.Append(sheet);

                // Add a SheetData -- the cells of data -- to the WorkSheet
                var sheetData = worksheetPart.Worksheet.AppendChild(new SheetData());

                if (useConditionalFormatting)
                {
                    // Create a conditional format section
                    var cf = new ConditionalFormatting
                    {
                        SequenceOfReferences = new ListValue<StringValue>()
                        {
                            InnerText = $"A2:M{data.Rows.Count + 1}"
                        }
                    };

                    var cfr1 = new ConditionalFormattingRule
                    {
                        Type = ConditionalFormatValues.Expression,
                        FormatId = 2U,
                        Priority = 1
                    };
                    var formula1 = new Formula
                    {
                        Text = $"$M2=\"{Constants.LabelDifferent}\""
                    };
                    cfr1.Append(formula1);
                    cf.Append(cfr1);

                    var cfr2 = new ConditionalFormattingRule
                    {
                        Type = ConditionalFormatValues.Expression,
                        FormatId = 1U,
                        Priority = 2
                    };
                    var formula2 = new Formula
                    {
                        Text = $"$M2=\"{Constants.LabelSame}\""
                    };
                    cfr2.Append(formula2);
                    cf.Append(cfr2);

                    var cfr3 = new ConditionalFormattingRule
                    {
                        Type = ConditionalFormatValues.Expression,
                        FormatId = 0U,
                        Priority = 3
                    };
                    var formula3 = new Formula
                    {
                        Text = $"$M2=\"{Constants.LabelMissing}\""
                    };
                    cfr3.Append(formula3);
                    cf.Append(cfr3);
                    worksheetPart.Worksheet.Append(cf);
                }

                // Header row
                Row row;
                uint styleIndex = 2;
                var cells = new Cell[data.Columns.Count];
                for (int c = 0; c < data.Columns.Count; c++)
                {
                    cells[c] = ConstructCell(data.Columns[c].ColumnName, CellValues.String, styleIndex);
                }
                row = new Row();
                row.Append(cells);
                sheetData.AppendChild(row);

                // Data Rows
                styleIndex = 0;
                for (int r = 0; r < data.Rows.Count; r++)
                {
                    cells = new Cell[data.Columns.Count];
                    var dataRow = data.Rows[r];

                    var status = dataRow.ItemArray[12].ToString();
                    if (status == Constants.LabelMissing && excludeMissing)
                        continue;
                    if (status == Constants.LabelSame && excludeSame)
                        continue;

                    if (!useConditionalFormatting)
                    {
                        switch (dataRow[data.Columns["Compare"].Ordinal])
                        {
                            case Constants.LabelMissing:
                                styleIndex = 3;
                                break;
                            case Constants.LabelSame:
                                styleIndex = 4;
                                break;
                            case Constants.LabelDifferent:
                                styleIndex = 5;
                                break;
                            default:
                                styleIndex = 0;
                                break;
                        }
                    }
                    for (int c = 0; c < data.Columns.Count; c++)
                    {
                        cells[c] = ConstructCell(dataRow[c].ToString(), CellValues.String, styleIndex);
                    }
                    row = new Row();
                    row.Append(cells);
                    sheetData.AppendChild(row);
                }

                // Save the document
                document.Close();
            }
        }

        private static Cell ConstructCell(string value, CellValues dataType, uint styleIndex = 0)
        {
            return new Cell()
            {
                CellValue = new CellValue(value),
                DataType = new EnumValue<CellValues>(dataType),
                StyleIndex = styleIndex
            };
        }

        private static Stylesheet GenerateStylesheet(string highlightHeader, string highlightMissing, string highlightSame, string highlightDifferent)
        {
            var fonts = new Fonts(
                new Font( // Index 0 - default
                    new FontSize() { Val = 11 }

                )
            );

            var fills = new Fills(
                new Fill(new PatternFill() { PatternType = PatternValues.None }), // Index 0 - default
                new Fill(new PatternFill() { PatternType = PatternValues.Gray125 }), // Index 1
                new Fill(
                    new PatternFill(
                        new ForegroundColor { Rgb = new HexBinaryValue() { Value = highlightHeader } }
                    )
                    { PatternType = PatternValues.Solid }
                ), // Index 2 - header
                new Fill(
                    new PatternFill(
                        new ForegroundColor { Rgb = new HexBinaryValue() { Value = highlightMissing } }
                    )
                    { PatternType = PatternValues.Solid }
                ), // Index 3 - missing
                new Fill(
                    new PatternFill(
                        new ForegroundColor { Rgb = new HexBinaryValue() { Value = highlightSame } }
                    )
                    { PatternType = PatternValues.Solid }
                ), // Index 4 - same
                new Fill(
                    new PatternFill(
                        new ForegroundColor { Rgb = new HexBinaryValue() { Value = highlightDifferent } }
                    )
                    { PatternType = PatternValues.Solid }
                ) // Index 5 - different
            );

            var borders = new Borders(
                new Border(), // index 0 default
                new Border( // index 1 black border
                    new LeftBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                    new RightBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                    new TopBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                    new BottomBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                    new DiagonalBorder())
            );

            var cellFormats = new CellFormats(
                new CellFormat(), // default
                new CellFormat { FillId = 0 }, // body
                new CellFormat { FillId = 2, ApplyFill = true }, // header
                new CellFormat { FillId = 3, ApplyFill = true }, // missing
                new CellFormat { FillId = 4, ApplyFill = true }, // same
                new CellFormat { FillId = 5, ApplyFill = true }  // different
            );

            var differentialFormats = new DifferentialFormats() { Count = (UInt32Value)3U };
            differentialFormats.Append(
                new DifferentialFormat(
                    new Fill(new PatternFill(new BackgroundColor() { Rgb = highlightMissing }))
                )
            );
            differentialFormats.Append(
                new DifferentialFormat(
                    new Fill(new PatternFill(new BackgroundColor() { Rgb = highlightSame }))
                )
            );
            differentialFormats.Append(
                new DifferentialFormat(
                    new Fill(new PatternFill(new BackgroundColor() { Rgb = highlightDifferent }))
                )
            );

            return new Stylesheet(fonts, fills, borders, cellFormats, differentialFormats);
        }
    }
}
