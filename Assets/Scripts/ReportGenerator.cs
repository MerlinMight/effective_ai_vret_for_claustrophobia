using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using iTextSharp.text;
using iTextSharp.text.pdf;

public class ReportGenerator : MonoBehaviour
{
    public static ReportGenerator Instance;
    public string patientName = "Unknown Patient";

    private Dictionary<string, float> timeSpentPerLevel = new Dictionary<string, float>();
    private Dictionary<string, int> panicButtonUsage = new Dictionary<string, int>();
    private float sessionStartTime;
    private int totalLevels = 6; // Ensure this matches your Unity level count
    string[] Names = { "Introductory", "Mild", "Moderate", "Significant", "Severe", "Extreme" };


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // Prevent duplicate instances
        }
    }

    void Start()
    {
        sessionStartTime = Time.time; // Capture session start time
    }

    public void RecordLevelTime(string levelName, float timeSpent)
    {
        if (timeSpentPerLevel.ContainsKey(levelName))
        {
            timeSpentPerLevel[levelName] += timeSpent;
        }
        else
        {
            timeSpentPerLevel[levelName] = timeSpent;
        }
    }

    public void RecordPanicButtonPress(string levelName)
    {
        if (panicButtonUsage.ContainsKey(levelName))
        {
            panicButtonUsage[levelName]++;
        }
        else
        {
            panicButtonUsage[levelName] = 1;
        }
    }

    public void OnSessionEnd()
    {
        GeneratePDFReport();
    }

    public void GeneratePDFReport()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "VRET_Report.pdf");

        try
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                Document document = new Document(PageSize.A4, 50f, 50f, 50f, 50f); // Adjusted margins for better spacing
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                document.Open();

                // 🎨 Add Page Border
                PdfContentByte canvas = writer.DirectContent;
                Rectangle pageBorder = new Rectangle(document.PageSize);
                pageBorder.Left += 30f;
                pageBorder.Right -= 30f;
                pageBorder.Top -= 30f;
                pageBorder.Bottom += 30f;
                pageBorder.Border = Rectangle.BOX;
                pageBorder.BorderWidth = 3f;
                pageBorder.BorderColor = new BaseColor(80, 80, 80);
                canvas.Rectangle(pageBorder);

                // 🏆 Styled Header
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 24, new BaseColor(30, 60, 120)); // Deep blue
                Paragraph header = new Paragraph("📘 Claustrophobia VRET Report", headerFont);
                header.Alignment = Element.ALIGN_CENTER;
                header.SpacingBefore = -25f;
                header.SpacingAfter = 20f; // Moves it higher
                document.Add(header);

                // 📝 Patient Info
                var infoFont = FontFactory.GetFont(FontFactory.HELVETICA, 14, BaseColor.BLACK);
                document.Add(new Paragraph($"👤 Patient Name: {patientName}", infoFont));
                document.Add(new Paragraph($"📅 Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}", infoFont));
                document.Add(new Paragraph("\n"));

                // 📊 Exposure Summary Table
                Paragraph tableTitle = new Paragraph("📊 Exposure Summary", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, new BaseColor(50, 120, 180)));
                tableTitle.SpacingAfter = 8f;
                document.Add(tableTitle);

                PdfPTable summaryTable = new PdfPTable(3);
                summaryTable.WidthPercentage = 100;
                summaryTable.SetWidths(new float[] { 2f, 2f, 2f });

                summaryTable.AddCell(CreateHeaderCell("Levels"));
                summaryTable.AddCell(CreateHeaderCell("Time Spent"));
                summaryTable.AddCell(CreateHeaderCell("Panic Usage"));

                float totalSessionTime = 0f;

                for (int i = 0; i < totalLevels; i++)
                {
                    string levelName = $"Level {i + 1}";
                    float levelTime = timeSpentPerLevel.ContainsKey(levelName) ? timeSpentPerLevel[levelName] : 0f;
                    int panicCount = panicButtonUsage.ContainsKey(levelName) ? panicButtonUsage[levelName] : 0;

                    totalSessionTime += levelTime;

                    summaryTable.AddCell(CreateDataCell(levelName));
                    summaryTable.AddCell(CreateDataCell(FormatTime(levelTime)));
                    summaryTable.AddCell(CreateDataCell($"{panicCount} time(s)"));
                }

                document.Add(summaryTable);
                document.Add(new Paragraph("\n"));

                // ⏳ Total Time
                Paragraph totalTime = new Paragraph($"⏳ Total Session Time: {FormatTime(totalSessionTime)}", infoFont);
                totalTime.SpacingAfter = 12f;
                document.Add(totalTime);

                // 📌 Observations & Recommendations (In Table Format)
                Paragraph obsHeader = new Paragraph("📌 Observations", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, new BaseColor(220, 80, 60))); // Red-orange title
                obsHeader.SpacingAfter = 8f;
                document.Add(obsHeader);

                PdfPTable obsTable = new PdfPTable(2);
                obsTable.WidthPercentage = 100;
                obsTable.SetWidths(new float[] { 2f, 5f });

                obsTable.AddCell(CreateHeaderCell("Levels"));
                obsTable.AddCell(CreateHeaderCell("Observations"));

                List<string> observations = GenerateObservations();

                for (int i = 0; i < totalLevels; i++)
                {
                    string levelName =  Names[i];//$"Level {i + 1}";
                    string obsText = observations[i];

                    PdfPCell levelCell = CreateDataCell(levelName);
                    levelCell.BackgroundColor = new BaseColor(230, 240, 250); // Light pastel blue
                    obsTable.AddCell(levelCell);

                    PdfPCell obsCell = CreateDataCell(obsText);
                    obsCell.BackgroundColor = new BaseColor(250, 230, 240); // Light pastel pink
                    obsTable.AddCell(obsCell);
                }

                //document.Add(obsTable);

                // 🌟 Overall Summary
                PdfPCell overallCell = new PdfPCell(new Phrase(observations[totalLevels], FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK)));
                overallCell.Colspan = 2;
                overallCell.HorizontalAlignment = Element.ALIGN_CENTER;
                overallCell.BackgroundColor = new BaseColor(200, 250, 200); // Light pastel green
                obsTable.AddCell(overallCell);
                document.Add(obsTable);

                // 🔚 Footer
                Paragraph footer = new Paragraph("\nGenerated by Claustrophobia VRET System ©", FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 10, BaseColor.GRAY));
                footer.Alignment = Element.ALIGN_CENTER;
                document.Add(footer);

                document.Close();
            }

            Debug.Log($"✅ PDF Report Generated: {filePath}");
            OpenReport(filePath);
        }
        catch (Exception ex)
        {
            Debug.LogError($"❗ Error generating PDF report: {ex.Message}");
        }
    }

    private PdfPCell CreateInfoCell(string text, iTextSharp.text.Font font)
    {
        PdfPCell cell = new PdfPCell(new Phrase(text, font));
        cell.Border = Rectangle.NO_BORDER;
        cell.PaddingBottom = 6f;
        return cell;
    }

    private PdfPCell CreateHeaderCell(string text)
    {
        PdfPCell cell = new PdfPCell(new Phrase(text, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.WHITE)));
        cell.BackgroundColor = new BaseColor(100, 120, 180); // soft blue
        cell.HorizontalAlignment = Element.ALIGN_CENTER;
        cell.Padding = 5;
        return cell;
    }

    private PdfPCell CreateDataCell(string text)
    {
        PdfPCell cell = new PdfPCell(new Phrase(text, FontFactory.GetFont(FontFactory.HELVETICA, 11, BaseColor.BLACK)));
        cell.HorizontalAlignment = Element.ALIGN_CENTER;
        cell.Padding = 5;
        cell.BackgroundColor = new BaseColor(240, 248, 255); // Alice Blue
        return cell;
    }

    private string FormatTime(float seconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(seconds);
        return string.Format("{0:D2}:{1:D2}:{2:D2}", time.Hours, time.Minutes, time.Seconds);
    }

    private List<string> GenerateObservations()
    {
        List<string> observations = new List<string>();
        int totalPanicCount = 0;

        for (int i = 0; i < totalLevels; i++)
        {
            string levelName = $"Level {i + 1}";
            int panicCount = panicButtonUsage.ContainsKey(levelName) ? panicButtonUsage[levelName] : 0;
            totalPanicCount += panicCount;

            if (panicCount == 0)
            {
                observations.Add($"✅Patient handled this level confidently without panic.");
            }
            else
            {
                if (i <= 2) // Levels 1–3 (Introductory to Moderate)
                {
                    if (panicCount == 1)
                        observations.Add($"⚠️Mild distress detected. Monitor closely if repeated.");
                    else if (panicCount == 2)
                        observations.Add($"❗ Moderate discomfort observed. Consider revisiting relaxation strategies.");
                    else
                        observations.Add($"🚨High panic response. Recommend avoiding this level temporarily and increasing preparatory sessions.");
                }
                else // Levels 4–6 (Severe to Extreme)
                {
                    if (panicCount == 1)
                    {
                        bool hadNoPriorPanics = totalPanicCount == 1; // First panic of the entire session
                        if (hadNoPriorPanics)
                        {
                            observations.Add($"⚠️Moderate to high distress observed at this advanced stage. Consider focused coping strategies before proceeding further.");
                        }
                        else
                        {
                            observations.Add($"🚨High distress observed. Strongly recommend intensive relaxation and coping strategies before retrying.");
                        }
                    }
                    else if (panicCount == 2)
                    {
                        observations.Add($"❗High distress detected at this level. Consider regression and reinforcement of relaxation techniques.");
                    }
                    else // panicCount >= 3
                    {
                        observations.Add($"🛑Severe anxiety response. Patient may not yet be ready for this intensity. Regression and therapeutic support advised.");
                    }
                }

            }
        }

        // Add Overall Summary
        if (totalPanicCount == 0)
        {
            observations.Add("\n🟢 Overall: Patient displayed strong coping skills throughout the session.");
        }
        else if (totalPanicCount <= 3)
        {
            observations.Add("\n🟡 Overall: Some signs of discomfort were observed. Controlled breathing and gradual exposure are advised.");
        }
        else
        {
            observations.Add("\n🔴 Overall: Significant anxiety observed. Recommend regression to lower levels and further relaxation training.");
        }

        return observations;
    }


    private void OpenReport(string filePath)
    {
#if UNITY_EDITOR
        Application.OpenURL(filePath); // Open in default browser during development
#elif UNITY_STANDALONE_WIN
        System.Diagnostics.Process.Start(filePath);
#elif UNITY_STANDALONE_OSX
        System.Diagnostics.Process.Start("open", filePath);
#elif UNITY_STANDALONE_LINUX
        System.Diagnostics.Process.Start("xdg-open", filePath);
#else
        Debug.LogWarning("Opening PDF reports is not supported on this platform.");
#endif
    }
}










