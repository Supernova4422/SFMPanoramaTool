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
        
        
       // protected FileStream Binary_5_File = System.IO.File.OpenRead(filedirectory);

        public void UpgradeDMX ()
        {
            
        }
        //This is a demo as of right now. It is injecting a new FOV into the camera for the first shot.
        public void TestDMX(string retrivedirectory)
        {
            //DM.Load(Binary_5_File,Datamodel.Codecs.DeferredMode = Datamodel.Codecs.);
            FileStream Binary_5_File = System.IO.File.OpenRead(retrivedirectory);

            var data = DM.Load(Binary_5_File);
           

            float FOV = 106.25F;

            //data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children")[0].Get<Element>("camera").Remove("fieldOfView");

            System.Single NewFOV = 1;

            foreach (Element datatype in data.AllElements)
            {
                if(datatype.Name == "fieldOfView_rescale" || datatype.Name == "fieldOfView")
                {
                    datatype.Remove("value");
                    datatype.Add("value", FOV);
                }
            }

         //   data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children")[0].Get<Element>("camera").Add("fieldOfView", FOV);

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



        string PreviousCamName;
        Guid previousCamID = new Guid();

        public DM AddShots (DM data)
        {
           // Datamodel.Datamodel.RegisterCodec(typeof(Datamodel.Codecs.));

            DM Newdata = new DM();
            DM OriginalData = data;
            //Set of cameras 
            Quaternion[] Cameras =
            {
                Quaternion.CreateFromYawPitchRoll(0, 0, 0),
                Quaternion.CreateFromYawPitchRoll(0, 0, 1.5708F),
                Quaternion.CreateFromYawPitchRoll(0, 0, 3.14159F),
                Quaternion.CreateFromYawPitchRoll(0, 0, 4.71239F ),
                Quaternion.CreateFromYawPitchRoll(1.5708F,0 , 0),
                Quaternion.CreateFromYawPitchRoll(4.71239F, 0 , 0)
            };

            int AmountOfShots = data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Count;
            int AmountOfShotsIncremental = AmountOfShots;

            TimeSpan BeginningTracksLength = new TimeSpan();
            TimeSpan BeggingTracksLowestStart = new TimeSpan();

            Element TransformOfLastCamera = new Element();

            foreach (Element GetShotTime in data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children"))
            {
                BeginningTracksLength = BeginningTracksLength.Add(GetShotTime.Get<Element>("timeFrame").Get<System.TimeSpan>("duration"));
      

                if (GetShotTime.Get<Element>("timeFrame").Get<System.TimeSpan>("start") < BeggingTracksLowestStart)
                {
                    BeggingTracksLowestStart = GetShotTime.Get<Element>("timeFrame").Get<System.TimeSpan>("start");
                }
            }

            Dictionary<int, string> CamerasToSearch = new Dictionary<int, string>();
            int camsearch = 0;

            foreach (Element Shot in data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children"))
                {
                if (Shot.Get<Element>("camera") == null)
                {
                    CamerasToSearch.Add(camsearch, "EmptyScene");
                }
                else
                {
                    CamerasToSearch.Add(camsearch, Shot.Get<Element>("camera").Name);
                }
                    camsearch++;
            }

                //We loop 6 times, because we're making 6 angles
                for (int i = 0; i < 6; i++)
            {
                int iterations = 0; //We do this to ensure that it doesn't infinitly loop by beginning processes on newly added shots
                int getcam = 0;
                foreach (Element Shot in data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children"))
                {
                    if (iterations == AmountOfShotsIncremental)
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
                    System.TimeSpan ClipStartTime = LowestStart.Add(TotalDuration).Subtract(BeginningTracksLength);


                    Console.WriteLine(DurationForCheck);
                    
                    
                   


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
                    Guid previousCamID = new Guid();
                    Guid CameraIDToInject = new Guid();
                     
                    foreach (KeyValuePair<string, object> Value in Shot)
                    {
                        if (Value.Value != null)
                        {
                            if (Value.Key == "camera")
                            {
                                //TODO Make this a void

                                Element CameraToDeriveFrom = (Element)Value.Value;

                                
                                var CameraToInject = new Element();

                                CameraToInject.Name = "Camera_" + i;

                                CameraToInject.ClassName = CameraToDeriveFrom.ClassName;

                                Shot.Add(Value.Key, CameraToInject);

                                previousCamID = CameraToDeriveFrom.ID;

                                previousCamID = CameraToInject.ID;

                                string Camname = CameraToDeriveFrom.Name;

                                Camname = CameraToDeriveFrom.Name;
                                if (!CamerasToSearch.ContainsKey(i))
                                {
            //                        CamerasToSearch.Add(i, Camname);
                                }
                                
                                Console.WriteLine("Previous cam id: {0} name {1}", previousCamID,PreviousCamName);

                                foreach (KeyValuePair<string,object> CameraValue in CameraToDeriveFrom)
                                {

                                    // Console.WriteLine(CameraValue.Value.GetType());

                                    if (CameraValue.Value is ElementArray)
                                    {
                                        ElementArray NewElementArray = (ElementArray)CameraValue.Value;

                                        ElementArray NewElementArray2 = new ElementArray();



                                        foreach (Element SubElement in NewElementArray)
                                        {
                                            NewElementArray2.Add(SubElement);

                                            // Console.WriteLine("Here is the value: {0} and {1} and also {2}" , SubElement.ToString(), SubElement.Owner.ToString() , SubElement.ID);
                                        }

                                        CameraToInject.Add(CameraValue.Key, NewElementArray2);

                                    }

                                    else if (CameraValue.Key == "transform")
                                    {
                                        Element ElementToRead = (Element)CameraValue.Value;
                                        var ElementToInject = new Element();
                                        foreach (KeyValuePair<string,object> SubElement in ElementToRead)
                                        {
                                            ElementToInject.Add(SubElement.Key, SubElement.Value);
                                        }
                                        TransformOfLastCamera = ElementToInject;
                                        
                                        ElementToInject.ClassName = ElementToRead.ClassName;
                                        CameraToInject.Add(CameraValue.Key, ElementToInject);
                                        Console.WriteLine("New Cam transform is: {0}", TransformOfLastCamera.ID);
                                        

                                    }
                                    else if (CameraValue.Key == "fieldOfView")
                                    {
                                        float FOVToInject = 106.25F;
                                        CameraToInject.Add(CameraValue.Key, FOVToInject);
                                    }
                                    else 
                                    {
                                        CameraToInject.Add(CameraValue.Key, CameraValue.Value);
                                    }

                                    
                                }
                                CameraToInject.Get<Element>("transform").Remove("orientation"); //First we remove the current orientation
                                CameraToInject.Get<Element>("transform").Add("orientation", Cameras[i]); //Then we add the orientation for the increment we're at and store in the new camera. We can chuck it at the end 
                                

                                data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Add(Value.Key, CameraToInject);
                                Console.WriteLine("Injected New Camera with ID of: {0}" , CameraToInject.ID);
                                CameraIDToInject = CameraToInject.ID;
                                
                                //Inject New Animation Set





                            }
                            else if (Value.Key == "animationSets")
                            {
                                ElementArray AimationsetToInject = new ElementArray();
                                //Inject Everything Prior
                                data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Add(Value.Key, AimationsetToInject);

                                Console.WriteLine("About to inject camera with ID: {0} into new animation set", CameraIDToInject.ToString());
                                
                                ElementArray AnimationSetToDeriveFrom = (ElementArray)Value.Value;

                                foreach (Element ValueInAnimationSet in AnimationSetToDeriveFrom)
                                {
                                    if (ValueInAnimationSet.ContainsKey("camera"))
                                    {
                                        Element Cameratoinject = new Element();
                                        Cameratoinject.ClassName = ValueInAnimationSet.ClassName;
                                        Cameratoinject.Name = ValueInAnimationSet.Name;
                                        data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Get<ElementArray>(Value.Key).Add(Cameratoinject);
                                            
                                        Console.WriteLine("Camera found");
                                        int valuetocheckiterations = 0;
                                        ElementArray ControlsToInject = new ElementArray();

                                        foreach (KeyValuePair<string,object> DataValue in ValueInAnimationSet)
                                        {
                                            if (DataValue.Key == "controls")
                                            {
                                                ElementArray controlsinject = new ElementArray();
                                                data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Get<ElementArray>(Value.Key).Last().Add("controls", controlsinject);
                                                int loops = 0;
                                                foreach (Element ValueToCheck in ValueInAnimationSet.Get<ElementArray>("controls"))
                                                {
                                                    if (ValueToCheck.Name == "transform")
                                                    {
                                                        Element NewTransform = new Element();
                                                        NewTransform.Name = ValueToCheck.Name;
                                                        NewTransform.ClassName = ValueToCheck.ClassName;

                                                        data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Get<ElementArray>(Value.Key).Last().Get<ElementArray>("controls").Add(NewTransform);



                                                        //data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Get<ElementArray>("animationSets")[valuetocheckiterations].Add("transform",NewTransform);

                                                        Element NewPositionChannel = new Element();
                                                        foreach (KeyValuePair<string, object> TransformValue in ValueInAnimationSet.Get<ElementArray>("controls")[loops])
                                                        {
                                                            if (TransformValue.Key == "positionChannel")
                                                            {
                                                                Element PosChannel = new Element();
                                                                PosChannel.Name = ValueInAnimationSet.Get<ElementArray>("controls")[loops].Get<Element>("positionChannel").Name;
                                                                PosChannel.ClassName = ValueInAnimationSet.Get<ElementArray>("controls")[loops].Get<Element>("positionChannel").ClassName;
                                                                data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Get<ElementArray>(Value.Key).Last().Get<ElementArray>("controls")[loops].Add("positionChannel", PosChannel);
                                                                // data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Get<ElementArray>(Value.Key).Last().Get<ElementArray>("controls")[loops].Add(TransformValue.Key, TransformValue.Value);
                                                                foreach (KeyValuePair<string, object> PosChannelData in ValueInAnimationSet.Get<ElementArray>("controls")[loops].Get<Element>("positionChannel"))
                                                                {
                                                                    if (PosChannelData.Key == "toElement")
                                                                    {
                                                                        data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Get<ElementArray>(Value.Key).Last().Get<ElementArray>("controls")[loops].Get<Element>("positionChannel").Add("toElement", data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Get<Element>("camera").Get<Element>("transform"));
                                                                        // Datamodel.Element objectToInject = new Element(data, TransformOfLastCamera);
                                                                        //objectToInject.ID = TransformOfLastCamera;
                                                                        // Console.WriteLine(PosChannelData.Value.GetType());
                                                                        //Element NewImport = new Element(data, TransformOfLastCamera.ID);

                                                                        //COMEBACKHERE
                                                                    }
                                                                    else
                                                                    {
                                                                        data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Get<ElementArray>(Value.Key).Last().Get<ElementArray>("controls")[loops].Get<Element>("positionChannel").Add(PosChannelData.Key, PosChannelData.Value);

                                                                        //Element ElementToInject = new Element();
                                                                        //ElementToInject = PosChannelData.Value;
                                                                        //NewPositionChannel.Add(PosChannelData.Key, PosChannelData.Value);
                                                                    }
                                                                }

                                                                // data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Get<ElementArray>(Value.Key).Last().Get<ElementArray>("controls")[loops].Get<Element>("positionChannel").Add(TransformValue.Key, TransformValue.Value);

                                                            }
                                                            else
                                                            {
                                                                data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Get<ElementArray>(Value.Key).Last().Get<ElementArray>("controls")[loops].Add(TransformValue.Key, TransformValue.Value);
                                                            }


                                                        }

                                                    }
                                                    
                                                    else
                                                    {
                                                        data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Get<ElementArray>(Value.Key).Last().Get<ElementArray>("controls").Add(ValueToCheck);
                                                    }
                                                    loops++;
                                                }

                                        }
                                            else if (DataValue.Key == "camera")
                                            {
                                                Element Toinsert = data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Get<Element>("camera");

                                                data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Get<ElementArray>(Value.Key).Last().Get<ElementArray>("controls").Add(Toinsert);
                                            }
                                            else
                                            {
                                                if (DataValue.Value.GetType().ToString() != "Datamodel.ElementArray")
                                                {
                                                    data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Get<ElementArray>(Value.Key).Last().Add(DataValue.Key, DataValue.Value);
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
                                                    data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Get<ElementArray>(Value.Key).Last().Add(DataValue.Key, NewElementArray2);

                                                }

                                                //data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Get<ElementArray>(Value.Key).Last().Add(DataValue.Key, DataValue.Value);
                                            }
                                        
                                            valuetocheckiterations++; 
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("NotCam");
                                        AimationsetToInject.Add(ValueInAnimationSet);
                                    }
                                }

                            }
                            //  Console.WriteLine(Value.Value.GetType().ToString());
                            else if (Value.Key == "timeFrame")
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
                //CAmInjectionCode
                //OnlyChecksIftheEndIsEmpty
                if (data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Get<ElementArray>("trackGroups").Count == 0)
                {

                }
                else
                {
                    int loopblock = data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Get<ElementArray>("trackGroups")[0].Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Count;
                    // int loopblock = 9999;
                    int loopiterations = 0;

                    foreach (Element Arraycheck in data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Get<ElementArray>("trackGroups")[0].Get<ElementArray>("tracks")[0].Get<ElementArray>("children"))
                    {
                        if (loopiterations == loopblock)
                        {
                            break;
                        }

                        Console.WriteLine("Looking for cam with name {0}", PreviousCamName);
                        if (Arraycheck.Name == CamerasToSearch[getcam])
                        {
                            Console.WriteLine("FoundCam");
                            data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Get<ElementArray>("trackGroups")[0].Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Add(new Element());


                            // Element ItemToInject = Arraycheck;
                            Element ItemToInject = new Element();

                            foreach (KeyValuePair<string, object> Datavalue in Arraycheck)
                            {
                                if (Datavalue.Value.GetType().ToString() != "Datamodel.ElementArray")
                                {
                                    data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Get<ElementArray>("trackGroups")[0].Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Add(Datavalue.Key, Datavalue.Value);
                                }
                                else
                                {
                                    ElementArray NewElementArray = (ElementArray)Datavalue.Value;

                                    ElementArray NewElementArray2 = new ElementArray();

                                    data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Get<ElementArray>("trackGroups")[0].Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Add(Datavalue.Key, NewElementArray2);

                                    foreach (Element SubElement in NewElementArray)
                                    {
                                        if (SubElement.Name == "transform_pos")
                                        {
                                            Element NewSubelement = new Element();
                                            NewSubelement.ClassName = SubElement.ClassName;
                                            NewSubelement.Name = SubElement.Name;
                                            NewElementArray2.Add(NewSubelement);
                                            foreach (KeyValuePair<string, object> PrintValue in SubElement)
                                            {
                                                if (PrintValue.Key == "toElement")
                                                {
                                                    Console.WriteLine("Actually injecting to {0}", TransformOfLastCamera);
                                                    NewElementArray2.Last().Add(PrintValue.Key, TransformOfLastCamera);

                                                }
                                                else
                                                {
                                                    NewElementArray2.Last().Add(PrintValue.Key, PrintValue.Value);
                                                }
                                            }

                                        }

                                        else
                                        {
                                            NewElementArray2.Add(SubElement);
                                        }
                                        // Console.WriteLine("Here is the value: {0} and {1} and also {2}" , SubElement.ToString(), SubElement.Owner.ToString() , SubElement.ID);
                                    }

                                }
                            }

                            data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Get<ElementArray>("trackGroups")[0].Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().ClassName = Arraycheck.ClassName;
                            data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Get<ElementArray>("trackGroups")[0].Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Name = "NewCam";

                            //  data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Get<ElementArray>("trackGroups")[0].Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Add(ItemToInject);

                            Console.WriteLine("Camera Found");
                            //data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Get<ElementArray>("trackGroups")[0].Get<ElementArray>("tracks")[0].Get<ElementArray>("children")[getcam].Get<ElementArray>("channels")[16].Get<Element>("toElement")
                            foreach (KeyValuePair<string, object> PrintValue in data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Get<ElementArray>("trackGroups")[0].Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Get<ElementArray>("channels")[16])
                            {
                                if (PrintValue.Key == "toElement")
                                {

                                    data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Get<ElementArray>("trackGroups")[0].Get<ElementArray>("tracks")[0].Get<ElementArray>("children")[getcam].Get<ElementArray>("channels")[16].Remove(PrintValue.Key);
                                    data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Get<ElementArray>("trackGroups")[0].Get<ElementArray>("tracks")[0].Get<ElementArray>("children")[getcam].Get<ElementArray>("channels")[16].Add(PrintValue.Key, TransformOfLastCamera);
                                    Console.WriteLine("Applying element to towards: {0}", TransformOfLastCamera.ID);

                                }
                                else
                                {
                                    data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Get<ElementArray>("trackGroups")[0].Get<ElementArray>("tracks")[0].Get<ElementArray>("children")[getcam].Get<ElementArray>("channels")[16].Remove(PrintValue.Key);
                                    data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Get<ElementArray>("trackGroups")[0].Get<ElementArray>("tracks")[0].Get<ElementArray>("children")[getcam].Get<ElementArray>("channels")[16].Add(PrintValue.Key, PrintValue.Value);
                                }
                                Console.WriteLine(PrintValue.Key);
                                Console.WriteLine(PrintValue.Value);
                            }

                        }
                        else
                        {
                            //   ItemToInject.Add(Arraycheck);
                        }

                        loopiterations++;
                    }
                }
                getcam++;


            }
            do
            {
                Console.WriteLine(AmountOfShots);
                data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").RemoveAt(AmountOfShots - 1);
                AmountOfShots--;
            } while (AmountOfShots != 0);

            //Console.WriteLine("break here");
            return data;

        }

        

    }
}
