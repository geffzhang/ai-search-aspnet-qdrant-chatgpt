using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using SKernel.Factory.Config;
using System.Collections.Generic;
using System.Linq;

namespace SKernel.Factory
{
    public class SemanticKernelFactory
    {
        private readonly NativeSkillsImporter _native;
        private readonly SemanticSkillsImporter _semantic;
        private readonly SKConfig _config;
        private readonly IMemoryStore _memoryStore;
        private readonly ILogger _logger;

        public SemanticKernelFactory(NativeSkillsImporter native, SemanticSkillsImporter semantic, SKConfig config,
            IMemoryStore memoryStore, ILoggerFactory logger)
        {
            _native = native;
            _semantic = semantic;
            _config = config;
            _memoryStore = memoryStore;
            _logger = logger.CreateLogger<SemanticKernelFactory>();
        }

        public IKernel Create(ApiKey key, IList<string>? skills = null)
        {
            var selected = (skills ?? new List<string>())
                .Select(_ => _.ToLower()).ToList();

            var kernel = new KernelBuilder()
                .WithOpenAI(_config, key)
                .WithLogger(_logger)
                .Build()
                .RegistryCoreSkills(selected)
                .Register(_native, selected)
                .Register(_semantic, selected);
            kernel.UseMemory("embedding", _memoryStore);
            return kernel;
        }
    }
}
