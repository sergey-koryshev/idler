using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Idler
{
    /// <summary>
    /// Provides base functionality to update/refresh data in DataBase
    /// </summary>
    public interface IUpdatable
    {
        void Refresh();

        void Update();
    }
}