// using System;
// using System.Collections.Generic;
// using System.IO;
// using UnityEngine;
// using iTextSharp.text;
// using iTextSharp.text.pdf;

// public class ReportGenerator : MonoBehaviour
// {
//     public static ReportGenerator Instance;
//     public string patientName = "Unknown Patient";

//     private Dictionary<string, float> timeSpentPerLevel = new Dictionary<string, float>();
//     private Dictionary<string, int> panicButtonUsage = new Dictionary<string, int>();
//     private float sessionStartTime;
//     private int totalLevels = 6;

//     void Awake()
//     {
//         if (Instance == null)
//         {
//             Instance = this;
//             DontDestroyOnLoad(gameObject);
//         }
//         else
//         {
//             Destroy(gameObject);
//         }
//     }

//     void Start()
//     {
//         sessionStartTime = Time.time;
//     }

//     public void RecordLevelTime(string levelName, float timeSpent)
//     {
//         if (timeSpentPerLevel.ContainsKey(levelName))
//             timeSpentPerLevel[levelName] += timeSpent;
//         else
//             timeSpentPerLevel[levelName] = timeSpent;
//     }

//     public void RecordPanicButtonPress(string levelName)
//     {
//         if (panicButtonUsage.ContainsKey(levelName))
//             panicButtonUsage[levelName]++;
//         else
//             panicButtonUsage[levelName] = 1;
//     }

