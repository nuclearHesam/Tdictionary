using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using tdic.DataContext;
using tdic.InternetConnectionayer.Downloader;
using tdic.InternetConnectionLayer.WebAPI;
using tdic.ModelViews;
using tdic.SettingJson;
using tdic.WordsRepository;
using WordsDBModelView;
using WordsListedModelView;

namespace tdic
{
    /// <summary>
    /// Interaction logic for AddWord.xaml
    /// </summary>
    public partial class AddWord : Window
    {
        public AddWord(bool IsEditable = false, string WordID = "")
        {
            InitializeComponent();
            this.IsEditable = IsEditable;
            this.WordID = WordID;
        }
        List<Word> words = new();
        Word word = new();
        List<Phonetic> phonetics = new();
        List<Meaning> meanings = new() {
            new Meaning { PartOfSpeech = "pronoun",Definitions = new List<Definition>() },
            new Meaning { PartOfSpeech = "verb",Definitions = new List<Definition>() },
            new Meaning { PartOfSpeech = "noun",Definitions = new List<Definition>() },
            new Meaning { PartOfSpeech = "adjective",Definitions = new List<Definition>() },
            new Meaning { PartOfSpeech = "adverb",Definitions = new List<Definition>() },
            new Meaning { PartOfSpeech = "preposition",Definitions = new List<Definition>() },
            new Meaning { PartOfSpeech = "conjunction",Definitions = new List<Definition>() },
            new Meaning { PartOfSpeech = "interjection",Definitions = new List<Definition>() },
            };
        Meaning meaning = new() { Definitions = new List<Definition>() };
        int from = 0, fromword = 0;
        private bool _ISFirstTime = false;
        private bool isRunTask = false;
        private readonly bool IsEditable = false;
        private bool isEmptyDefinition = true;
        private string AudioFolder;
        private string sourceUrl;
        private readonly string WordID;

        #region MessageBox Text

        MessageBoxOptions mbo_MessageBoxLanguage = new();

        readonly MessageBoxLanguage definitionIsEmpty = new();
        readonly MessageBoxLanguage InternetConnectionError = new();
        readonly MessageBoxLanguage WordNotFound = new();
        readonly MessageBoxLanguage PleaseEnterTheWord = new();
        readonly MessageBoxLanguage findedWordCount = new();
        readonly MessageBoxLanguage title = new();
        readonly MessageBoxLanguage SearchingInternet = new();
        readonly MessageBoxLanguage mbl_downloadaudio = new();
        readonly MessageBoxLanguage mbl_Recordaudio = new();
        readonly MessageBoxLanguage mbl_WordValidation = new();
        readonly MessageBoxLanguage mbl_WordExist = new();
        readonly MessageBoxLanguage mbl_WordSaved = new();
        readonly MessageBoxLanguage mbl_EnterWord = new();
        readonly MessageBoxLanguage mbl_CancelProgress = new();

        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            back_English_btn.Visibility = Visibility.Hidden;
            next_English_btn.Visibility = Visibility.Hidden;
            back_Persian_btn.Visibility = Visibility.Hidden;
            next_Persian_btn.Visibility = Visibility.Hidden;
            uk_speaker_btn.IsEnabled = false;
            us_speaker_btn.IsEnabled = false;
            ca_speaker_btn.IsEnabled = false;

            var Settingslang = Serializer.ReadSettingJson();
            ChangeLanguageSetting(Settingslang.LanguageSettings);

            AudioFolder = Directory.GetCurrentDirectory() + "\\audios";
            if (!Directory.Exists(AudioFolder))
            {
                Directory.CreateDirectory(AudioFolder);
            }

            if (IsEditable)
            {
                using (UnitOfWork db = new())
                {
                    var worddb = db.WordsRepository.ReadWord(WordID);
                    var phoneticsdb = db.WordsRepository.ReadPhonetics(WordID);
                    var meaningsdb = db.WordsRepository.ReadMeanings(WordID);
                    List<Definitions> definitionsdb = new();
                    foreach (var meaning in meaningsdb)
                    {
                        var definition = db.WordsRepository.ReadDefinitions(meaning.MeaningID);
                        foreach (var item in definition)
                        {
                            definitionsdb.Add(item);
                        }
                    }
                    var word = ListedModelConvertor.WordsConvertor(worddb, phoneticsdb, meaningsdb, definitionsdb);
                    this.word = word;
                }
                SetSearchedWord(this.word);
                SetPhonetics();
            }
        }

