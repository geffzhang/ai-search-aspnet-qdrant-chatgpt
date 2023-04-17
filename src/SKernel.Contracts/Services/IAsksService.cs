using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKernel.Contract.Services
{
    public interface IAsksService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<IList<string>> PostAsync(int? iterations, Message message);
    }
}
