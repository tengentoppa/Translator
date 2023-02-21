

using Newtonsoft.Json;
using Translator.Helper.Google.Dto;

namespace Translator.Helper.Google.Translation
{
    public class TranslationHelper
    {
        private readonly string _url;
        private readonly string _translationKey;
        private readonly HttpClient _httpClient;
        public TranslationHelper(string url, string traslationKey)
        {
            _url = url;
            _translationKey = traslationKey;
            _httpClient = new HttpClient();
        }

        public async Task<string> GetTranslate(string text)
        {
            var format = "text";
            var source = "zh-CN";
            var target = "en-GB";
            var rsp = await _httpClient.PostAsync(
                $"{_url}?format={format}&source={source}&target={target}&key={_translationKey}&q={text}",
                new ByteArrayContent(Array.Empty<byte>())
            );
            var rspText = await rsp.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<TranslationDto>(rspText);
            return result.Data.Translations.First().TranslatedText;
        }
    }
}