        private async void Search_btn_Click(object sender, RoutedEventArgs e)
        {
            if (word_English_txt.Text.Trim().Length != 0)
            {
                try
                {
                    this.Title = SearchingInternet.Caption;

                    words = await OnlineDictionary.GetWordFromAPI(word_English_txt.Text.Trim());

                    this.Title = $"{title.Caption}";

                    if (words.Count != 1)
                    {
                        back_English_btn.Visibility = Visibility.Visible;
                        next_English_btn.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        back_English_btn.Visibility = Visibility.Hidden;
                        next_English_btn.Visibility = Visibility.Hidden;
                    }

                    System.Windows.MessageBox.Show(words.Count + findedWordCount.Text, "", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information, MessageBoxResult.None, mbo_MessageBoxLanguage);

                    SetSearchedWord(words[fromword]);
                    SetPhonetics();
                }
                catch (Exception ex)
                {
                    this.Title = $"{title.Caption}";
                    InternetExeptions(ex);
                }
            }
            else
            {
                System.Windows.MessageBox.Show(PleaseEnterTheWord.Text, PleaseEnterTheWord.Caption, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error, MessageBoxResult.None, mbo_MessageBoxLanguage);
            }
        }

        void SetSearchedWord(Word word)
        {
            word_English_txt.Text = word.English;
            word_Persian_txt.Text = word.Persian;
            sourceUrl = word.SourceUrl;

            foreach (var meaning in meanings)
            {
                meaning.Definitions.Clear();
            }
            phonetics.Clear();

            foreach (var meaning1 in word.Meanings)
            {
                var meaning = meanings.Find(mean => mean.PartOfSpeech == meaning1.PartOfSpeech);
                foreach (var definition in meaning1.Definitions)
                {
                    meaning.Definitions.Add(definition);
                }
            }

            DefinitionCount_txt.Text = meaning.Definitions.Count.ToString();
            Definition_txt.Text = example_txt.Text = "";

            //
            if (meaning.Definitions.Count >= 1)
            {
                Definition_txt.Text = meaning.Definitions[0].definition.ToString();
                if (meaning.Definitions[0].Example != null)
                {
                    example_txt.Text = meaning.Definitions[0].Example.ToString();
                }
            }

            var uk = word.Phonetics.Find(ph => ph.Language == "uk");
            if (uk != null)
            {
                uk_speaker_btn.IsEnabled = true;
                phonetics.Add(uk);
                uk_txt.Text = uk.Text;
            }
            else
            {
                uk_txt.Text = "";
                uk_speaker_btn.IsEnabled = false;
            }

            var us = word.Phonetics.Find(ph => ph.Language == "us");
            if (us != null)
            {
                us_speaker_btn.IsEnabled = true;
                phonetics.Add(us);
                us_txt.Text = us.Text;
            }
            else
            {
                us_txt.Text = "";
                us_speaker_btn.IsEnabled = false;
            }

            var ca = word.Phonetics.Find(ph => ph.Language == "au");
            if (ca != null)
            {
                ca_speaker_btn.IsEnabled = true;
                phonetics.Add(ca);
                ca_txt.Text = ca.Text;
            }
            else
            {
                ca_txt.Text = "";
                ca_speaker_btn.IsEnabled = false;
            }
        }

        async void SetPhonetics()
        {
            this.Title = mbl_downloadaudio.Caption;
            isRunTask = true;

            foreach (var phonetic in phonetics)
            {
                if (!File.Exists(phonetic.Audio) && phonetic.Audio != null)
                {
                    string[] audioname = phonetic.Audio.Split('/');
                    string AudioPath = AudioFolder + "\\" + audioname[6];

                    if (!File.Exists(AudioPath))
                    {
                        if (await AudioDownloadManager.DownloadAudio(phonetic.Audio, AudioPath))
                        {
                            phonetic.Audio = AudioPath;
                        }
                    }
                    else
                    {
                        phonetic.Audio = AudioPath;
                    }
                }
            }

            this.Title = $"{title.Caption}";
            isRunTask = false;
        }

