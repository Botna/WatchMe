using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WatchMe.Library.Views
{
    public class CameraViewPane : View
    {
        public event EventHandler CamerasLoaded;

        public CameraViewPane()
        {
            //HandlerChanged += CameraView_HandlerChanged;
            //Current = this;
        }
    }
}
