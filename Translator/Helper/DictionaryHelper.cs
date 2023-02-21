
namespace Translator.Helper
{
    public class DictionaryHelper
    {
        private readonly string _path;
        private readonly string _splitText;
        public DictionaryHelper(string path, string splitText)
        {
            _path = path;
            _splitText = splitText;
        }

        public Dictionary<string, string> GetDictionary()
        {
            if (string.IsNullOrEmpty(_path) || !File.Exists(_path))
            {
                return new();
            }
            var text = File.ReadAllText(_path);
            try
            {
                return text
                    .Split("\r\n", StringSplitOptions.RemoveEmptyEntries)
                    .Select(d => d.Split(_splitText, StringSplitOptions.RemoveEmptyEntries))
                    .ToDictionary(d => d[0], d => d[1]);
            }
            catch
            {
                Console.WriteLine("Dictionary format invalid");
                return new();
            }
        }

        public void WriteBackToDictionary(ref Dictionary<string, string> dic, string key, string value)
        {
            var keyExist = dic.ContainsKey(key);
            dic[key] = value;
            if (string.IsNullOrEmpty(_path) || keyExist)
            {
                return;
            }
            if (!File.Exists(_path))
            {
                File.Create(_path);
            }
            var fs = File.AppendText(_path);
            fs.WriteLine($"{key}{_splitText}{value}");
            fs.Close();
        }
    }
}
