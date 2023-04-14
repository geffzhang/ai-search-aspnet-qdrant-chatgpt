using Microsoft.SemanticKernel;
using SKernel.Factory.Config;
using System;
using System.Collections.Generic;

namespace SKernel.Factory
{
    public class NativeSkillsImporter : ISkillsImporter
    {
        private readonly IList<Type> _skills;
        private readonly IServiceProvider _provider;

        public NativeSkillsImporter(SkillOptions skillOptions, IServiceProvider provider)
        {
            _skills = skillOptions.NativeSkillTypes;
            _provider = provider;
        }

        public void ImportSkills(IKernel kernel, IList<string> skills)
        {
            foreach (var skill in _skills)
            {
                var instance = _provider.GetService(skill);
                if (instance != null && (skills.Count == 0 || skills.Contains(skill.Name.ToLower())))
                    kernel.ImportSkill(instance, skill.Name);
            }
        }
    }
}