        void EnableSpeakers()
        {
            var uk = phonetics.Find(ph => ph.Language == "uk");
            if (uk != null)
            {
                if (uk.Audio != "")
                {
                    uk_speaker_btn.IsEnabled = true;
                }
                else
                {
                    uk_speaker_btn.IsEnabled = false;
                }
            }
            else
            {
                uk_speaker_btn.IsEnabled = false;
            }

            var us = phonetics.Find(ph => ph.Language == "us");
            if (us != null)
            {
                if (us.Audio != "")
                {
                    us_speaker_btn.IsEnabled = true;
                }
                else
                {
                    us_speaker_btn.IsEnabled = false;
                }
            }
            else
            {
                us_speaker_btn.IsEnabled = false;
            }

            var ca = phonetics.Find(ph => ph.Language == "au");
            if (ca != null)
            {
                if (ca.Audio != "")
                {
                    ca_speaker_btn.IsEnabled = true;
                }
                else
                {
                    ca_speaker_btn.IsEnabled = false;
                }
            }
            else
            {
                ca_speaker_btn.IsEnabled = false;
            }
        }

        #region word

        private void Back_English_btn_Click(object sender, RoutedEventArgs e)
        {
            if (words.Count != 1)
            {
                if (fromword != 0)
                {
                    fromword--;
                    ResetValues();
                    SetSearchedWord(words[fromword]);
                    SetPhonetics();
                }
            }
        }

        void ResetValues()
        {
            meanings = new List<Meaning>() {
            new Meaning { PartOfSpeech = "pronoun",Definitions = new List<Definition>() },
            new Meaning { PartOfSpeech = "verb",Definitions = new List<Definition>() },
            new Meaning { PartOfSpeech = "noun",Definitions = new List<Definition>() },
            new Meaning { PartOfSpeech = "adjective",Definitions = new List<Definition>() },
            new Meaning { PartOfSpeech = "adverb",Definitions = new List<Definition>() },
            new Meaning { PartOfSpeech = "preposition",Definitions = new List<Definition>() },
            new Meaning { PartOfSpeech = "conjunction",Definitions = new List<Definition>() },
            new Meaning { PartOfSpeech = "interjection",Definitions = new List<Definition>() },
            };
            meaning = new Meaning { Definitions = new List<Definition>() };
            phonetics = new List<Phonetic>();
        }

        private void Next_English_btn_Click(object sender, RoutedEventArgs e)
        {
            if (words.Count != 1)
            {
                if ((fromword + 1) < words.Count)
                {
                    fromword++;
                    ResetValues();
                    SetSearchedWord(words[fromword]);
                    SetPhonetics();
                }
            }
        }

        #endregion

        #region Meaning

        private void Back_definition_btn_Click(object sender, RoutedEventArgs e)
        {
            if ((from - 1) != -1)
            {
                SaveDefinition();
                DeleteNullsDefinition();
                from--;
                Definition_txt.Text = meaning.Definitions[from].definition;
                example_txt.Text = meaning.Definitions[from].Example;
                DefinitionCount_txt.Text = (from + 1) + "/" + (meaning.Definitions.Count).ToString();
            }
        }

        private void Add_definition_btn_Click(object sender, RoutedEventArgs e)
        {
            if (Definition_txt.Text != "")
            {
                if (from == meaning.Definitions.Count)
                {
                    meaning.Definitions.Add(new Definition { definition = Definition_txt.Text.Trim(), Example = example_txt.Text.Trim() });
                    from++;
                    Definition_txt.Text = example_txt.Text = "";
                    DefinitionCount_txt.Text = (from + 1) + "/" + (meaning.Definitions.Count + 1).ToString();
                }
                else
                {
                    SaveDefinition();
                    from = meaning.Definitions.Count;
                    DefinitionCount_txt.Text = (from + 1) + "/" + (meaning.Definitions.Count + 1).ToString();
                    Definition_txt.Text = example_txt.Text = "";
                }
            }
            else
            {
                System.Windows.MessageBox.Show(definitionIsEmpty.Text, definitionIsEmpty.Caption, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error, MessageBoxResult.None, mbo_MessageBoxLanguage);
            }
        }

        private void Next_definition_btn_Click(object sender, RoutedEventArgs e)
        {
            if ((from + 1) < meaning.Definitions.Count)
            {
                SaveDefinition();
                DeleteNullsDefinition();
                if (isEmptyDefinition)
                {
                    from++;
                }
                isEmptyDefinition = true;
                Definition_txt.Text = meaning.Definitions[from].definition;
                example_txt.Text = meaning.Definitions[from].Example;
                DefinitionCount_txt.Text = (from + 1) + "/" + (meaning.Definitions.Count).ToString();
            }
        }