//     public void OnSessionEnd()
//     {
//         GeneratePDFReport();
//     }

//     public void GeneratePDFReport()
//     {
//         string filePath = Path.Combine(Application.persistentDataPath, "VRET_Report.pdf");

//         try
//         {
//             using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
//             {
//                 Document document = new Document(PageSize.A4);
//                 PdfWriter.GetInstance(document, stream);
//                 document.Open();

//                 // Define Fonts and Colors
//                 BaseColor headerColor = new BaseColor(52, 152, 219); // Blue Header
//                 BaseColor tableHeaderColor = new BaseColor(46, 204, 113); // Green Table Header
//                 BaseColor alternateRowColor = new BaseColor(230, 230, 230); // Light Gray
//                 iTextSharp.text.Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 22, headerColor);
//                 iTextSharp.text.Font tableHeaderFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, BaseColor.WHITE);
//                 iTextSharp.text.Font textFont = FontFactory.GetFont(FontFactory.HELVETICA, 12, BaseColor.BLACK);

//                 // Title Section
//                 Paragraph title = new Paragraph("🌀 Claustrophobia VRET Report", headerFont);
//                 title.Alignment = Element.ALIGN_CENTER;
//                 document.Add(title);
//                 document.Add(new Paragraph("\n"));

//                 // Patient Information
//                 document.Add(new Paragraph($"👤 Patient Name: {patientName}", textFont));
//                 document.Add(new Paragraph($"📅 Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}", textFont));
//                 document.Add(new Paragraph("\n"));

