namespace NotificationGateway.Shared
{
    public static class StringExtensions
    {
        extension(string str)
        {
            public bool IsNullOrEmpty => string.IsNullOrEmpty(str);
            public bool IsNullOrWhiteSpace => string.IsNullOrWhiteSpace(str);

            public string ToTitleCase =>
              System.Globalization.CultureInfo
              .CurrentCulture.TextInfo
              .ToTitleCase(str.ToLower());
        }
    }

    public class Test
    {
        public void T()
        {
            string s = "sdf";
            if (s.IsNullOrEmpty == true)
            {

            }
        }
    }

    public static class ExtensionMembers
    {
        extension<TSource>(IEnumerable<TSource> source)
        {
            public bool IsEmpty => !source.Any();
        }

        extension(string str)
        {
            public bool IsEmpty => str.Length == 0;
            public bool IsNullOrEmpty => string.IsNullOrEmpty(str);
            public bool IsNullOrWhiteSpace => string.IsNullOrWhiteSpace(str);
            public string Same => str;
            public string Trimmed => "Хуй";
            public static string Default => "df";
        }
    }

    public class TestClass
    {
        public void NewExtensions(string str)
        {
            var enumerable = new List<int>();

            bool isEmpty = enumerable.IsEmpty;
            var t = str.Same;
            var trimmed = str.Trimmed;
            var def = string.Default;
            bool isNullOrEmpty = str.IsNullOrEmpty;
        }
    }
}
