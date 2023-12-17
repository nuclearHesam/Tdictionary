using System.Collections.Generic;

namespace WordsDBModelView
{
    public class Words
    {
        public string WordID { get; set; }
        public string English { get; set; }
        public string Persian { get; set; }
        public string Rate { get; set; }
        public string SourceUrl { get; set; }
    }

    public class Phonetics
    {
        public string WordID { get; set; }
        public string PhoneticID { get; set; }
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
        public string DefinitionID { get; set; }
        public string Text { get; set; }
        public string Example { get; set; }
    }
}

namespace WordsListedModelView
{
    public class Phonetic
    {
        public string WordID { get; set; }
        public string PhoneticID { get; set; }
        public string Text { get; set; }
        public string Audio { get; set; }
        public string Language { get; set; }
    }

    public class Definition
    {
        public string MeaningID { get; set; }
        public string DefinitionID { get; set; }
        public string definition { get; set; }
        public string Example { get; set; }
    }

    public class Meaning
    {
        public string WordID { get; set; }
        public string MeaningID { get; set; }
        public string PartOfSpeech { get; set; }
        public List<Definition> Definitions { get; set; }
    }

    public class Word
    {
        public string WordID { get; set; }
        public string English { get; set; }
        public string Persian { get; set; }
        public List<Meaning> Meanings { get; set; }
        public List<Phonetic> Phonetics { get; set; }
        public string Rate { get; set; }
        public string SourceUrl { get; set; }
    }
}