//                 // Table for Time Spent Per Level
//                 PdfPTable timeTable = new PdfPTable(2);
//                 timeTable.WidthPercentage = 100;
//                 timeTable.SetWidths(new float[] { 3, 2 });

//                 PdfPCell timeHeader = new PdfPCell(new Phrase("🕒 Time Spent Per Level", tableHeaderFont))
//                 {
//                     BackgroundColor = tableHeaderColor,
//                     Colspan = 2,
//                     HorizontalAlignment = Element.ALIGN_CENTER,
//                     Padding = 5
//                 };
//                 timeTable.AddCell(timeHeader);

//                 float totalSessionTime = 0f;

//                 for (int i = 0; i < totalLevels; i++)
//                 {
//                     string levelName = $"Level {i+1}";
//                     float levelTime = timeSpentPerLevel.ContainsKey(levelName) ? timeSpentPerLevel[levelName] : 0;
//                     totalSessionTime += levelTime;

//                     PdfPCell levelCell = new PdfPCell(new Phrase(levelName, textFont))
//                     {
//                         BackgroundColor = (i % 2 == 0) ? alternateRowColor : BaseColor.WHITE,
//                         Padding = 5
//                     };
//                     PdfPCell timeCell = new PdfPCell(new Phrase(FormatTime(levelTime), textFont))
//                     {
//                         BackgroundColor = (i % 2 == 0) ? alternateRowColor : BaseColor.WHITE,
//                         Padding = 5
//                     };

//                     timeTable.AddCell(levelCell);
//                     timeTable.AddCell(timeCell);
//                 }
//                 document.Add(timeTable);
//                 document.Add(new Paragraph("\n"));

//                 // Table for Panic Button Usage
//                 PdfPTable panicTable = new PdfPTable(2);
//                 panicTable.WidthPercentage = 100;
//                 panicTable.SetWidths(new float[] { 3, 2 });

//                 PdfPCell panicHeader = new PdfPCell(new Phrase("⚠️ Panic Button Usage", tableHeaderFont))
//                 {
//                     BackgroundColor = headerColor,
//                     Colspan = 2,
//                     HorizontalAlignment = Element.ALIGN_CENTER,
//                     Padding = 5
//                 };
//                 panicTable.AddCell(panicHeader);

//                 for (int i = 0; i < totalLevels; i++)
//                 {
//                     string levelName = $"Level {+1}";
//                     int panicCount = panicButtonUsage.ContainsKey(levelName) ? panicButtonUsage[levelName] : 0;

//                     PdfPCell levelCell = new PdfPCell(new Phrase(levelName, textFont))
//                     {
//                         BackgroundColor = (i % 2 == 0) ? alternateRowColor : BaseColor.WHITE,
//                         Padding = 5
//                     };
//                     PdfPCell panicCell = new PdfPCell(new Phrase($"{panicCount} times", textFont))
//                     {
//                         BackgroundColor = (i % 2 == 0) ? alternateRowColor : BaseColor.WHITE,
//                         Padding = 5
//                     };

//                     panicTable.AddCell(levelCell);
//                     panicTable.AddCell(panicCell);
//                 }
//                 document.Add(panicTable);
//                 document.Add(new Paragraph("\n"));

//                 // Total Session Time
//                 document.Add(new Paragraph($"⏳ Total Session Time: {FormatTime(totalSessionTime)}", textFont));
//                 document.Add(new Paragraph("\n"));

//                 // Observations & Recommendations
//                 document.Add(new Paragraph("📌 Observations & Recommendations:", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, BaseColor.RED)));
//                 document.Add(new Paragraph("──────────────────────────────────────", textFont));
//                 document.Add(new Paragraph(GenerateObservations(), textFont));

