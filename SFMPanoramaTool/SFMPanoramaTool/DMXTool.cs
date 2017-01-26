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

        public void UpgradeDMX()
        {

        }
        //This is a demo as of right now. It is injecting a new FOV into the camera for the first shot.
        public void TestDMX(string retrivedirectory, string savedirectory)
        {
            FileStream Binary_5_File = System.IO.File.OpenRead(retrivedirectory);

            var data = DM.Load(Binary_5_File);


            float FOV = 106.25F;

            System.Single NewFOV = 1;

            foreach (Element datatype in data.AllElements)
            {
                if (datatype.Name == "fieldOfView_rescale" || datatype.Name == "fieldOfView")
                {
                    datatype.Remove("value");
                    datatype.Add("value", FOV);
                }
            }

            DM file = data;

            data = AddShots(data);

            SaveAndConvert(data, data.Encoding, data.EncodingVersion, savedirectory);
            return;
        }


        protected void SaveAndConvert(Datamodel.Datamodel dm, string encoding, int version, string savedirectory)
        {
            Console.WriteLine("Processing Completed, Saving now");
            dm.Save(savedirectory, encoding, version);
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


                            if ((EndTime.TotalSeconds % (1 / FPS)) != 0)
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
        
        public DM AddShots(DM data)
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
                Quaternion.CreateFromYawPitchRoll(9.42478F, 0 , 0)
            };


            Matrix4x4[] MatrixCams =
            {
                Matrix4x4.CreateFromYawPitchRoll(0, 0, 0),
                Matrix4x4.CreateFromYawPitchRoll(0, 0, 1.5708F),
                Matrix4x4.CreateFromYawPitchRoll(0, 0, 3.14159F),
                Matrix4x4.CreateFromYawPitchRoll(0, 0, 4.71239F ),
                Matrix4x4.CreateFromYawPitchRoll(1.5708F,0 , 0),
                Matrix4x4.CreateFromYawPitchRoll(4.71239F, 0 , 0)
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
                    newshot.Name = Shot.Name + "_" + i;

                    data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Add(newshot);
                    
                    Shot.Get<ElementArray>("trackGroups");

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
                                
                                foreach (KeyValuePair<string, object> CameraValue in CameraToDeriveFrom)
                                {

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
                                        foreach (KeyValuePair<string, object> SubElement in ElementToRead)
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
                                //                     getcam needs to be used
                                //  data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children")
                                //                   Quaternion AngleToInject = CameraToInject.Get<Element>("transform").Get<Quaternion>("orientation");
                                Quaternion PreviousOrientation = CameraToDeriveFrom.Get<Element>("transform").Get<Quaternion>("orientation");

                                Quaternion OrientationTOInject = PreviousOrientation * Cameras[i];
                                //TODO LAST CAM IS DUMB


                                //   Quaternion Test = Quaternion.CreateFromRotationMatrix()

                                //Quaternion NewValue = AngleToInject + Cameras[i];
                                CameraToInject.Get<Element>("transform").Remove("orientation"); //First we remove the current orientation
                                CameraToInject.Get<Element>("transform").Add("orientation", OrientationTOInject); //Then we add the orientation for the increment we're at and store in the new camera. We can chuck it at the end 
                                                                                                                  //   CameraToInject.Get<Element>("transform").Add("orientation", Quaternion.Add(AngleToInject, Cameras[i]));

                                data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Add(Value.Key, CameraToInject);
                                Console.WriteLine("Injected New Camera with ID of: {0}", CameraToInject.ID);
                                CameraIDToInject = CameraToInject.ID;

                                //Inject New Animation Set





                            }
                            else if (Value.Key == "animationSets")
                            {
                                ElementArray AimationsetToInject = new ElementArray();
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

                                        foreach (KeyValuePair<string, object> DataValue in ValueInAnimationSet)
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
                                                        
                                                        Element NewPositionChannel = new Element();

                                                        foreach (KeyValuePair<string, object> TransformValue in ValueInAnimationSet.Get<ElementArray>("controls")[loops])
                                                        {
                                                            if (TransformValue.Key == "positionChannel")
                                                            {
                                                                Element PosChannel = new Element();
                                                                PosChannel.Name = ValueInAnimationSet.Get<ElementArray>("controls")[loops].Get<Element>("positionChannel").Name;
                                                                PosChannel.ClassName = ValueInAnimationSet.Get<ElementArray>("controls")[loops].Get<Element>("positionChannel").ClassName;
                                                                data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Get<ElementArray>(Value.Key).Last().Get<ElementArray>("controls")[loops].Add("positionChannel", PosChannel);
                                                                foreach (KeyValuePair<string, object> PosChannelData in ValueInAnimationSet.Get<ElementArray>("controls")[loops].Get<Element>("positionChannel"))
                                                                {
                                                                    if (PosChannelData.Key == "toElement")
                                                                    {
                                                                        data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Get<ElementArray>(Value.Key).Last().Get<ElementArray>("controls")[loops].Get<Element>("positionChannel").Add("toElement", data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Get<Element>("camera").Get<Element>("transform"));
                                                                    }
                                                                    else
                                                                    {
                                                                        data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Get<ElementArray>(Value.Key).Last().Get<ElementArray>("controls")[loops].Get<Element>("positionChannel").Add(PosChannelData.Key, PosChannelData.Value);
                                                                    }
                                                                }
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
                            else if (Value.Key == "timeFrame")
                            {


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
                                if (!Value.Value.GetType().ToString().Equals("Datamodel.ElementArray"))
                                {
                                    Console.WriteLine("There may be an error in your file as this component hasn't been properly tested");
                                    data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Add(Value.Key, Value.Value);
                                }
                                else
                                {
                                    ElementArray NewElementArray = (ElementArray)Value.Value;

                                    ElementArray NewElementArray2 = new ElementArray();
                                    
                                    foreach (Element SubElement in NewElementArray)
                                    {
                                        NewElementArray2.Add(SubElement);
                                    }

                                    data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Last().Add(Value.Key, NewElementArray2);

                                }
                            }
                        }
                    }

                    iterations++;
                }

                int CameraToClear = AmountOfShots;
                for (int x = CameraToClear; x != 0; x--)

                {
                    int length = data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Count;
                    Console.WriteLine("X = {0}, length = {1}", x, length);

                    if (data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children")[length - x].Get<ElementArray>("trackGroups").Count == 0)
                    {

                    }
                    else
                    {

                        int loopblock = data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children")[length - x].Get<ElementArray>("trackGroups")[0].Get<ElementArray>("tracks")[0].Get<ElementArray>("children").Count;

                        int loopiterations = 0;
                        int tracknum = 0;
                        int childrennum = 0;
                        foreach (Element TrackGroup in data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children")[length - x].Get<ElementArray>("trackGroups"))
                        {
                            foreach (Element Arraycheck in TrackGroup.Get<ElementArray>("tracks"))
                            {
                                foreach (Element Child in Arraycheck.Get<ElementArray>("children"))
                                {
                                    int camlength = CamerasToSearch.Count;
                                    Console.WriteLine("Found item name {0} while looking for {1}", Child.Name, CamerasToSearch[getcam]);
                                    if (Child.Name == CamerasToSearch[getcam])
                                    {
                                        Console.WriteLine("FoundCamNumbers");
                                        Element ItemToInject = new Element();
                                        ItemToInject.ClassName = "DmeChannelsClip";

                                        Arraycheck.Get<ElementArray>("children").Add(ItemToInject);


                                        foreach (KeyValuePair<string, object> Datavalue in Child)
                                        {
                                            if (Datavalue.Value.GetType().ToString() != "Datamodel.ElementArray")
                                            {
                                                Arraycheck.Get<ElementArray>("children").Last().Add(Datavalue.Key, Datavalue.Value);
                                                Console.WriteLine("Check Injection");
                                            }
                                            else
                                            {
                                                ElementArray NewElementArray = (ElementArray)Datavalue.Value;

                                                ElementArray NewElementArray2 = new ElementArray();

                                                Arraycheck.Get<ElementArray>("children").Last().Add(Datavalue.Key, NewElementArray2);

                                                foreach (Element SubElement in NewElementArray)
                                                {
                                                    if (SubElement[1].Value.ToString() == "valuePosition")
                                                    {
                                                        Element NewSubelement = new Element();
                                                        NewSubelement.ClassName = SubElement.ClassName;
                                                        NewSubelement.Name = SubElement.Name;
                                                        NewElementArray2.Add(NewSubelement);
                                                        foreach (KeyValuePair<string, object> PrintValue in SubElement)
                                                        {
                                                            if (PrintValue.Key == "toElement")
                                                            {
                                                                Console.WriteLine("Actually really injecting to {0}", data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children")[length - x].Get<Element>("camera").Get<Element>("transform").ID);
                                                                NewElementArray2.Last().Add(PrintValue.Key, data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children")[length - x].Get<Element>("camera").Get<Element>("transform"));
                                                                Console.WriteLine("Ok, Check for injection");
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

                                                }

                                            }
                                        }
                                        break;
                                    }
                                    if (CamerasToSearch.ContainsValue(Child.Name))
                                    {
                                        Console.WriteLine("Contained");
                                    }
                                    childrennum++;
                                }
                                tracknum++;
                            }

                        }

                        loopiterations++;
                    }
                    getcam++;
                }


                
            }
            do
            {
                Console.WriteLine(AmountOfShots);
                data.Root.Get<Element>("activeClip").Get<Element>("subClipTrackGroup").Get<ElementArray>("tracks")[0].Get<ElementArray>("children").RemoveAt(AmountOfShots - 1);
                AmountOfShots--;
            } while (AmountOfShots != 0);

            return data;

        }
    }




    
}

