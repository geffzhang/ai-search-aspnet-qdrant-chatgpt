namespace SKernel.Contract.Services
{
    public  interface ISkillsService
    {
        Task<IResult> GetSkillsAsync();

        Task<IResult> GetSkillFunctionAsync(string skill, string function);
    }
}
