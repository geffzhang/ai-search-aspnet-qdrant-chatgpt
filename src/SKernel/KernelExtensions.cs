using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Memory.Qdrant;
using Microsoft.SemanticKernel.CoreSkills;
using Microsoft.SemanticKernel.KernelExtensions;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;
using SKernel.Factory;
using SKernel.Factory.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SKernel
{
    public static partial class Extensions
    {
        internal static ISKFunction CreatePlan(this IKernel kernel) =>
            kernel.Skills.GetFunction("plannerskill", "createplan");

        internal static ISKFunction ExecutePlan(this IKernel kernel) =>
            kernel.Skills.GetFunction("plannerskill", "executeplan");

        internal static ContextVariables ToContext(this IEnumerable<KeyValuePair<string, string>> variables)
        {
            var context = new ContextVariables();
            foreach (var variable in variables) context[variable.Key] = variable.Value;
            return context;
        }

        internal static IKernel RegisterSemanticSkills(this IKernel kernel, string skill, IList<string> skills, ILogger logger)
        {
            foreach (var prompt in Directory.EnumerateFiles(skill, "*.txt", SearchOption.AllDirectories)
                         .Select(_ => new FileInfo(_)))
            {
                logger.LogDebug($"{prompt} === ");
                logger.LogDebug($"{skill} === ");
                logger.LogDebug($"{prompt.Directory?.Parent} === ");
                var skillName = FunctionName(new DirectoryInfo(skill), prompt.Directory);
                logger.LogDebug($"{skillName} === ");
                if (skills.Count != 0 && !skills.Contains(skillName.ToLower())) continue;
                logger.LogDebug($"Importing semantic skill ${skill}/${skillName}");
                kernel.ImportSemanticSkillFromDirectory(skill, skillName);
            }
            return kernel;
        }

        private static string FunctionName(DirectoryInfo skill, DirectoryInfo? folder)
        {
            while (!skill.FullName.Equals(folder?.Parent?.FullName)) folder = folder?.Parent;
            return folder.Name;
        }

        internal static KernelBuilder WithOpenAI(this KernelBuilder builder, SKConfig config, ApiKey api) =>
     builder.Configure(_ =>
     {
         if (api.Text != null)
             _.AddOpenAITextCompletionService("text", config.Models.Text, api.Text);
         if (api.Embedding != null)
             _.AddOpenAIEmbeddingGenerationService("embedding", config.Models.Embedding, api.Embedding);
         if (api.Chat != null)
             _.AddOpenAIChatCompletionService("chat", config.Models.Chat, api.Chat);
     });

        internal static IKernel Register(this IKernel kernel, ISkillsImporter importer, IList<string> skills)
        {
            importer.ImportSkills(kernel, skills);
            return kernel;
        }

        public static IKernel RegistryCoreSkills(this IKernel kernel, IList<string> skills)
        {
            if (ShouldLoad(skills, nameof(FileIOSkill))) 
                kernel.ImportSkill(new FileIOSkill(), nameof(FileIOSkill));
            if (ShouldLoad(skills, nameof(HttpSkill)))
                kernel.ImportSkill(new HttpSkill(), nameof(HttpSkill));
            if (ShouldLoad(skills, nameof(TextSkill)))
                kernel.ImportSkill(new TextSkill(), nameof(TextSkill));
            if (ShouldLoad(skills, nameof(TextMemorySkill)))
                kernel.ImportSkill(new TextMemorySkill(), nameof(TextMemorySkill));
            if (ShouldLoad(skills, nameof(ConversationSummarySkill)))
                kernel.ImportSkill(new ConversationSummarySkill(kernel), nameof(ConversationSummarySkill));
            if (ShouldLoad(skills, nameof(TimeSkill)))
                kernel.ImportSkill(new TimeSkill(), nameof(TimeSkill));

            kernel.ImportSkill(new PlannerSkill(kernel), nameof(PlannerSkill));
            return kernel;
        }

        private static bool ShouldLoad(IList<string> skills, string skill) =>
            skills.Count == 0 || skills.Contains(skill.ToLower());

        public static IHostBuilder ConfigureAdventKernelDefaults(this IHostBuilder builder, IConfiguration configuration) =>
       builder.ConfigureServices(services =>
       {
           services.AddSemanticKernelFactory(configuration);
           services.AddConsoleLogger(configuration);
       });

        public static IServiceCollection AddSemanticKernelFactory(this IServiceCollection services, IConfiguration configuration)
        {
            var config = new SKConfig();
            configuration.Bind(config);

            var options = config.Skills.ToSkillOptions();
            foreach (var skillType in options.NativeSkillTypes)
                services.AddSingleton(skillType);

            services.AddSingleton(options);
            services.AddSingleton(config);
            services.AddSingleton<NativeSkillsImporter>();
            services.AddSingleton<SemanticSkillsImporter>();
            services.AddSingleton<SemanticKernelFactory>();
            services.AddSingleton(typeof(IPlanExecutor), typeof(DefaultPlanExecutor));

            services.AddSingleton<IMemoryStore>(
                config.Memory.Type == "Volatile"
                    ? new VolatileMemoryStore()
                    : new QdrantMemoryStore(config.Memory.Host, config.Memory.Port, config.Memory.VectorSize));
            return services;
        }

        public static IServiceCollection AddConsoleLogger(this IServiceCollection services, IConfiguration configuration)
        {
            var factory = LoggerFactory.Create(builder =>
            {
                builder.AddConfiguration(configuration.GetSection("Logging"));
                builder.AddConsole();
            });
            services.AddSingleton(factory);
            services.AddSingleton<ILogger>(factory.CreateLogger<object>());
            return services;
        }

        public static IList<FunctionView> ToSkills(this IKernel kernel)
        {
            var view = kernel.Skills.GetFunctionsView();
            return view.NativeFunctions.Values.SelectMany(Enumerable.ToList)
                .Union(view.SemanticFunctions.Values.SelectMany(Enumerable.ToList)).ToList();
        }

        public static async Task<SKContext> InvokePipedFunctions(this IKernel kernel, Message message) =>
            await kernel.RunAsync(message.Variables.ToContext(),
                (message.Pipeline?.Select(_ => kernel.Skills.GetFunction(_.Skill, _.Name)) ?? Array.Empty<ISKFunction>())
                .ToArray());

        public static SkillOptions ToSkillOptions(this string[] directories) =>
       new()
       {
           SemanticSkillsFolders = directories,
           NativeSkillTypes = directories.SelectMany(_ => Directory
               .EnumerateFiles(_, "*.dll", SearchOption.AllDirectories)
               .SelectMany(file => Assembly.LoadFrom(file).GetTypes().Where(_ =>
                   _.GetMethods().Any(m => m.GetCustomAttribute<SKFunctionAttribute>() != null)))).ToList()
       };
    }
}
