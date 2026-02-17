using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gathering_the_Magic.DeckEdit.Data.Library
{
    static public class Helper
    {
        static public IEnumerable<string> ParseTags(string _text)
        {
            if (string.IsNullOrEmpty(_text)) return Enumerable.Empty<string>();

            return _text.Split(["-", "/"], StringSplitOptions.RemoveEmptyEntries).
                Select(x => refineTag(x)).
                Where(x => !(x.Length >= 5 && x.All(c => c >= '0' && c <= '9'))). // numbers larger than 5 digits
                Distinct();
        }

        static private string refineTag(string _tag)
        {
            _tag = _tag.ToLower();
            _tag = _tag.Trim();
            _tag = _tag.Replace("_", " ");
            while (_tag.Contains("  ")) _tag = _tag.Replace("  ", " ");
            return _tag;
        }
    }
}
