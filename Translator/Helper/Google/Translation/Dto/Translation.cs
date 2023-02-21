namespace Translator.Helper.Google.Dto
{

    class TranslationDto : GoogleBaseRsp<Translation>
    {
    }
    class Translation
    {
        public TranslationObj[] Translations { get; set; }
        public class TranslationObj
        {
            public string TranslatedText { get; set; }
        }
    }
}
