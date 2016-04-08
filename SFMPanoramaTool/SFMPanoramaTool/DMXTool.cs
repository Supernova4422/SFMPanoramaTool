using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Datamodel;
using System.Numerics;
using System.Windows.Forms;
using DM = Datamodel.Datamodel;

namespace SFMPanoramaTool
{
    class DMXTool
    {
        
        protected FileStream Binary_5_File = System.IO.File.OpenRead("D:/SFMPanorama/Datamodel.NET/Tests/Resources/sulfur_panotext.dmx");

        public void UpgradeDMX ()
        {
            
        }
        //This is a demo as of right now. It is injecting a new FOV into the camera for the first shot.
        public void TestDMX()
        {
            DM.Load(Binary_5_File);
            var data = DM.Load(Binary_5_File);
            var rootdata = data.Root;
            var data2 = data.AllElements;
            var getclip1 = data.Root.Get<Element>("activeClip");
            System.Single FOV = 100;

            rootdata.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children")[0].Get<Element>("camera").Remove("fieldOfView");
            
            rootdata.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children")[0].Get<Element>("camera").Add("fieldOfView", FOV);

            //data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Add(getclip);
            //  var data3 = data.Root.Get<Element>("shot1");
            return;
        }
        
    }
}
