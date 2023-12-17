using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bd_lite
{
    public class Words
    {
        public string WordID { get; set; }
        public string English { get; set; }
        public string Persian { get; set; }
    }

    public class Phonetics
    {
        public string WordID { get; set; }
        public string Text { get; set; }
        public string Audio { get; set; }
        public string Language { get; set; }
    }

    public class Meanings
    {
        public string WordID { get; set; }
        public string PartOfSpeech { get; set; }
        public string MeaningID { get; set; }
    }

    public class Definitions
    {
        public string MeaningID { get; set; }
        public string Text { get; set; }
        public string Example { get; set; }
    }
}