//                 // Footer
//                 document.Add(new Paragraph("\n──────────────────────────────────────"));
//                 document.Add(new Paragraph("🌀 Generated by Claustrophobia VRET System ©", FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.GRAY)));

//                 document.Close();
//             }

//             Debug.Log($"✅ PDF Report Generated: {filePath}");
//             OpenReport(filePath);
//         }
//         catch (Exception ex)
//         {
//             Debug.LogError($"❗ Error generating PDF report: {ex.Message}");
//         }
//     }

//     private string FormatTime(float seconds)
//     {
//         TimeSpan time = TimeSpan.FromSeconds(seconds);
//         return string.Format("{0:D2}:{1:D2}:{2:D2}", time.Hours, time.Minutes, time.Seconds);
//     }

//     private string GenerateObservations()
//     {
//         int panicTotal = 0;
//         foreach (var panic in panicButtonUsage.Values)
//             panicTotal += panic;

//         if (panicTotal > 5) return "🚨 High panic response. Recommend more gradual exposure.";
//         if (panicTotal > 2) return "⚠️ Moderate panic detected. Consider relaxation exercises.";
//         return "✅ Patient adapted well with minimal panic responses.";
//     }

//         private void OpenReport(string filePath)
//         {
//     #if UNITY_EDITOR
//             Application.OpenURL(filePath); // Open in default browser during development
//     #elif UNITY_STANDALONE_WIN
//             System.Diagnostics.Process.Start(filePath);
//     #elif UNITY_STANDALONE_OSX
//             System.Diagnostics.Process.Start("open", filePath);
//     #elif UNITY_STANDALONE_LINUX
//             System.Diagnostics.Process.Start("xdg-open", filePath);
//     #else
//             Debug.LogWarning("Opening PDF reports is not supported on this platform.");
//     #endif
//         }
// }









// using System;
// using System.Collections.Generic;
// using System.IO;
// using UnityEngine;
// using iTextSharp.text;
// using iTextSharp.text.pdf;

// public class ReportGenerator : MonoBehaviour
// {
//     public static ReportGenerator Instance;
//     public string patientName = "Unknown Patient";

//     private Dictionary<string, float> timeSpentPerLevel = new Dictionary<string, float>();
//     private Dictionary<string, int> panicButtonUsage = new Dictionary<string, int>();
//     private float sessionStartTime;
//     private int totalLevels = 6;

//     void Awake()
//     {
//         if (Instance == null)
//         {
//             Instance = this;
//             DontDestroyOnLoad(gameObject);
//         }
//         else
//         {
//             Destroy(gameObject);
//         }
//     }

//     void Start()
//     {
//         sessionStartTime = Time.time;
//     }

//     public void RecordLevelTime(string levelName, float timeSpent)
//     {
//         if (timeSpentPerLevel.ContainsKey(levelName))
//             timeSpentPerLevel[levelName] += timeSpent;
//         else
//             timeSpentPerLevel[levelName] = timeSpent;
//     }

//     public void RecordPanicButtonPress(string levelName)
//     {
//         if (panicButtonUsage.ContainsKey(levelName))
//             panicButtonUsage[levelName]++;
//         else
//             panicButtonUsage[levelName] = 1;
//     }

//     public void OnSessionEnd()
//     {
//         GeneratePDFReport();
//     }

//     public void GeneratePDFReport()
//     {
//         string filePath = Path.Combine(Application.persistentDataPath, "VRET_Report.pdf");

//         try
//         {
//             using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
//             {
//                 Document document = new Document(PageSize.A4);
//                 PdfWriter.GetInstance(document, stream);
//                 document.Open();

//                 // Title Section
//                 iTextSharp.text.Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 24, BaseColor.BLACK);
//                 Paragraph title = new Paragraph("Claustrophobia VRET Report", headerFont);
//                 title.Alignment = Element.ALIGN_CENTER;
//                 document.Add(title);
//                 document.Add(new Paragraph("\n"));

//                 // Patient Info
//                 iTextSharp.text.Font infoFont = FontFactory.GetFont(FontFactory.HELVETICA, 14, BaseColor.BLACK);
//                 document.Add(new Paragraph($"Patient Name: {patientName}", infoFont));
//                 document.Add(new Paragraph($"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}", infoFont));
//                 document.Add(new Paragraph("\n"));

