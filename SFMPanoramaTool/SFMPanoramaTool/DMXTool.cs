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
        
        protected FileStream Binary_5_File = System.IO.File.OpenRead("D:/sulfur_panotext.dmx");

        public void UpgradeDMX ()
        {
            
        }
        //This is a demo as of right now. It is injecting a new FOV into the camera for the first shot.
        public void TestDMX()
        {
            DM.Load(Binary_5_File);
            var data = DM.Load(Binary_5_File);
           
            System.Single FOV = 100;

            data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children")[0].Get<Element>("camera").Remove("fieldOfView");

            System.Single NewFOV = 1;

            foreach (Element datatype in data.AllElements)
            {
                if(datatype.Name == "fieldOfView_rescale" || datatype.Name == "fieldOfView")
                {
                    datatype.Remove("value");
                    datatype.Add("value", NewFOV);
                }
            }

            data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children")[0].Get<Element>("camera").Add("fieldOfView", FOV);

            DM file = data;
            
            data = AddShots(data);

            SaveAndConvert(data, data.Encoding, data.EncodingVersion);
            //data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Add(getclip);
            //  var data3 = data.Root.Get<Element>("shot1");
            return;
        }
        protected void SaveAndConvert(Datamodel.Datamodel dm, string encoding, int version)
        {
            dm.Save("D:/Steam/steamapps/common/SourceFilmmaker/game/bin/newpano.dmx", encoding, version);
        }
        public DM AddShots (DM data)
        {
            DM Newdata = new DM();
            //Set of cameras 
            Quaternion[] Cameras =
            {
                Quaternion.CreateFromYawPitchRoll(0, 0, 0),
                Quaternion.CreateFromYawPitchRoll(0, 0, 90),
                Quaternion.CreateFromYawPitchRoll(0, 0, 180),
                Quaternion.CreateFromYawPitchRoll(0, 0, -90),
                Quaternion.CreateFromYawPitchRoll(0, 90, 0),
                Quaternion.CreateFromYawPitchRoll(0, -90, 0)
            };


            int AmountOfShots = data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Count;
            System.TimeSpan CurrentTime = data.Root.Get<Element>("activeClip").Get<Element>("timeFrame").Get<System.TimeSpan>("duration");

            System.TimeSpan EditedTime = CurrentTime + CurrentTime + CurrentTime + CurrentTime + CurrentTime + CurrentTime + CurrentTime + CurrentTime + CurrentTime + CurrentTime + CurrentTime;

            data.Root.Get<Element>("activeClip").Get<Element>("timeFrame").Remove("duration");

            data.Root.Get<Element>("activeClip").Get<Element>("timeFrame").Add("duration", EditedTime);

            Element TimeIncrement = new Element();


            System.TimeSpan StartTime = new System.TimeSpan();


            //We loop 6 times, because we're making 6 angles
            for (int i = 0; i < 6; i++)
            {
                int iterations = 0; //We do this to ensure that it doesn't infinitly loop by beginning processes on newly added shots
                foreach (Element Shot in data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children"))
                {
                    if (iterations == AmountOfShots)
                    {
                        break;
                    }
                    Shot.Get<Element>("camera").Get<Element>("transform").Remove("orientation"); //First we remove the current orientation
                    Shot.Get<Element>("camera").Get<Element>("transform").Add("orientation", Cameras[i]); //Then we add the orientation for the increment we're at and store in the new camera

                    var newshot = new Element(); //This generates both a new element AND a new ID

                    newshot.ClassName = "DmeFilmClip";
                    newshot.Name = "Shot" + i ;

                    Shot.Remove("ID");
                    
                    Shot.Get<ElementArray>("trackGroups");
                    
                    /* foreach (KeyValuePair<string, object> Value in Shot)
                    {
                       Console.WriteLine(Value.Value.GetType().ToString());
                       data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Add(Value.Key, Value.Value);  
                    } */
                    
                    //We do it like this to insert every value BUT the ID    

                    foreach (KeyValuePair<string,object> Value in Shot)
                    {
                      if (Value.Value != null)
                         { 
                           Console.WriteLine(Value.Value.GetType().ToString());
                            /*if (Value.Key == "timeFrame")
                            {
                                //Add incerement for timeframe shit
                                Element Newtime = new Element();
                                Newtime.Add("start", StartTime);

                                data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Add(Value.Key, Value.Value);
                                data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Get<Element>("timeFrame").Remove("start");
                                data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Get<Element>("timeFrame").Add("start",StartTime);
                                StartTime = StartTime.Add(data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Get<Element>("timeFrame").Get<System.TimeSpan>("duration"));
                                
                                Console.WriteLine("Current time {0} Planning to add: {1} " , StartTime.ToString(), data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Get<Element>("timeFrame").Get<System.TimeSpan>("duration"));
                            } */

                           if (Value.Value.GetType().ToString() != "Datamodel.ElementArray")
                             {
                               data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Add(Value.Key, Value.Value);
                             }
                           else
                            {
                                ElementArray NewElementArray = (ElementArray)Value.Value;

                                ElementArray NewElementArray2 = new ElementArray();

                                

                                foreach (Element SubElement in NewElementArray)
                                {
                                    NewElementArray2.Add(SubElement);
                                    
                                    Console.WriteLine("Here is the value: {0} and {1} and also {2}" , SubElement.ToString(), SubElement.Owner.ToString() , SubElement.ID);
                                }
                                data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Add(Value.Key, NewElementArray2);

                            }
                        }
                    }
                   
                    //data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Add(Shot);
                    data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Add(newshot);

                    Console.Write("Done1");
                    iterations++;
                }
            }

            //Console.WriteLine("break here");
            return data;

        }

        

    }
}
