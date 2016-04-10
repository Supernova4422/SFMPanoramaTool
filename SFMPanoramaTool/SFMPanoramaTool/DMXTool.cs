using Datamodel.Codecs;
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
        
        protected FileStream Binary_5_File = System.IO.File.OpenRead("D:/SFMTOOLSTEST/severalshots.dmx");

        public void UpgradeDMX ()
        {
            
        }
        //This is a demo as of right now. It is injecting a new FOV into the camera for the first shot.
        public void TestDMX()
        {
            //DM.Load(Binary_5_File,Datamodel.Codecs.DeferredMode = Datamodel.Codecs.);
            
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

        public void TimeRead()
        {
            DM.Load(Binary_5_File);
            var data = DM.Load(Binary_5_File);

            System.Single FOV = 100;

            data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children")[0].Get<Element>("camera").Remove("fieldOfView");

            System.Single NewFOV = 1;

            foreach (Element datatype in data.AllElements)
            {
                if (datatype.Name == "fieldOfView_rescale" || datatype.Name == "fieldOfView")
                {
                    datatype.Remove("value");
                    datatype.Add("value", NewFOV);
                }
            }

            data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children")[0].Get<Element>("camera").Add("fieldOfView", FOV);

            DM file = data;

            data = AlignAllToFPS(data);

            SaveAndConvert(data, data.Encoding, data.EncodingVersion);
            //data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Add(getclip);
            //  var data3 = data.Root.Get<Element>("shot1");
            return;
        }
        protected void SaveAndConvert(Datamodel.Datamodel dm, string encoding, int version)
        {
            dm.Save("D:/Steam/steamapps/common/SourceFilmmaker/game/bin/newpano.dmx", encoding, version);
        }

        public DM AlignAllToFPS(DM data)
        {
            
            float FPS = data.Root.Get<Element>("settings").Get<Element>("renderSettings").Get<float>("frameRate");
            foreach (Element Shot in data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children"))
            {
                
                foreach (KeyValuePair<string, object> Value in Shot)
                {
                    if (Value.Value != null)
                    {
                        
                        if (Value.Key == "timeFrame")
                        {
                            Element AllTimeValues = (Element)Shot[0].Value;
                            
                            System.TimeSpan StartTime = (System.TimeSpan)AllTimeValues[0].Value;
                            System.TimeSpan Duration = (System.TimeSpan)AllTimeValues[1].Value;
                            System.TimeSpan OffSet = (System.TimeSpan)AllTimeValues[2].Value;
                            System.TimeSpan EndTime = StartTime.Add(Duration).Add(OffSet);
                            

                            if ((EndTime.TotalSeconds % (1/ FPS)) != 0)
                            {
                                Console.WriteLine("{0} Divided by {1} / {2} = {3} ", EndTime.TotalSeconds, "1", FPS, EndTime.TotalSeconds - (EndTime.TotalSeconds % (1 / FPS)));
                                //EndTime.TotalSeconds - (EndTime.TotalSeconds % (1 / FPS));
                                //string display = (EndTime.TotalSeconds - (EndTime.TotalSeconds % (1 / FPS))).ToString();
                                //TimeSpan NewDuration = TimeSpan.Parse(display);
                                TimeSpan NewDuration = TimeSpan.FromSeconds((EndTime.TotalSeconds - (EndTime.TotalSeconds % (1 / FPS))));
                                Console.WriteLine(Duration.TotalSeconds + " converted to: " + NewDuration.TotalSeconds);
                            }

                        }

                    }


                }
                
            }
            return data;
        }





        public DM AddShots (DM data)
        {
           // Datamodel.Datamodel.RegisterCodec(typeof(Datamodel.Codecs.));

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

                    System.TimeSpan TotalDuration = new System.TimeSpan();
                    System.TimeSpan DurationForCheck = new System.TimeSpan();
                    System.TimeSpan LowestStart = new System.TimeSpan();
                    foreach (Element GetShotTime in data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children"))
                    {
                        TotalDuration = TotalDuration.Add(GetShotTime.Get<Element>("timeFrame").Get<System.TimeSpan>("duration"));
                        DurationForCheck = GetShotTime.Get<Element>("timeFrame").Get<System.TimeSpan>("duration");


                    if (GetShotTime.Get<Element>("timeFrame").Get<System.TimeSpan>("start") < LowestStart)
                        {
                            LowestStart = GetShotTime.Get<Element>("timeFrame").Get<System.TimeSpan>("start");
                        }
                    }
                    System.TimeSpan ClipStartTime = LowestStart.Add(TotalDuration);


                    Console.WriteLine(DurationForCheck);
                    
                    
                    Shot.Get<Element>("camera").Get<Element>("transform").Remove("orientation"); //First we remove the current orientation
                    Shot.Get<Element>("camera").Get<Element>("transform").Add("orientation", Cameras[i]); //Then we add the orientation for the increment we're at and store in the new camera
                    


                    var newshot = new Element(); //This generates both a new element AND a new ID

                    newshot.ClassName = "DmeFilmClip";
                    newshot.Name = Shot.Name + "_" + i ;

                    data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Add(newshot);


                    // Shot.Remove("ID");

                    Shot.Get<ElementArray>("trackGroups");

                    /* foreach (KeyValuePair<string, object> Value in Shot)
                    {
                       Console.WriteLine(Value.Value.GetType().ToString());
                       data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Add(Value.Key, Value.Value);  
                    } */

                    //We do it like this to insert every value BUT the ID    

                    foreach (KeyValuePair<string, object> Value in Shot)
                    {
                        if (Value.Value != null)
                        {
                            //  Console.WriteLine(Value.Value.GetType().ToString());
                            if (Value.Key == "timeFrame")
                            {

                                //Add incerement for timeframe shit
                                Element AllTimeValues = (Element)Value.Value;
                                Element TimeValueToInject = new Element();
                                TimeValueToInject.ClassName = "DmeTimeFrame";

                                foreach (KeyValuePair<string, object> timevalue in AllTimeValues)
                                {
                                    if (timevalue.Key == "scale")
                                    {
                                        System.Single scaleinteger = (System.Single)timevalue.Value;
                                    }
                                    else
                                    {
                                        //Console.WriteLine("Value is {0} type is {1}", timevalue.Key, timevalue.Value.GetType());
                                        System.TimeSpan timevaluecast = (System.TimeSpan)timevalue.Value;
                                        if (timevalue.Key == "start")
                                        {
                                            TimeValueToInject.Add("start", ClipStartTime);
                                            Console.WriteLine("This is the clip start time we're adding: {0}", ClipStartTime);                               
                                        }
                                        else
                                        {
                                            TimeValueToInject.Add(timevalue.Key, timevalue.Value);
                                        }
                                    }
                                }
                                data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Add(Value.Key, TimeValueToInject);
                                
                            }
                            else
                            {


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

                                        // Console.WriteLine("Here is the value: {0} and {1} and also {2}" , SubElement.ToString(), SubElement.Owner.ToString() , SubElement.ID);
                                    }
                                    data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Add(Value.Key, NewElementArray2);

                                }
                            }
                        }
                    }
                   
                    //data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Add(Shot);
                    
                    
                    iterations++;
                }
            }

            //Console.WriteLine("break here");
            return data;

        }

        

    }
}
