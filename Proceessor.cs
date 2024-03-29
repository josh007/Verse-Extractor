using System;
using System.Collections.Generic;
using System.Linq;

namespace VerseExtractor
{
    public class Proceessor
    {
        private List<VerseHolder> ExtractedVersesWithOutText { get; set; }
        private List<VerseHolder> ProcessedVersesWithOutText { get; set; }
        private List<VerseHolder> ListOfVersesWithText { get; set; }

        public string OutPutText { get; set; }
        public string Warning { get; set; }

        const string ReadThisPhrase = "ን አንብብ።";

        public Proceessor()
        {
            ExtractedVersesWithOutText = new List<VerseHolder>();
            ProcessedVersesWithOutText = new List<VerseHolder>();
            ListOfVersesWithText = new List<VerseHolder>();
        }

        public void ExtractVerseAndProcessRaw(string content)
        {
            FirstPass(content);
            SecondPass(content);

            ProcessForPropoerManupilation();
            ProcessOutput();
        }

        private void ProcessOutput()
        {
            var processedContent = "\r\n---------------RESULT 1---------------\r\n";
            processedContent += string.Join("\r\n", ExtractedVersesWithOutText.OrderBy(v => Convert.ToInt32(v.ParagraphNo)).Select(v => "[" + v.ParagraphNo + "] " + v.Verse).ToArray());

            processedContent += "\r\n---------------RESULT 2---------------\r\n";
            processedContent += string.Join("\r\n", ProcessedVersesWithOutText.OrderBy(v => Convert.ToInt32(v.ParagraphNo)).Select(v => "[" + v.ParagraphNo + "] " + v.Verse +
                                    (v.isReadAuthorized ? ReadThisPhrase : "")).ToArray());

            OutPutText = processedContent;
        }
        public void MergeProcessedVerseWithVerseText(string content)
        {
            ThirdPass(content);
            MergeVerseText();
        }

