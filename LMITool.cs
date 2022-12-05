using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lead.Tool.CommonData_3D;
using Lead.Tool.Interface;
using Lead.Tool.XML;
using Lmi3d.GoSdk;
using Lmi3d.GoSdk.Messages;
using Lmi3d.Zen;
using Lmi3d.Zen.Io;
using Lead.Tool.Log;

namespace Lead.Tool.LMI
{
    public class DataContext
    {
        public Double xResolution;
        public Double zResolution;
        public Double xOffset;
        public Double zOffset;
        public uint serialNumber;
    }

    public class LMITool:ITool
    {
        public Config _Config;
        private ConfigUI _ConfigControl = null;
        private DebugUI _DebugControl = null;
        private IToolState _State = IToolState.ToolMin;
        public string _ConfigPath = "";

        //点云
        private List<FSPoint[]> lstPointCloud = new List<FSPoint[]>();
        private  bool IsDataRecivedEnable = false;

        //传感器系统
        private GoSystem system;
        private GoSensor sensor; //定义类型为GoSensor类型的变量sensor

        private LMITool()
        {
        }

        public LMITool(string Name, string Path)
        {
            _ConfigPath = Path;
            if (File.Exists(Path))
            {
                _Config = (Config)XmlSerializerHelper.ReadXML(Path, typeof(Config));
            }
            else
            {
                _Config = new Config();
            }

            _ConfigControl = new ConfigUI(this);
            _DebugControl = new DebugUI(this);

            _Config.Name = Name;
        }

        public List<FSPoint[]> GetScanResult()
        {
            return lstPointCloud;
        }

        public void ClearScanResult()
        {
            lstPointCloud.Clear();
            Logger.Info(_Config.Name + " 清空缓存数据成功！");
        }

        public Control ConfigUI
        {
            get
            {
                return _ConfigControl;
            }
        }

        public Control DebugUI
        {
            get
            {
                return _DebugControl;
            }
        }

        public IToolState State
        {
            get
            {
                return _State;
            }
        }

        public void Init()
        {
            try
            {
                IsDataRecivedEnable = false;
                KApiLib.Construct();
                GoSdkLib.Construct();//初始化
                system = new GoSystem();//构建新系统

                string SENSOR_IP = "192.168.1.10";
                string sensor_IP = _Config.Sensor.IP;
                KIpAddress ipAddress = KIpAddress.Parse(SENSOR_IP); //定义类型为KIpAddress的变量ipAddress并赋值
                sensor = system.FindSensorByIpAddress(ipAddress); //通过IP找到Gocator Sensor
                sensor.Connect();

                _State = IToolState.ToolInit;
            }
            catch (Exception ex)
            {
                throw new Exception("LMI初始化失败原因：" + ex.ToString());
            }
        }

        public void Start()
        {
            try
            {
                system.EnableData(true);
                //sensor.CopyFile("dsf.job", "newExposure.job");
                sensor.DefaultJob =_Config.Sensor.JobName;
                system.SetDataHandler(onData);
                system.Start();

                _State = IToolState.ToolRunning;
            }
            catch (Exception ex)
            {
                throw new Exception("LMI启动失败。原因："+ex.ToString());
            };
        }

        public void Terminate()
        {
            try
            {
                system.Stop();
                system = null;
                sensor = null;

                _State = IToolState.ToolTerminate;
            }
            catch
            {
            }
        }

        //public void onData(KObject data)
        //{
        //    if (IsDataRecivedEnable == false)
        //    {
        //        return;
        //    }

        //    GoDataSet dataSet = (GoDataSet)data;
        //    DataContext context = new DataContext();

        //    for (UInt32 i = 0; i < dataSet.Count; i++)
        //    {
        //        GoDataMsg dataObj = (GoDataMsg)dataSet.Get(i);
        //        switch (dataObj.MessageType)
        //        {
        //            #region //UniformSurface

