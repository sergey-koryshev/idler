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
        bool IsRefreshing { get; set; }

        event EventHandler RefreshCompleted;
        event EventHandler RefreshStarted;
        event EventHandler UpdateCompleted;
        event EventHandler UpdateStarted;

        Task RefreshAsync();
        Task UpdateAsync();
    }
}
