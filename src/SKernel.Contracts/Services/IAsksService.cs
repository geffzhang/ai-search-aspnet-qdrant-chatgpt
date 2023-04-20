namespace SKernel.Contract.Services
{
    public interface IAsksService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<IResult> PostAsync(int? iterations, Message message);
    }
}