        private void FirstPass(string content)
        {
            var paragraphs = content.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var par in paragraphs)
            {
                var vers = par.Split('(');
                string currentNo = "";

                bool isFirstIteration = true;
                foreach (var ver in vers)
                {
                    if (isFirstIteration)
                    {
                        if (!char.IsNumber(ver[0]))
                            currentNo = "1";
                        else
                        {
                            currentNo = ver[0].ToString();
                            if (char.IsNumber(ver[1]))
                                currentNo += ver[1];
                        }
                        isFirstIteration = false;
                        continue;
                    }

                    var singleVers = ver.Split(')')[0];
                    if (singleVers.Any(c => char.IsNumber(c)) && currentNo != "")
                        ExtractedVersesWithOutText.Add(new VerseHolder { ParagraphNo = currentNo, Verse = singleVers });
                }
            }
        }
        private void SecondPass(string content)
        {
            var paragraphs = content.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var par in paragraphs)
            {

                var vers = par.Split('—');
                string currentNo = "";

                bool isFirstIteration = true;
                foreach (var ver in vers)
                {
                    if (isFirstIteration)
                    {
                        if (!char.IsNumber(ver[0]))
                            currentNo = "1";
                        else
                        {
                            currentNo = ver[0].ToString();
                            if (char.IsNumber(ver[1]))
                                currentNo += ver[1];
                        }
                        isFirstIteration = false;
                        continue;
                    }

                    ExtractedVersesWithOutText.Add(new VerseHolder { ParagraphNo = currentNo, Verse = vers[1] });
                }
            }
        }
        private void ThirdPass(string content)
        {
            var paragraphs = content.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var par in paragraphs)
            {
                var vers = par.Split('(');
                string currentNo = "";


                bool found = false;
                foreach (var c in vers[0])
                {
                    if (char.IsNumber(c))
                    {
                        found = true;
                        currentNo += c.ToString();
                    }
                    else if (found)
                        break;
                }

                var singleVers = vers[1].Split(')');
                if (singleVers[0].Any(c => char.IsNumber(c)))
                    ListOfVersesWithText.Add(new VerseHolder { ParagraphNo = currentNo, Verse = singleVers[0], VerseText = singleVers[1] });
            }
        }

        private void ProcessForPropoerManupilation()
        {
            for (int i = 0; i < ExtractedVersesWithOutText.Count; i++)
            {
                var isReadAuthorized = false;
                int number;
                var extractedVerWithOutText = ExtractedVersesWithOutText[i];

                if (!extractedVerWithOutText.Verse.Contains(":")) // means this is not a text but something else
                    continue;

                // if multiple verses are in same box . . . with a ; in amharic ofcourse
                var splitMultipleVers = extractedVerWithOutText.Verse.Split(new[] { char.ConvertFromUtf32(4964) }, StringSplitOptions.RemoveEmptyEntries);

                var extractedBookName = string.Join("", splitMultipleVers[0].TakeWhile(c => !char.IsNumber(c)).Select(c => c.ToString()).ToArray());
                foreach (var singleVers in splitMultipleVers)
                {
                    var splitOnlyBookChapterVers = singleVers.Split(new[] { ReadThisPhrase }, StringSplitOptions.RemoveEmptyEntries);
                    isReadAuthorized = singleVers.Contains(ReadThisPhrase);

                    if (splitOnlyBookChapterVers.Count() > 1 && splitOnlyBookChapterVers[1] != ReadThisPhrase)
                        Warning = "WARNING: Different Read This Phrase detected . . . fix it and re-run";

                    var splitBookChapterVers = splitOnlyBookChapterVers[0].Split(':'); //0:book + chapter, verses
                    string name = splitBookChapterVers[0].TrimStart();

                    var bookName = string.Join("", name.TakeWhile(c => !char.IsNumber(c)).Select(c => c.ToString()).ToArray());
                    if (char.IsNumber(name[0]) && !Int32.TryParse(name, out number)) // means 1 soemthing 12:2 or there is no book name but only a chapter . . .
                    {
                        name = name.Remove(0, 1).Trim();
                        bookName = splitBookChapterVers[0].TrimStart().Substring(0, 1) + " " + string.Join("", name.TakeWhile(c => !char.IsNumber(c)).Select(c => c.ToString()).ToArray());
                    }
                    
                    string chapter = string.Join("", name.Split(new[] { bookName }, StringSplitOptions.RemoveEmptyEntries)[0].SkipWhile(c => !char.IsNumber(c)).Select(c => c.ToString()).ToArray());
                    
                    if (singleVers.Contains(ReadThisPhrase))
                    {
                        bookName = GetMappedBookName(bookName);    
                    }

                    if (bookName.Trim() == "")
                        bookName = extractedBookName;
                    else
                        extractedBookName = bookName;

                    bookName = bookName.TrimStart();
                    if (singleVers.Contains(","))
                    {
                        var splitCommaVers = splitBookChapterVers[1].Split(',');
                        var prev = ""; //1, 19, 20,22-24,27,28
                        bool found = false;
                        foreach (var ver in splitCommaVers)
                        {
                            if (prev == "")
                            {
                                prev = ver;
                                continue;
                            }
                            int _prev, _cur = 0;
                            if (Int32.TryParse(prev, out _prev) && Int32.TryParse(ver, out _cur))
                            {
                                if (_prev + 1 == _cur)
                                {
                                    ProcessedVersesWithOutText.Add(
                                        new VerseHolder
                                            {
                                                ParagraphNo = extractedVerWithOutText.ParagraphNo,
                                                Verse = bookName + chapter + ":" + _prev + ", " + _cur,
                                                isReadAuthorized = isReadAuthorized
                                            });
                                    found = true;
                                }
                                else
                                {
                                    if (!found)
                                    {
                                        ProcessedVersesWithOutText.Add(
                                            new VerseHolder
                                            {
                                                ParagraphNo = extractedVerWithOutText.ParagraphNo,
                                                Verse = bookName + chapter + ":" + _prev,
                                                isReadAuthorized = isReadAuthorized
                                            });
                                    }
                                    found = false;
                                }
                            }
                            else
                            {
                                if (_prev != 0 && !found)// prev is ok but cur is not
                                {
                                    ProcessedVersesWithOutText.Add(
                                        new VerseHolder
                                        {
                                            ParagraphNo = extractedVerWithOutText.ParagraphNo,
                                            Verse = bookName + chapter + ":" + prev.Trim(),
                                            isReadAuthorized = isReadAuthorized
                                        });
                                }
                                else if (_prev == 0)
                                    ProcessedVersesWithOutText.Add(
                                        new VerseHolder
                                        {
                                            ParagraphNo = extractedVerWithOutText.ParagraphNo,
                                            Verse = bookName + chapter + ":" + prev.Trim(),
                                            isReadAuthorized = isReadAuthorized
                                        });

                                found = false;
                            }
                            prev = ver;
                        }
                        if (!found) // the last vers
                        {
                            ProcessedVersesWithOutText.Add(
                                new VerseHolder
                                {
                                    ParagraphNo = extractedVerWithOutText.ParagraphNo,
                                    Verse = bookName + chapter + ":" + prev.Trim(),
                                    isReadAuthorized = isReadAuthorized
                                });
                        }
                    }
                    else
                    {
                        ProcessedVersesWithOutText.Add(new
                                                           VerseHolder
                        {
                            ParagraphNo = extractedVerWithOutText.ParagraphNo,
                            Verse = bookName + chapter + ":" + splitBookChapterVers[1],
                            isReadAuthorized = isReadAuthorized
                        });
                    }
                }
            }
        }

        private string GetMappedBookName(string bookName)
        {
            bookName = bookName.Trim();
            var Map = new Dictionary<string, string>();
            
            Map.Add("ሉቃስ", "ሉቃስ");//Luke
            Map.Add("የሐዋርያት ሥራ", "ሥራ");//Actis
            Map.Add("ሮም", "ሮም"); //Romans
            Map.Add("ፊልጵስዩስ", "ፊልጵ.");//Philipians
            Map.Add("ቲቶ", "ቲቶ");//Titus
            Map.Add("ይሁዳ", "ይሁዳ");//Jude
            Map.Add("ራእይ", "ራእይ");//Revelations

            

            if (Map.ContainsKey(bookName)) // means no exception
                return Map[bookName] + " ";
            
            return (char.IsNumber(bookName[0]) ? bookName.Substring(0, 4) + ". " : bookName.Substring(0, 2) + ". ");
        }

        private void MergeVerseText()
        {
            string output = "";

            foreach (var singleVersWithOutText in ProcessedVersesWithOutText.OrderBy(p => Convert.ToInt32(p.ParagraphNo)))
            {
                string _singleVersWithOutText = singleVersWithOutText.Verse;
                var currentVersWithText = ListOfVersesWithText.Where(v => v.Verse == _singleVersWithOutText);
                currentVersWithText.All(v => v.isMerged = true);

                output += "\r\n[par " + singleVersWithOutText.ParagraphNo + "] " + singleVersWithOutText.Verse +
                                (singleVersWithOutText.isReadAuthorized ? ReadThisPhrase : "") + "\r\n" +
                                    (currentVersWithText.Any() ? currentVersWithText.First().VerseText : singleVersWithOutText.VerseText);
            }

            //Process all that has been skipped due to mis-matches . . . 
            if (!ListOfVersesWithText.Where(v => v.isMerged == false).Any())
                Warning = "No Skipped Verses . . . ";
            else
            {
                output += "\r\n\r\r\n\r\n ----------SKIPPED---------------\r\n";
                output += string.Join("\r\n",
                                      ListOfVersesWithText.Where(v => v.isMerged == false && v.ParagraphNo != "").OrderBy(
                                          v => Convert.ToInt32(v.ParagraphNo)).Select(
                                          v => "[" + v.ParagraphNo + "] " + v.Verse +
                                               (v.isReadAuthorized ? ReadThisPhrase : "") + "\r\n" + v.VerseText).
                                          ToArray());
                Warning = "There are few skipped Verses . . . ";
            }
            OutPutText = output;
        }
    }
}