using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKernel.Contract.Services
{
    public  interface ISkillsService
    {
        Task<Dictionary<string, List<Dictionary<string, object>>>> GetSkillsAsync();

        Task<List<string>> GetSkillFunctionAsync(string skill, string function);
    }
}
