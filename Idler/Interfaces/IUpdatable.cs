using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Idler.Interfaces
{
    /// <summary>
    /// Provides base functionality to update/refresh data in DataBase
    /// </summary>
    public interface IUpdatable
    {
        bool Changed { get; set; }

        Task RefreshAsync();

        Task UpdateAsync();
    }
}
