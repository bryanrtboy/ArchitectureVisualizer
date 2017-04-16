/*
 * Copyright (c) 2015, Roger Lew (rogerlew.gmail.com)
 * Date: 6/16/2015
 * License: BSD (3-clause license)
 * 
 * The project described was supported by NSF award number IIA-1301792
 * from the NSF Idaho EPSCoR Program and by the National Science Foundation.
 * 
 */


using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using System.Text;


namespace VTL.IO
{
    /// <summary>
    ///  Reads .csv files using from a Unity resource location
    ///  Oject is modeled after Python's csv.DictReader and contains
    ///  an IEnumerator yielding Dictionary<string, string> objects
    ///  containing header keys and values from the row
    /// </summary>
    public class DictReader : IEnumerable
    {
        string[] header;
        List<Dictionary<string, string>> _lines;

        public static string LoadTextResource(string resourceLocation)
        {
            // Using Resources.Load to try to make this standalone/multi-platform friendly
            TextAsset theTextFile = Resources.Load<TextAsset>(resourceLocation);

            if (theTextFile == null)
            {
                Debug.LogError("Could not open text resource:" + resourceLocation);
                return string.Empty;
            }

            return theTextFile.text;

        }

        static string[] SplitCsvLine(string line, bool appendEmpty=true)
        {
            string pattern = @"(((?<x>(?=[,\r\n]+))|""(?<x>([^""]|"""")+)""|(?<x>[^,\r\n]+)),?)";
              
            // Instantiate the regular expression object.
            Regex r = new Regex(pattern, RegexOptions.IgnoreCase);

            // Match the regular expression pattern against a text string.
            Match m = r.Match(line);

            var tokens = new List<string>();
            while (m.Success)
            {
                Group g = m.Groups[2];
                CaptureCollection cc = g.Captures;

                string c = cc[0].ToString().Trim();

                // If token is double-quoted then remove the quotes
                if (c.Length >= 2)
                    if (c[0] == '"' && c[c.Length - 1] == '"')
                        c = c.Substring(1, c.Length - 2);

                if (appendEmpty)
                    tokens.Add(c);
                else if (c.Length > 0)
                    tokens.Add(c);

                m = m.NextMatch();
            }

            return tokens.ToArray();
        }

        public DictReader(string resourceLocation)
        {
            string text = LoadTextResource(resourceLocation);
            string[] lines = text.Split(new char[] { '\n' });

            string[] header = SplitCsvLine(lines[0], false);

            _lines = new List<Dictionary<string, string>>();
            for (int i = 1; i < lines.Length; i++)
            {
                string[] tokens = SplitCsvLine(lines[i]);

                if ((tokens.Length == 0) && (i + 1 == lines.Length))
                    break; // empty last line (what Excel does by default)
                else if (tokens.Length < header.Length)
                {
                    var error = string.Format("DictReader Parsing Error: Expecting {0} cells, found {1}, on line {2} or {3}",
                                              header.Length, tokens.Length, i + 1, lines.Length);
                    Debug.LogError(error);
                    continue;
                }

                _lines.Add(new Dictionary<string, string>());
                for (int j = 0; j < header.Length; j++)
                {
                    _lines[i - 1].Add(header[j], tokens[j]);
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("[ ");
            for ( int i = 0; i < _lines.Count; i++ )
            {
                sb.Append("  { ");
                int j = 0;
                foreach (var item in _lines[i])
                {
                    sb.Append(string.Format("{0}={1}", item.Key, item.Value));

                    if (j < _lines[i].Count - 1)
                        sb.Append(", ");

                    j++;
                }
                sb.Append(" }");

                if (i < _lines.Count - 1)
                    sb.Append(",");

                sb.AppendLine();
            }
            sb.AppendLine("]");

            return sb.ToString();
        }

        // Implementation for the GetEnumerator method.
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        public DictReaderEnum GetEnumerator()
        {
            return new DictReaderEnum(_lines);
        }
    }

    // When you implement IEnumerable, you must also implement IEnumerator. 
    public class DictReaderEnum : IEnumerator
    {
        public List<Dictionary<string, string>> _lines;

        // Enumerators are positioned before the first element 
        // until the first MoveNext() call. 
        int position = -1;

        public DictReaderEnum(List<Dictionary<string, string>> list)
        {
            _lines = list;
        }

        public bool MoveNext()
        {
            position++;
            return (position < _lines.Count);
        }

        public void Reset()
        {
            position = -1;
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public Dictionary<string, string> Current
        {
            get
            {
                try
                {
                    return _lines[position];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }
}