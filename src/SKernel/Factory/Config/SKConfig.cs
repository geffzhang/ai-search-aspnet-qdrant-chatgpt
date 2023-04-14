namespace SKernel.Factory.Config
{
    public class SKConfig
    {
        public string[] Skills { get; set; } = { "./skills" };

        public LanguageModel Models { get; set; } = new();

        public SemanticMemory Memory { get; set; } = new();

        public class LanguageModel
        {
            public string Text { get; set; } = "text-davinci-003";
            public string Chat { get; set; } = "gpt-3.5-turbo";
            public string Embedding { get; set; } = "text-embedding-ada-002";
        }

        public class SemanticMemory
        {
            public string Type { get; set; } = "Volatile";
            public string Host { get; set; } = "localhost";
            public int Port { get; set; } = 6333;
            public int VectorSize { get; set; } = 1536;
        }
    }
}