        //            case GoDataMessageType.UniformSurface:
        //                {
        //                    GoUniformSurfaceMsg goSurfaceMsg = (GoUniformSurfaceMsg)dataObj;
        //                    long length = goSurfaceMsg.Length;    //surface长度
        //                    long width = goSurfaceMsg.Width;      //surface宽度
        //                    long bufferSize = width * length;
        //                    double XResolution = goSurfaceMsg.XResolution / 1000000.0;  //surface 数据X方向分辨率为nm,转为mm
        //                    double YResolution = goSurfaceMsg.YResolution / 1000000.0;  //surface 数据Y方向分辨率为nm,转为mm
        //                    double ZResolution = goSurfaceMsg.ZResolution / 1000000.0;  //surface 数据X方向分辨率为nm,转为mm
        //                    double XOffset = goSurfaceMsg.XOffset / 1000.0;             //接收到surface数据X方向补偿单位um，转mm
        //                    double YOffset = goSurfaceMsg.YOffset / 1000.0;             //接收到surface数据Y方向补偿单位um，转mm
        //                    double ZOffset = goSurfaceMsg.ZOffset / 1000.0;             //接收到surface数据Z方向补偿单位um，转mm
        //                    //long rowldx, colldx;
        //                    IntPtr bufferPointer = goSurfaceMsg.Data;
        //                    int rowIdx, colIdx;
        //                    short[] ranges = new short[bufferSize];
        //                    Marshal.Copy(bufferPointer, ranges, 0, ranges.Length);

        //                    for (rowIdx = 0; rowIdx < length; rowIdx++)//row is in Y direction
        //                    {
        //                        //FSPoint[] sur = new FSPoint[width];
        //                        for (colIdx = 0; colIdx < width; colIdx++)//col is in X direction
        //                        {
        //                            if (_Points[StartIndex].Length != width)
        //                            {
        //                                _Points[StartIndex] = new FSPoint[width];
        //                                for (int index = 0; index < width; index++)
        //                                {
        //                                    _Points[StartIndex][index] = new FSPoint();
        //                                }
        //                            }

        //                            //sur[colIdx] = new FSPoint();
        //                            _Points[StartIndex][colIdx].X = colIdx * XResolution + XOffset;//客户需要的点云数据X值
        //                            _Points[StartIndex][colIdx].Y = rowIdx * YResolution + YOffset;//客户需要的点云数据Y值
        //                            _Points[StartIndex][colIdx].Z = ranges[rowIdx * width + colIdx] * ZResolution + ZOffset;//客户需要的点云数据Z值

        //                            //sur[colIdx] = surfacePointCloud[rowIdx * width + colIdx];
        //                            //short value = goSurfaceMsg.Get(rowIdx, colIdx);
        //                            //if (value == short.MinValue)
        //                            //{
        //                            //    laserData[rowIdx, colIdx] = double.NaN;
        //                            //}
        //                            //else
        //                            //{
        //                            //    laserData[rowIdx, colIdx] = value * ZResolution + ZOffset;
        //                            //}
        //                        }
        //                        lstPointCloud.Add(_Points[StartIndex]);
        //                        StartIndex++;
        //                    }
        //                }
        //                break;

        //            #endregion

        //            case GoDataMessageType.UniformProfile:
        //                {
        //                    GoUniformProfileMsg profileMsg = (GoUniformProfileMsg)dataObj;

        //                    for (UInt32 k = 0; k < profileMsg.Count; ++k)
        //                    {
        //                        int validPointCount = 0;
        //                        int profilePointCount = profileMsg.Width;
        //                        context.xResolution = (double)profileMsg.XResolution / 1000000;
        //                        context.zResolution = (double)profileMsg.ZResolution / 1000000;
        //                        context.xOffset = (double)profileMsg.XOffset / 1000;
        //                        context.zOffset = (double)profileMsg.ZOffset / 1000;

