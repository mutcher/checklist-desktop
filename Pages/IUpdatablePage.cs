using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace desktop.Pages
{
    interface IUpdatablePage
    {
        void AttachUpdateEvent();
        void DetachUpdateEvent();
        void Update();
    }
}