//                 // Visual: Time Spent per Level (Bar Chart)
//                 document.Add(new Paragraph("📊 Time Spent per Level", headerFont));
//                 AddBarChart(document, timeSpentPerLevel, "Time Spent (in minutes)");

//                 // Visual: Panic Button Usage (Pie Chart)
//                 document.Add(new Paragraph("\n📊 Panic Button Usage", headerFont));
//                 AddPieChart(document, panicButtonUsage, "Panic Button Usage");

//                 // Visual: Line Chart (Total Time Progression)
//                 document.Add(new Paragraph("\n📈 Total Session Time Progression", headerFont));
//                 AddLineChart(document, timeSpentPerLevel);

//                 // Summary Table
//                 document.Add(new Paragraph("\n📋 Summary Table", headerFont));
//                 AddSummaryTable(document);

//                 // Recommendations Section
//                 document.Add(new Paragraph("\n🧾 Recommendations", headerFont));
//                 document.Add(new Paragraph(GenerateObservations(), infoFont));

//                 // Close Document
//                 document.Close();
//             }

//             Debug.Log($"✅ PDF Report Generated: {filePath}");
//             OpenReport(filePath);
//         }
//         catch (Exception ex)
//         {
//             Debug.LogError($"❗ Error generating PDF report: {ex.Message}");
//         }
//     }

//     private void AddSummaryTable(Document document)
//     {
//         PdfPTable table = new PdfPTable(3);
//         table.WidthPercentage = 100;
//         table.SetWidths(new float[] { 3, 2, 2 });

//         // Header
//         table.AddCell("Level");
//         table.AddCell("Time Spent (HH:MM:SS)");
//         table.AddCell("Panic Button Usage");

//         float totalSessionTime = 0f;

//         for (int i = 0; i < totalLevels; i++)
//         {
//             string levelName = $"Level {i + 1}";
//             float timeSpent = timeSpentPerLevel.ContainsKey(levelName) ? timeSpentPerLevel[levelName] : 0;
//             int panicCount = panicButtonUsage.ContainsKey(levelName) ? panicButtonUsage[levelName] : 0;
//             totalSessionTime += timeSpent;

//             table.AddCell(levelName);
//             table.AddCell(FormatTime(timeSpent));
//             table.AddCell(panicCount.ToString());
//         }

//         // Total Row
//         table.AddCell("Total");
//         table.AddCell(FormatTime(totalSessionTime));
//         table.AddCell("-");

//         document.Add(table);
//     }

//     private string FormatTime(float seconds)
//     {
//         TimeSpan time = TimeSpan.FromSeconds(seconds);
//         return $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}";
//     }

//     private string GenerateObservations()
//     {
//         int panicTotal = 0;
//         foreach (var panic in panicButtonUsage.Values)
//             panicTotal += panic;

//         if (panicTotal > 5) return "🚨 High panic response. Recommend more gradual exposure.";
//         if (panicTotal > 2) return "⚠️ Moderate panic detected. Consider relaxation exercises.";
//         return "✅ Patient adapted well with minimal panic responses.";
//     }

//     private void OpenReport(string filePath)
//     {
// #if UNITY_EDITOR
//         Application.OpenURL(filePath);
// #elif UNITY_STANDALONE_WIN
//         System.Diagnostics.Process.Start(filePath);
// #elif UNITY_STANDALONE_OSX
//         System.Diagnostics.Process.Start("open", filePath);
// #elif UNITY_STANDALONE_LINUX
//         System.Diagnostics.Process.Start("xdg-open", filePath);
// #else
//         Debug.LogWarning("Opening PDF reports is not supported on this platform.");
// #endif
//     }

//     private void AddBarChart(Document document, Dictionary<string, float> data, string chartTitle)
//     {
//         // Placeholder for bar chart generation (Add actual chart generation using external libraries like XCharts if required)
//         document.Add(new Paragraph($"[Bar Chart: {chartTitle} Placeholder]"));
//     }

//     private void AddPieChart(Document document, Dictionary<string, int> data, string chartTitle)
//     {
//         // Placeholder for pie chart generation
//         document.Add(new Paragraph($"[Pie Chart: {chartTitle} Placeholder]"));
//     }

//     private void AddLineChart(Document document, Dictionary<string, float> data)
//     {
//         // Placeholder for line chart generation
//         document.Add(new Paragraph($"[Line Chart: Total Session Time Progression Placeholder]"));
//     }
// }