        //                        short[] points = new short[profilePointCount];
        //                        //FSPoint[] profileBuffer = new FSPoint[profilePointCount];
        //                        if (_Points[StartIndex].Length != profilePointCount)
        //                        {
        //                            _Points[StartIndex] = new FSPoint[profilePointCount];
        //                            for (int index = 0; index < profilePointCount; index++)
        //                            {
        //                                _Points[StartIndex][index] = new FSPoint();
        //                            }
        //                        }
        //                        IntPtr pointsPtr = profileMsg.Data;
        //                        Marshal.Copy(pointsPtr, points, 0, points.Length);

        //                        for (UInt32 arrayIndex = 0; arrayIndex < profilePointCount; ++arrayIndex)
        //                        {
        //                            if (points[arrayIndex] != -32768)
        //                            {
        //                                _Points[StartIndex][arrayIndex] = new FSPoint();
        //                                _Points[StartIndex][arrayIndex].X = context.xOffset + context.xResolution * arrayIndex;
        //                                _Points[StartIndex][arrayIndex].Z = context.zOffset + context.zResolution * points[arrayIndex];
        //                                validPointCount++;
        //                            }
        //                            else
        //                            {
        //                                _Points[StartIndex][arrayIndex] = new FSPoint();
        //                                _Points[StartIndex][arrayIndex].X = context.xOffset + context.xResolution * arrayIndex;
        //                                _Points[StartIndex][arrayIndex].Z = -32768;
        //                            }
        //                        }
        //                        //添加到指定传感器点云集合中
        //                        lstPointCloud.Add(_Points[StartIndex]);
        //                        StartIndex++;
        //                        //trigCount[dataSet.SenderId.ToString()]++;
        //                    }
        //                }
        //                break;

        //                #region 
        //                //case GoDataMessageType.UniformProfile:
        //                //{
        //                //    //   // GoUniformSurfaceMsg goSurfaceMsg = (GoUniformSurfaceMsg)dataObj;
        //                //    GoUniformProfileMsg ProfileMsg =(GoUniformProfileMsg)dataObj;
        //                //    for (UInt32 K = 0; K < ProfileMsg.Count; K++)
        //                //    {

        //                //        int ValidPointCount = 0;
        //                //        int ProFilePointCount = ProfileMsg.Width;
        //                //        context.xResolution = (double)ProfileMsg.XResolution / 1000000.0;
        //                //        context.zResolution = (double)ProfileMsg.ZResolution / 1000000.0;
        //                //        context.xOffset = (double)ProfileMsg.XOffset / 1000.0;
        //                //        context.zOffset = (double)ProfileMsg.ZOffset / 1000.0;
        //                //        short[] Points = new short[ProFilePointCount];
        //                //        ProfilePoints[] ProfileBuffer = new ProfilePoints[ProFilePointCount];
        //                //        IntPtr PointsPtr = ProfileMsg.Data;
        //                //        Marshal.Copy(PointsPtr, Points, 0, Points.Length);
        //                //        for (UInt32 ArrayIndex = 0; ArrayIndex < ProFilePointCount; ArrayIndex++)
        //                //        {
        //                //            if (Points[ArrayIndex] != -32768)
        //                //            {
        //                //                ProfileBuffer[ArrayIndex] = new ProfilePoints();
        //                //                ProfileBuffer[ArrayIndex].x = context.xOffset + context.xResolution * ArrayIndex;
        //                //                ProfileBuffer[ArrayIndex].z = context.zOffset + context.zResolution * Points[ArrayIndex];
        //                //            }
        //                //            else
        //                //            {

        //                //                ProfileBuffer[ArrayIndex] = new ProfilePoints();
        //                //                ProfileBuffer[ArrayIndex].x = context.xOffset + context.xResolution * ArrayIndex;
        //                //                ProfileBuffer[ArrayIndex].z = -32768;
        //                //            }
        //                //        }

        //                //        lstPointCloud.Add(ProfileBuffer);

        //                //     }

        //                //}
        //                //break;

        //                //default:


        //                //    MessageBox.Show("错误类型："+ dataObj.MessageType.ToString());
        //                //    break;
        //                #endregion
        //        }
        //    }
        //}

