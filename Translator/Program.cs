// See https://aka.ms/new-console-template for more information

using System.Text;
using System.Text.Unicode;
using Translator.Helper;
using Translator.Helper.Google.Translation;

const string dicPath = "Dictionary.csv";
const string url = "https://translation.googleapis.com/language/translate/v2";
const string token = "{your google api token}";

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

int min = UnicodeRanges.CjkUnifiedIdeographs.FirstCodePoint;
int max = min + UnicodeRanges.CjkUnifiedIdeographs.Length;

var dicHelper = new DictionaryHelper(dicPath, "\t");
var dictionary = dicHelper.GetDictionary();
while (true)
{
    Console.WriteLine("Give me a directory: ");
    var dir = Console.ReadLine();
    if (string.IsNullOrEmpty(dir))
    {
        return;
    }
    if (!Directory.Exists(dir))
    {
        Console.WriteLine($"{dir} not exist\n");
        continue;
    }

    var files = Directory.GetFiles(dir, "*", SearchOption.TopDirectoryOnly);
    Console.WriteLine();
    Console.WriteLine(string.Join('\n', files));
    var transClient = new TranslationHelper(url, token);
    foreach (var file in files)
    {
        var content = File.ReadAllText(file);
        if (string.IsNullOrEmpty(content))
        {
            continue;
        }
        var index = -1;
        var transObj = default(TranslateObj);
        var stack = new Stack<TranslateObj>();
        foreach (var c in content)
        {
            index++;
            if (!IsChinese(c))
            {
                if (transObj == default)
                {
                    continue;
                }
                transObj.Count = index - transObj.Start;
                stack.Push(transObj);
                transObj = default;
                continue;
            }
            if (transObj == default)
            {
                transObj = new TranslateObj
                {
                    Start = index,
                    Str = new List<char> { c },
                };
                continue;
            }
            transObj.Str.Add(c);
        }
        if (transObj != default)
        {
            transObj.Count = content.Length - transObj.Start;
            stack.Push(transObj);
        }
        var tmpContent = new StringBuilder(content);
        while (stack.Any())
        {
            var tmp = stack.Pop();
            var origin = new string(tmp.Str.ToArray());
            if (dictionary.TryGetValue(origin, out var trans))
            {
                //transResult = CheckReplace(str, transResult);
                Console.WriteLine($"{origin} => {trans}");
                tmpContent = replaceText(tmpContent, tmp, trans);
                continue;
            }
            trans = await transClient.GetTranslate(origin);
            trans = trans.Insert(1, char.ToUpper(trans[0]).ToString());
            trans = trans.Remove(0, 1);
            trans = CheckReplace(origin, trans);
            tmpContent = replaceText(tmpContent, tmp, trans);
            dicHelper.WriteBackToDictionary(ref dictionary, origin, trans);
        }
        File.WriteAllText(file, tmpContent.ToString());
        Console.WriteLine(new string('=', 20) + "All Done, next file" + new string('=', 20));
    }
}

string CheckReplace(string origin, string trans)
{
    Console.WriteLine($"\n{origin} => {trans}\nEnter with empty input for acceptance, or self-translation");
    var input = Console.ReadLine();
    if (!string.IsNullOrEmpty(input))
    {
        Console.WriteLine();
        return input;
    }
    return trans;
}

StringBuilder replaceText(StringBuilder sb, TranslateObj transObj, string transResult)
{
    sb = sb.Remove(transObj.Start, transObj.Count);
    sb = sb.Insert(transObj.Start, transResult);
    return sb;
}
bool IsChinese(char c)
{
    return c >= min && c < max;
}

StreamWriter CreateOrOpenFile(string path)
{
    if (string.IsNullOrEmpty(path))
    {
        throw new ArgumentException("Path invalid");
    }
    if (!File.Exists(path))
    {
        return new StreamWriter(File.Create(path));
    }
    return File.AppendText(path);
}
class TranslateObj
{
    public int Start { get; set; }
    public int Count { get; set; }
    public List<char> Str { get; set; }
}