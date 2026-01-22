using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace SmartStore
{
    public static class HtmlTextWriterExtensions
    {
        public static void AddAttributes(this TextWriter writer, IDictionary<string, object> attributes)
        {
            if (attributes.Any())
            {
                foreach (var pair in attributes)
                {
                    if (pair.Value != null)
                    {
                        writer.Write($" {pair.Key}=\"{pair.Value}\"");
                    }
                }
            }
        }
    }
}