        public void onData(KObject data)
        {
            if (IsDataRecivedEnable == false)
            {
                return;
            }

            GoDataSet dataSet = (GoDataSet)data;
            DataContext context = new DataContext();


            for (UInt32 i = 0; i < dataSet.Count; i++)
            {
                GoDataMsg dataObj = (GoDataMsg)dataSet.Get(i);
                switch (dataObj.MessageType)
                {
                    #region //UniformSurface

                    case GoDataMessageType.UniformSurface:
                        {
                            GoUniformSurfaceMsg goSurfaceMsg = (GoUniformSurfaceMsg)dataObj;
                            long length = goSurfaceMsg.Length;    //surface长度
                            long width = goSurfaceMsg.Width;      //surface宽度
                            long bufferSize = width * length;
                            //laserData = new double[length, width];
                            double XResolution = goSurfaceMsg.XResolution / 1000000.0;  //surface 数据X方向分辨率为nm,转为mm
                            double YResolution = goSurfaceMsg.YResolution / 1000000.0;  //surface 数据Y方向分辨率为nm,转为mm
                            double ZResolution = goSurfaceMsg.ZResolution / 1000000.0;  //surface 数据X方向分辨率为nm,转为mm
                            double XOffset = goSurfaceMsg.XOffset / 1000.0;             //接收到surface数据X方向补偿单位um，转mm
                            double YOffset = goSurfaceMsg.YOffset / 1000.0;             //接收到surface数据Y方向补偿单位um，转mm
                            double ZOffset = goSurfaceMsg.ZOffset / 1000.0;             //接收到surface数据Z方向补偿单位um，转mm
                            //long rowldx, colldx;
                            IntPtr bufferPointer = goSurfaceMsg.Data;
                            int rowIdx, colIdx;
                            //FSPoint[] surfacePointCloud = new FSPoint[bufferSize];
                            short[] ranges = new short[bufferSize];
                            Marshal.Copy(bufferPointer, ranges, 0, ranges.Length);

                            for (rowIdx = 0; rowIdx < length; rowIdx++)//row is in Y direction
                            {
                                FSPoint[] sur = new FSPoint[width];

                                for (colIdx = 0; colIdx < width; colIdx++)//col is in X direction
                                {
                                    sur[colIdx] = new FSPoint();
                                    sur[colIdx].X = colIdx * XResolution + XOffset;//客户需要的点云数据X值
                                    sur[colIdx].Y = rowIdx * YResolution + YOffset;//客户需要的点云数据Y值
                                    sur[colIdx].Z = ranges[rowIdx * width + colIdx] * ZResolution + ZOffset;//客户需要的点云数据Z值

                                    //sur[colIdx] = surfacePointCloud[rowIdx * width + colIdx];
                                    //short value = goSurfaceMsg.Get(rowIdx, colIdx);
                                    //if (value == short.MinValue)
                                    //{
                                    //    laserData[rowIdx, colIdx] = double.NaN;
                                    //}
                                    //else
                                    //{
                                    //    laserData[rowIdx, colIdx] = value * ZResolution + ZOffset;
                                    //}
                                }
                                lstPointCloud.Add(sur);
                            }
                        }
                        break;

                    #endregion

                    case GoDataMessageType.UniformProfile:
                        {
                            GoUniformProfileMsg profileMsg = (GoUniformProfileMsg)dataObj;

                            for (UInt32 k = 0; k < profileMsg.Count; ++k)
                            {
                                int validPointCount = 0;
                                int profilePointCount = profileMsg.Width;
                                context.xResolution = (double)profileMsg.XResolution / 1000000;
                                context.zResolution = (double)profileMsg.ZResolution / 1000000;
                                context.xOffset = (double)profileMsg.XOffset / 1000;
                                context.zOffset = (double)profileMsg.ZOffset / 1000;

                                short[] points = new short[profilePointCount];
                                FSPoint[] profileBuffer = new FSPoint[profilePointCount];
                                IntPtr pointsPtr = profileMsg.Data;
                                Marshal.Copy(pointsPtr, points, 0, points.Length);

                                for (UInt32 arrayIndex = 0; arrayIndex < profilePointCount; ++arrayIndex)
                                {
                                    if (points[arrayIndex] != -32768)
                                    {
                                        profileBuffer[arrayIndex] = new FSPoint();
                                        profileBuffer[arrayIndex].X = context.xOffset + context.xResolution * arrayIndex;
                                        profileBuffer[arrayIndex].Z = context.zOffset + context.zResolution * points[arrayIndex];
                                        validPointCount++;
                                    }
                                    else
                                    {
                                        profileBuffer[arrayIndex] = new FSPoint();
                                        profileBuffer[arrayIndex].X = context.xOffset + context.xResolution * arrayIndex;
                                        profileBuffer[arrayIndex].Z = -32768;
                                    }
                                }
                                //添加到指定传感器点云集合中
                                lstPointCloud.Add(profileBuffer);
                                //trigCount[dataSet.SenderId.ToString()]++;
                            }
                        }
                        break;

                        #region 
                        //case GoDataMessageType.UniformProfile:
                        //{
                        //    //   // GoUniformSurfaceMsg goSurfaceMsg = (GoUniformSurfaceMsg)dataObj;
                        //    GoUniformProfileMsg ProfileMsg =(GoUniformProfileMsg)dataObj;
                        //    for (UInt32 K = 0; K < ProfileMsg.Count; K++)
                        //    {

                        //        int ValidPointCount = 0;
                        //        int ProFilePointCount = ProfileMsg.Width;
                        //        context.xResolution = (double)ProfileMsg.XResolution / 1000000.0;
                        //        context.zResolution = (double)ProfileMsg.ZResolution / 1000000.0;
                        //        context.xOffset = (double)ProfileMsg.XOffset / 1000.0;
                        //        context.zOffset = (double)ProfileMsg.ZOffset / 1000.0;
                        //        short[] Points = new short[ProFilePointCount];
                        //        ProfilePoints[] ProfileBuffer = new ProfilePoints[ProFilePointCount];
                        //        IntPtr PointsPtr = ProfileMsg.Data;
                        //        Marshal.Copy(PointsPtr, Points, 0, Points.Length);
                        //        for (UInt32 ArrayIndex = 0; ArrayIndex < ProFilePointCount; ArrayIndex++)
                        //        {
                        //            if (Points[ArrayIndex] != -32768)
                        //            {
                        //                ProfileBuffer[ArrayIndex] = new ProfilePoints();
                        //                ProfileBuffer[ArrayIndex].x = context.xOffset + context.xResolution * ArrayIndex;
                        //                ProfileBuffer[ArrayIndex].z = context.zOffset + context.zResolution * Points[ArrayIndex];
                        //            }
                        //            else
                        //            {

                        //                ProfileBuffer[ArrayIndex] = new ProfilePoints();
                        //                ProfileBuffer[ArrayIndex].x = context.xOffset + context.xResolution * ArrayIndex;
                        //                ProfileBuffer[ArrayIndex].z = -32768;
                        //            }
                        //        }

                        //        lstPointCloud.Add(ProfileBuffer);

                        //     }

                        //}
                        //break;

                        //default:


                        //    MessageBox.Show("错误类型："+ dataObj.MessageType.ToString());
                        //    break;
                        #endregion
                }
            }
        }

        public void StartMeasure()
        {
            IsDataRecivedEnable = true;

            try
            {
                sensor.Start();
                Logger.Info(_Config.Name + " 允许采集成功！");
            }
            catch (Exception ex)
            {
                Logger.Warn(_Config.Name + " 允许采集失败："+ ex.Message);
            }
        }

        public void StopMeasure()
        {
            sensor.Stop();
            IsDataRecivedEnable = false;
            Logger.Info(_Config.Name + " 停止采集成功！");
        }
    }
}