        private void Pos_txt_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_ISFirstTime)
            {
                SaveDefinition();
                DeleteNullsDefinition();
            }

            var meaning1 = meanings.Find(mean => mean.PartOfSpeech == meaning.PartOfSpeech);
            if (meaning1 != null)
            {
                meaning1 = meaning;
            }

            meaning = meanings.Find(meaning => meaning.PartOfSpeech == pos_txt.SelectedItem.ToString().Split(':')[1].Trim());

            from = 0;
            if (meaning.Definitions.Count != 0)
            {
                Definition_txt.Text = meaning.Definitions[0].definition;
                example_txt.Text = meaning.Definitions[0].Example;
                DefinitionCount_txt.Text = (from + 1) + "/" + (meaning.Definitions.Count).ToString();
            }
            else
            {
                if (_ISFirstTime)
                {
                    Definition_txt.Text = example_txt.Text = "";
                    DefinitionCount_txt.Text = (from + 1) + "/" + (meaning.Definitions.Count + 1).ToString();
                }
            }

            _ISFirstTime = true;
        }

        void SaveDefinition()
        {
            if (from == meaning.Definitions.Count)
            {
                meaning.Definitions.Add(new Definition { definition = Definition_txt.Text.Trim(), Example = example_txt.Text.Trim() });
            }
            meaning.Definitions[from].definition = Definition_txt.Text.Trim();
            meaning.Definitions[from].Example = example_txt.Text.Trim();
        }

        void DeleteNullsDefinition()
        {
            var definitionList = meaning.Definitions.FindAll(def => def.definition == "" && def.Example == "");
            foreach (var definition in definitionList)
            {
                isEmptyDefinition = false;
                meaning.Definitions.Remove(definition);
            }
        }

        #endregion

        #region speakers Bottons

        private void Us_speaker_btn_Click(object sender, RoutedEventArgs e)
        {
            AudioPlayer("us");
        }

        private void Uk_speaker_btn_Click(object sender, RoutedEventArgs e)
        {
            AudioPlayer("uk");
        }

        private void Ca_speaker_btn_Click(object sender, RoutedEventArgs e)
        {
            AudioPlayer("au");
        }

        void AudioPlayer(string language)
        {
            MediaPlayer mediaPlayer = new();

            try
            {
                var AudioDirectory = phonetics.Find(ph => ph.Language == language).Audio;
                if (File.Exists(AudioDirectory))
                {
                    mediaPlayer.Open(new Uri(AudioDirectory));

                    mediaPlayer.Play();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error: " + ex.Message);
            }
        }

        #endregion

        #region Mics Buttons

        readonly string micOnImage = "D:\\Study\\MyAPP\\Project\\tdic\\tdic\\Images\\AddWord\\mic-on.png";
        readonly string micOffImage = "D:\\Study\\MyAPP\\Project\\tdic\\tdic\\Images\\AddWord\\mic-off.png";

        private void Us_mic_btn_Click(object sender, RoutedEventArgs e)
        {
            if (WordValidation())
            {
                us_mic_btn.Background = ChangeMicButtonIcon(micOnImage);
                this.Title = mbl_Recordaudio.Caption;

                var AudioPath = AudioFolder + "\\" + word_English_txt.Text.Trim() + "-us.wav";

                AudioRecorder(AudioPath);

                if (isRecording)
                {
                    us_mic_btn.Background = ChangeMicButtonIcon(micOffImage);
                    this.Title = title.Caption;

                    var phonetic = phonetics.FirstOrDefault(phonetic => phonetic.Language == "us");
                    if (phonetic != null)
                    {
                        phonetic.Audio = AudioPath;
                    }
                    else
                    {
                        phonetics.Add(new Phonetic { Language = "us", Audio = AudioPath });
                    }
                    EnableSpeakers();
                }
            }
        }

        private void Uk_mic_btn_Click(object sender, RoutedEventArgs e)
        {
            if (WordValidation())
            {
                uk_mic_btn.Background = ChangeMicButtonIcon(micOnImage);
                this.Title = mbl_Recordaudio.Caption;

                var AudioPath = AudioFolder + "\\" + word_English_txt.Text.Trim() + "-uk.wav";

                AudioRecorder(AudioPath);

                if (isRecording)
                {
                    uk_mic_btn.Background = ChangeMicButtonIcon(micOffImage);
                    this.Title = title.Caption;

                    var phonetic = phonetics.FirstOrDefault(phonetic => phonetic.Language == "uk");
                    if (phonetic != null)
                    {
                        phonetic.Audio = AudioPath;
                    }
                    else
                    {
                        phonetics.Add(new Phonetic { Language = "uk", Audio = AudioPath });
                    }
                    EnableSpeakers();

                }
            }
        }

        private void Ca_mic_btn_Click(object sender, RoutedEventArgs e)
        {
            if (WordValidation())
            {
                ca_mic_btn.Background = ChangeMicButtonIcon(micOnImage);
                this.Title = mbl_Recordaudio.Caption;

                var AudioPath = AudioFolder + "\\" + word_English_txt.Text.Trim() + "-au.wav";

                AudioRecorder(AudioPath);

                if (isRecording)
                {
                    ca_mic_btn.Background = ChangeMicButtonIcon(micOffImage);
                    this.Title = title.Caption;

                    var phonetic = phonetics.FirstOrDefault(phonetic => phonetic.Language == "au");
                    if (phonetic != null)
                    {
                        phonetic.Audio = AudioPath;
                    }
                    else
                    {
                        phonetics.Add(new Phonetic { Language = "au", Audio = AudioPath });
                    }
                    EnableSpeakers();
                }
            }
        }

        #region Audio Recorder

        static WaveInEvent? waveSource;
        static WaveFileWriter? waveFile;
        readonly int deviceNumber = 0;
        readonly int sampleRate = 44100;
        readonly int channels = 2;
        bool isRecording = true;

        void AudioRecorder(string path)
        {
            try
            {
                if (isRecording)
                {
                    waveSource = new WaveInEvent
                    {
                        DeviceNumber = deviceNumber,
                        WaveFormat = new WaveFormat(sampleRate, channels)
                    };
                    waveSource.DataAvailable += new EventHandler<WaveInEventArgs>(WaveSource_DataAvailable);
                    waveSource.RecordingStopped += new EventHandler<StoppedEventArgs>(WaveSource_RecordingStopped);

                    waveFile = new WaveFileWriter(path, waveSource.WaveFormat);

                    waveSource.StartRecording();
                    isRecording = false;
                }
                else
                {
                    waveSource.StopRecording();
                    waveFile.Close();

                    isRecording = true;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error" + ex.Message);
            }
        }

        private static void WaveSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            waveFile.Write(e.Buffer, 0, e.BytesRecorded);
            waveFile.Flush();
        }

        private static void WaveSource_RecordingStopped(object sender, StoppedEventArgs e)
        {
            waveSource.Dispose();
            waveFile.Dispose();
        }

        #endregion

        bool WordValidation()
        {
            if (word_English_txt.Text.Trim() != "")
            {
                return true;
            }
            else
            {
                System.Windows.MessageBox.Show(mbl_WordValidation.Text, mbl_WordValidation.Caption, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Exclamation, MessageBoxResult.None, mbo_MessageBoxLanguage);
                return false;
            }
        }

        static ImageBrush ChangeMicButtonIcon(string path)
        {
            BitmapImage image = new();
            image.BeginInit();
            image.UriSource = new Uri(path);
            image.EndInit();

            ImageBrush imageBrush = new()
            {
                ImageSource = image
            };

            return imageBrush;
        }

        #endregion

        #region Delete Buttons

        private void Us_delete_btn_Click(object sender, RoutedEventArgs e)
        {
            var us = phonetics.Find(ph => ph.Language == "us");
            if (us != null)
            {
                phonetics.Remove(us);
                us_txt.Text = "";
                us_speaker_btn.IsEnabled = false;
            }
        }

        private void Au_delete_btn_Click(object sender, RoutedEventArgs e)
        {
            var au = phonetics.Find(ph => ph.Language == "au");
            if (au != null)
            {
                phonetics.Remove(au);
                ca_txt.Text = "";
                ca_speaker_btn.IsEnabled = false;
            }
        }

        private void Uk_delete_btn_Click(object sender, RoutedEventArgs e)
        {
            var uk = phonetics.Find(ph => ph.Language == "uk");
            if (uk != null)
            {
                phonetics.Remove(uk);
                uk_txt.Text = "";
                uk_speaker_btn.IsEnabled = false;
            }
        }

        #endregion

        #region translate

        private async void Translate_btn_Click(object sender, RoutedEventArgs e)
        {
            if (word_English_txt.Text.Length != 0)
            {
                try
                {
                    this.Title = SearchingInternet.Caption;

                    word_Persian_txt.Text = await OnlineTranslate.Translate(word_English_txt.Text.Trim(), "fa-IR");

                    this.Title = $"{title.Caption}";

                }
                catch (Exception ex)
                {
                    this.Title = $"{title.Caption}";
                    InternetExeptions(ex);
                }
            }
            else
            {
                System.Windows.MessageBox.Show(PleaseEnterTheWord.Text, PleaseEnterTheWord.Caption, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error, MessageBoxResult.None, mbo_MessageBoxLanguage);
            }
        }

        private void Translate_btn_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("translate will be open...");
            // show translate page
        }

        #endregion

        private void ChangeLanguageSetting(LanguageSettings languageSettings)
        {
            if (languageSettings.MainLanguage != "null")
            {
                _Page(languageSettings.MainLanguage);
                _ButtonsLanguage(languageSettings.MainLanguage);
                _TextBlockLanguage(languageSettings.MainLanguage);
                _MessageBoxLanguage(languageSettings.MainLanguage);
            }
            else
            {
                _ButtonsLanguage(languageSettings.ButtonsLanguage);
                _TextBlockLanguage(languageSettings.TextBlockLanguage);
                _MessageBoxLanguage(languageSettings.MessageBoxLanguage);
            }

            void _Page(string Language)
            {
                if (Language == "English")
                {
                    mbl_downloadaudio.Caption = "Downloading Audio...";
                    SearchingInternet.Caption = "Searching from Internet...";
                    mbl_Recordaudio.Caption = "Recording...";
                    title.Caption = "Add new word";
                    this.Title = "Add new word";
                    Meanings_gbx.Header = "Meanings";
                    Phonetics_gbx.Header = "Phonetics";
                }
                else if (languageSettings.ButtonsLanguage == "Persian")
                {
                    mbl_downloadaudio.Caption = "در حال دانلود صدا...";
                    SearchingInternet.Caption = "جستجو از اینترنت...";
                    mbl_Recordaudio.Caption = "در حال ظبط کردن...";
                    title.Caption = "افزودن کلمه جدید";
                    this.Title = "افزودن کلمه جدید";
                    Meanings_gbx.Header = "معانی";
                    Phonetics_gbx.Header = "تلفظ ها";
                }
            }

            void _ButtonsLanguage(string Language)
            {
                if (Language == "English")
                {
                    back_definition_btn.Content = "< Previous";
                    next_definition_btn.Content = "Next >";
                    btnReset.Content = "Reset";
                    Save_btn.Content = "Save";
                }
                else if (languageSettings.ButtonsLanguage == "Persian")
                {
                    back_definition_btn.Content = "< قبلی";
                    next_definition_btn.Content = "بعدی >";
                    btnReset.Content = "بازنشانی";
                    Save_btn.Content = "ذخیره";
                }
            }

            void _TextBlockLanguage(string Language)
            {
                if (Language == "English")
                {
                    pos_txb.Text = "PartOfSpeech:";
                }
                else if (languageSettings.TextBlockLanguage == "Persian")
                {
                    pos_txb.Text = "دستور زبان:";
                }
            }

            void _MessageBoxLanguage(string Language)
            {
                if (Language == "English")
                {
                    mbo_MessageBoxLanguage = MessageBoxOptions.None;

                    definitionIsEmpty.Caption = "Please enter a definition!";
                    definitionIsEmpty.Text = "Meaning cannot be without definition!";

                    InternetConnectionError.Caption = "Problems connecting to the Internet!";
                    InternetConnectionError.Text = "Please check your internet";

                    WordNotFound.Caption = "The desired word was not found!";
                    WordNotFound.Text = "Please enter the word correctly";

                    PleaseEnterTheWord.Caption = "The word cannot be empty!";
                    PleaseEnterTheWord.Text = "Please enter the word";

                    findedWordCount.Text = " word was found";

                    mbl_WordValidation.Caption = "Please enter the word first.";
                    mbl_WordValidation.Text = "The word cannot be empty!";

                    mbl_WordExist.Caption = " Existed!";
                    mbl_WordExist.Text = "Do you want to add again?";

                    mbl_WordSaved.Caption = "Word Saved!";
                    mbl_WordSaved.Text = "Do You Want Go to ";

                    mbl_EnterWord.Caption = "Please Enter Word";
                    mbl_EnterWord.Text = "Word cannot Be Empty!";

                    mbl_CancelProgress.Caption = "Please wait!";
                    mbl_CancelProgress.Text = "An operation is in progress. Do you want to stop it?";
                }
                else if (languageSettings.MessageBoxLanguage == "Persian")
                {
                    mbo_MessageBoxLanguage = MessageBoxOptions.RtlReading;

                    definitionIsEmpty.Caption = "لطفا یک تعریف وارد کنید!";
                    definitionIsEmpty.Text = "معنی نمی تواند بدون تعریف باشد!";

                    InternetConnectionError.Caption = "اشکال در برقراری ارتباط به اینترنت!";
                    InternetConnectionError.Text = "لطفا اینترنت خود را بررسی کنید";

                    WordNotFound.Caption = "کلمه مورد نظر پیدا نشد!";
                    WordNotFound.Text = "لطفا کلمه را درست وارد کنید";

                    PleaseEnterTheWord.Caption = "کلمه نمی تواند خالی باشد!";
                    PleaseEnterTheWord.Text = "لطفا کلمه را وارد کنید";

                    findedWordCount.Text = "کلمه پیدا شد";

                    mbl_WordValidation.Caption = "لطفا اول کلمه را وارد کنید.";
                    mbl_WordValidation.Text = "کلمه نمی تواند خالی باشد!";

                    mbl_WordExist.Caption = " وجود دارد!";
                    mbl_WordExist.Text = "آیا می خواهید دوباره اضافه کنید؟";

                    mbl_WordSaved.Caption = "کلمه ذخیره شد!";
                    mbl_WordSaved.Text = "آیا می خواهید بروید به ";

                    mbl_EnterWord.Caption = "لطفا کلمه رو وارد کنید";
                    mbl_EnterWord.Text = "کلمه نمیتواند خالی باشد!";

                    mbl_CancelProgress.Caption = "لطفا صبر کنید!";
                    mbl_CancelProgress.Text = "عملیاتی در حال انجام است. آیا می خواهید آن را متوقف کنید؟";
                }
            }
        }

        private void InternetExeptions(Exception ex)
        {
            if (ex.Message == "Not Found")
            {
                System.Windows.MessageBox.Show(WordNotFound.Text, WordNotFound.Caption, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error, MessageBoxResult.None, mbo_MessageBoxLanguage);
            }
            else if (ex.Message == "The remote name could not be resolved")
            {
                System.Windows.MessageBox.Show(InternetConnectionError.Text, InternetConnectionError.Caption, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error, MessageBoxResult.None, mbo_MessageBoxLanguage);
            }
            else
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void ResetContent()
        {
            back_English_btn.Visibility = Visibility.Hidden;
            next_English_btn.Visibility = Visibility.Hidden;
            uk_txt.Text = us_txt.Text = ca_txt.Text = "";
            uk_speaker_btn.IsEnabled = false;
            us_speaker_btn.IsEnabled = false;
            ca_speaker_btn.IsEnabled = false;
            from = 0; fromword = 0;
            DefinitionCount_txt.Text = "1/1";
            Definition_txt.Text = example_txt.Text = "";
            word_English_txt.Text = "";
            word_Persian_txt.Text = "";
        }

        private void Save_btn_Click(object sender, RoutedEventArgs e)
        {
            if (word_English_txt.Text.Trim() != "")
            {
                if (isRunTask)
                {
                    if (MessageBox.Show(mbl_CancelProgress.Text, mbl_CancelProgress.Caption, MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.No, mbo_MessageBoxLanguage) == MessageBoxResult.Yes)
                    {
                        return;
                    }
                }

                if (IsEditable)
                {
                    SaveDefinition();
                    DeleteNullsDefinition();

                    // set phonetics
                    foreach (var phonetic in this.phonetics)
                    {
                        if (phonetic.Language == "uk")
                        {
                            phonetic.Text = uk_txt.Text.Trim();
                        }
                        else if (phonetic.Language == "us")
                        {
                            phonetic.Text = us_txt.Text.Trim();
                        }
                        else if (phonetic.Language == "au")
                        {
                            phonetic.Text = ca_txt.Text.Trim();
                        }

                        if (phonetic.WordID == null)
                        {
                            phonetic.WordID = this.WordID;
                            phonetic.PhoneticID = Guid.NewGuid().ToString();
                        }
                    }

                    var meanings = this.meanings.FindAll(meaning => meaning.Definitions.Count != 0);

                    // set meanings
                    foreach (var meaning in meanings)
                    {
                        var mean = this.word.Meanings.Find(mean => mean.PartOfSpeech == meaning.PartOfSpeech);
                        if (mean != null)
                        {
                            meaning.MeaningID = mean.MeaningID;
                            meaning.WordID = this.word.WordID;

                            foreach (var definition in mean.Definitions)
                            {
                                var def = mean.Definitions.Find(def => def.DefinitionID == definition.DefinitionID);
                                if (definition != null)
                                {
                                    definition.MeaningID = meaning.MeaningID;
                                    definition.DefinitionID = def.DefinitionID;
                                }
                                else
                                {
                                    definition.MeaningID = meaning.MeaningID;
                                    definition.DefinitionID = Guid.NewGuid().ToString();
                                }
                            }
                        }
                        else
                        {
                            meaning.WordID = this.word.WordID;
                            meaning.MeaningID = Guid.NewGuid().ToString();

                            foreach (var definition in meaning.Definitions)
                            {
                                definition.MeaningID = meaning.MeaningID;
                                definition.DefinitionID = Guid.NewGuid().ToString();
                            }
                        }
                    }

                    // set word
                    Word word = new()
                    {
                        WordID = this.word.WordID,
                        English = word_English_txt.Text.Trim(),
                        Persian = word_Persian_txt.Text.Trim(),
                        Meanings = meanings,
                        Phonetics = this.phonetics,
                        SourceUrl = sourceUrl
                    };

                    Words Words = DbModelConvertor.WordConvertor(word);
                    List<Phonetics> Phonetics = DbModelConvertor.PhoneticsConvertor(word.Phonetics);
                    List<Meanings> Meanings = DbModelConvertor.MeaningsConvertor(word.Meanings);
                    List<Definitions> Definitions = DbModelConvertor.Definitions;

                    using (UnitOfWork db = new())
                    {
                        db.WordsRepository.UpdateWord(Words);
                        db.WordsRepository.UpdatePhonetics(Phonetics);
                        db.WordsRepository.UpdateMeanings(Meanings);
                        db.WordsRepository.UpdateDefinitions(Definitions);
                    }
                    MessageBox.Show("Success");

                }
                else
                {
                    SaveDefinition();
                    DeleteNullsDefinition();

                    // set phonetic
                    foreach (var item in phonetics)
                    {
                        if (item.Language == "uk")
                        {
                            item.Text = uk_txt.Text.Trim();
                        }
                        else if (item.Language == "us")
                        {
                            item.Text = us_txt.Text.Trim();
                        }
                        else if (item.Language == "au")
                        {
                            item.Text = ca_txt.Text.Trim();
                        }
                    }

                    // set word
                    Word word = new()
                    {
                        English = word_English_txt.Text.Trim(),
                        Persian = word_Persian_txt.Text.Trim(),
                        Meanings = meanings.FindAll(meaning => meaning.Definitions.Count != 0),
                        Phonetics = phonetics,
                        SourceUrl = sourceUrl
                    };

                    bool isExist;
                    using (UnitOfWork db = new())
                    {
                        isExist = db.WordsRepository.WordExist(word.English);
                    }

                    if (isExist)
                    {
                        if (System.Windows.MessageBox.Show(mbl_WordExist.Text, $"'{word.English}' " + mbl_WordExist.Caption, System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Information, MessageBoxResult.None, mbo_MessageBoxLanguage) == System.Windows.MessageBoxResult.Yes)
                        {
                            return;
                        }
                    }
                    else
                    {
                        string WordId = word.WordID = Guid.NewGuid().ToString();
                        foreach (var phonetic in word.Phonetics)
                        {
                            phonetic.PhoneticID = Guid.NewGuid().ToString();
                            phonetic.WordID = WordId;
                        }
                        foreach (var meaning in word.Meanings)
                        {
                            meaning.WordID = WordId;
                            meaning.MeaningID = Guid.NewGuid().ToString();
                            foreach (var definition in meaning.Definitions)
                            {
                                definition.DefinitionID = Guid.NewGuid().ToString();
                                definition.MeaningID = meaning.MeaningID;
                            }
                        }


                        var dbWord = word.WordConvertor();
                        var dbPhonetic = word.Phonetics.PhoneticsConvertor();
                        var dbMeaning = word.Meanings.MeaningsConvertor();
                        var dbDefinition = DbModelConvertor.Definitions;

                        using (UnitOfWork db = new())
                        {
                            db.WordsRepository.CreateWord(dbWord, dbPhonetic, dbMeaning, dbDefinition);
                        }

                        if (System.Windows.MessageBox.Show(mbl_WordSaved.Text + $"'{word.English}'?", mbl_WordSaved.Caption, System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Information, MessageBoxResult.None, mbo_MessageBoxLanguage) == System.Windows.MessageBoxResult.Yes)
                        {
                            WordPage wordPage = new(word.WordID);
                            wordPage.Show();
                            this.Close();
                        }
                        else
                        {
                            ResetContent();
                            ResetValues();
                        }
                    }
                }

            }
            else
            {
                MessageBox.Show(PleaseEnterTheWord.Text, PleaseEnterTheWord.Caption, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error, MessageBoxResult.None, mbo_MessageBoxLanguage);
            }
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            ResetContent();
            ResetValues();
        }

    }
}