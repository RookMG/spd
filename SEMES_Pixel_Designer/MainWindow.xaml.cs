using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using netDxf;
using netDxf.Blocks;
using netDxf.Collections;
using netDxf.Entities;
using GTE = netDxf.GTE;
using netDxf.Header;
using netDxf.Objects;
using netDxf.Tables;
using netDxf.Units;
using Attribute = netDxf.Entities.Attribute;
using FontStyle = netDxf.Tables.FontStyle;
using Image = netDxf.Entities.Image;
using Point = netDxf.Entities.Point;
using Trace = netDxf.Entities.Trace;
using SEMES_Pixel_Designer.Utils;

using System.Net;
using System.Net.Sockets;

namespace SEMES_Pixel_Designer
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            #region 다른 페이지에서 사용할 수 있게 함수 등록

            // 다른 페이지에서 사용하려면...
            // Utils.Mediator.NotifyColleagues("등록한 string", 파라미터);

            // 예시)
            // Utils.Mediator.NotifyColleagues("MainWindow.OpenDxf", null);
            // Utils.Mediator.NotifyColleagues("MainWindow.ColorScreen", true);

            Utils.Mediator.Register("MainWindow.NewDxf", NewDxf);
            InputBindings.Add(new KeyBinding(new DelegateCommand(NewDxf), new KeyGesture(Key.N, ModifierKeys.Control)));

            Utils.Mediator.Register("MainWindow.OpenDxf", OpenDxf);
            InputBindings.Add(new KeyBinding(new DelegateCommand(OpenDxf), new KeyGesture(Key.O, ModifierKeys.Control)));

            Utils.Mediator.Register("MainWindow.SaveDxf", SaveDxf);
            InputBindings.Add(new KeyBinding(new DelegateCommand(SaveDxf), new KeyGesture(Key.S, ModifierKeys.Control)));

            Utils.Mediator.Register("MainWindow.SaveAsDxf", SaveAsDxf);
            Utils.Mediator.Register("MainWindow.SaveBackupDxf", SaveBackupDxf);
            Utils.Mediator.Register("MainWindow.Undo", Undo);
            InputBindings.Add(new KeyBinding(new DelegateCommand(Undo), new KeyGesture(Key.Z, ModifierKeys.Control)));

            Utils.Mediator.Register("MainWindow.Redo", Redo);
            InputBindings.Add(new KeyBinding(new DelegateCommand(Redo), new KeyGesture(Key.Y, ModifierKeys.Control)));

            Utils.Mediator.Register("MainWindow.DeleteEntities", DeleteEntities);
            InputBindings.Add(new KeyBinding(new DelegateCommand(DeleteEntities), new KeyGesture(Key.Delete)));

            Utils.Mediator.Register("MainWindow.Copy", Copy);
            InputBindings.Add(new KeyBinding(new DelegateCommand(Copy), new KeyGesture(Key.C, ModifierKeys.Control)));

            Utils.Mediator.Register("MainWindow.Cut", Cut);
            InputBindings.Add(new KeyBinding(new DelegateCommand(Cut), new KeyGesture(Key.X, ModifierKeys.Control)));

            Utils.Mediator.Register("MainWindow.Paste", Paste);
            InputBindings.Add(new KeyBinding(new DelegateCommand(Paste), new KeyGesture(Key.V, ModifierKeys.Control)));

            Utils.Mediator.Register("MainWindow.DrawDot", DrawDot);
            Utils.Mediator.Register("MainWindow.DrawLine", DrawLine);
            Utils.Mediator.Register("MainWindow.DrawRectangle", DrawRectangle);
            Utils.Mediator.Register("MainWindow.DrawPolygon", DrawPolygon);
            Utils.Mediator.Register("MainWindow.CloneEntites", CloneEntities);
            Utils.Mediator.Register("MainWindow.MoveEntities", MoveEntities);
            Utils.Mediator.Register("MainWindow.ZoomIn", ZoomIn);
            Utils.Mediator.Register("MainWindow.ZoomOut", ZoomOut);
            Utils.Mediator.Register("MainWindow.MoveScreen", MoveScreen);
            Utils.Mediator.Register("MainWindow.FitScreen", FitScreen);
            Utils.Mediator.Register("MainWindow.ColorScreen", ColorScreen);
            Utils.Mediator.Register("MainWindow.ColorBackground", ColorBackground);
            Utils.Mediator.Register("MainWindow.ToggleGrid", ToggleGrid);
            Utils.Mediator.Register("MainWindow.ToggleLineWidth", ToggleLineWidth);
            Utils.Mediator.Register("MainWindow.ShowLayers", ShowLayers);
            Utils.Mediator.Register("MainWindow.ChangeLayer", ChangeLayer);
            Utils.Mediator.Register("MainWindow.DrawCanvas", DrawCanvas);
            Utils.Mediator.Register("MainWindow.ShowEntityTypes", ShowEntityTypes);
            Utils.Mediator.Register("MainWindow.ShowEntityProperties", ShowEntityProperties);
            Utils.Mediator.Register("MainWindow.ShowEntityPropertyDetail", ShowEntityPropertyDetail);
            Utils.Mediator.Register("MainWindow.ShowMousePosition", ShowMousePosition);
            Utils.Mediator.Register("MainWindow.ShowEntitiesPosition", ShowEntitiesPosition);
            Utils.Mediator.Register("MainWindow.Exit", Exit);

            Utils.Mediator.Register("MainWindow.TcpConnection", TcpConnection);

            #endregion

        }

        // 현재 편집 중인 문서
        // 다른 페이지에서 사용하려면...
        // MainWindow.doc
        // 예시)
        // foreach (var line in MainWindow.doc.Entities.Lines) {...}
        public static DxfDocument doc = new DxfDocument();
        public static string fileName = null;


        // 기능 명세서 참고
        // https://pattern-ounce-358.notion.site/a56b400c29784dc18bbf978384464316?pvs=4

        #region 파일 입출력 관련 함수들

        // 새 파일 만들기
        public void NewDxf(object obj)
        {
            // TODO : 편집 중인 파일이 있다면 저장할지 확인

            doc = new DxfDocument();
            DrawCanvas(null);
            fileName = null;
        }

        // 파일 불러오기
        public void OpenDxf(object obj)
        {
            OpenFileDialog dlgOpenFile = new OpenFileDialog();
            dlgOpenFile.Filter = "dxf files (*.dxf) | *.dxf";

            if (dlgOpenFile.ShowDialog().ToString() == "OK")
            {
                System.Windows.MessageBox.Show(dlgOpenFile.FileName);
                doc = DxfDocument.Load(dlgOpenFile.FileName, new List<string> { @".\Support" });
                fileName = dlgOpenFile.FileName;
                Utils.Mediator.NotifyColleagues("StatusBar.PrintFilepath", fileName);
                // Test(dlgOpenFile.FileName, "test_log.txt");
            }
            DrawCanvas(null);
        }

        // 파일 저장
        public void SaveDxf(object obj)
        {
            if (fileName == null) SaveAsDxf(null);
            else doc.Save(fileName);
        }

        // 파일 다른 이름으로 저장
        public void SaveAsDxf(object obj)
        {
            SaveFileDialog dlgSaveAsFile = new SaveFileDialog();
            dlgSaveAsFile.Title = "파일 저장";
            dlgSaveAsFile.Filter = "dxf files (*.dxf) | *.dxf";

            if (dlgSaveAsFile.ShowDialog().ToString() == "OK")
            {
                System.Windows.MessageBox.Show(dlgSaveAsFile.FileName);
                doc.Save(dlgSaveAsFile.FileName);

                fileName = dlgSaveAsFile.FileName;
                Utils.Mediator.NotifyColleagues("StatusBar.PrintFilepath", fileName);
            }
        }

        // 파일 자동 임시저장
        public void SaveBackupDxf(object obj)
        {

            //TODO : 구현

        }

        #endregion


        #region 파일 편집 관련 함수들

        // 실행 취소
        public void Undo(object obj)
        {

            //TODO : 구현

        }

        // 다시 실행
        public void Redo(object obj)
        {

            //TODO : 구현

        }

        // 삭제
        public void DeleteEntities(object obj)
        {

            //TODO : 구현

        }

        // 복사
        public void Copy(object obj)
        {

            //TODO : 구현

        }

        // 잘라내기
        public void Cut(object obj)
        {

            //TODO : 구현

        }

        // 붙여넣기
        public void Paste(object obj)
        {

            //TODO : 구현

        }

        // 점 그리기
        public void DrawDot(object obj)
        {

            //TODO : 구현

        }

        // 선 그리기
        public void DrawLine(object obj)
        {

            //TODO : 구현

        }

        // 직사각형 그리기
        public void DrawRectangle(object obj)
        {

            //TODO : 구현

        }

        // 폴리곤 그리기
        public void DrawPolygon(object obj)
        {

            //TODO : 구현

        }

        // 선택 도형 N*M개 복제
        public void CloneEntities(object obj)
        {

            //TODO : 구현

        }

        // 선택 도형 이동
        public void MoveEntities(object obj)
        {

            //TODO : 구현

        }

        #endregion


        #region 화면 출력 관련 함수들

        // 화면 확대
        public void ZoomIn(object obj)
        {

            //TODO : 구현

        }

        // 화면 축소
        public void ZoomOut(object obj)
        {

            //TODO : 구현

        }

        // 화면 이동
        public void MoveScreen(object obj)
        {

            //TODO : 구현

        }

        // 도면을 창 크기에 맞추기
        public void FitScreen(object obj)
        {

            //TODO : 구현

        }

        // 컬러 도면 보기, 흑백 도면 보기
        public void ColorScreen(object obj)
        {

            //TODO : 구현
            // ColorScreen("true") : 컬러 도면 보기
            // ColorScreen("false") : 흑백 도면 보기

        }

        // 흰 배경색, 검은 배경색
        public void ColorBackground(object obj)
        {

            //TODO : 구현
            // ColorBackground("white") : 흰 배경색
            // ColorBackground("black") : 검은 배경색

        }

        // 격자 표시
        public void ToggleGrid(object obj)
        {

            //TODO : 구현

        }

        // 선 굵기 표시
        public void ToggleLineWidth(object obj)
        {

            //TODO : 구현

        }

        // 레이어 보기
        public void ShowLayers(object obj)
        {

            //TODO : 구현

        }

        // 레이어 변경
        public void ChangeLayer(object obj)
        {

            //TODO : 구현

        }

        // 도면 화면에 그리기
        public void DrawCanvas(object obj)
        {

            Utils.Mediator.NotifyColleagues("MainDrawer.DrawCanvas", null);

        }

        #endregion


        #region 상세정보 출력 관련 함수들

        // 엔티티 종류 보기
        public void ShowEntityTypes(object obj)
        {

            //TODO : 구현

        }

        // 엔티티 속성
        public void ShowEntityProperties(object obj)
        {

            //TODO : 구현

        }

        // 엔티티 속성 상세 설명
        public void ShowEntityPropertyDetail(object obj)
        {

            //TODO : 구현

        }

        // 마우스 좌표 보기
        public void ShowMousePosition(object obj)
        {
            //TODO : 구현
            Utils.Mediator.NotifyColleagues("StatusBar.ShowMousePosition", obj);
        }

        // 선택한 엔티티 좌표 보기
        public void ShowEntitiesPosition(object obj)
        {
            //TODO : 구현
            if(fileName == null)
            {
                Utils.Mediator.NotifyColleagues("StatusBar.PrintFilepath", null);
            }
            else
            {
                Utils.Mediator.NotifyColleagues("StatusBar.PrintFilepath", fileName);
            }
        }

        #endregion


        #region 윈도우 관련 함수들

        // 프로그램 종료
        public void Exit(object obj)
        {

            //TODO : 구현
            //저장하지 않은 내용 확인 필수!!

        }

        #endregion


        #region 기타 디버깅용 함수들
        private static DxfDocument Test(string file, string output = null)
        {
            // optionally you can save the information to a text file
            bool outputLog = !string.IsNullOrEmpty(output);
            TextWriter writer = null;
            if (outputLog)
            {
                writer = new StreamWriter(File.Create(output));
                Console.SetOut(writer);
            }

            // check if the dxf actually exists
            FileInfo fileInfo = new FileInfo(file);

            if (!fileInfo.Exists)
            {
                Console.WriteLine("THE FILE {0} DOES NOT EXIST", file);
                Console.WriteLine();

                if (outputLog)
                {
                    writer.Flush();
                    writer.Close();
                }
                else
                {
                    Console.WriteLine("Press a key to continue...");
                    Console.ReadLine();
                }
                return null;
            }

            DxfVersion dxfVersion = DxfDocument.CheckDxfFileVersion(file, out bool isBinary);

            // check if the file is a dxf
            if (dxfVersion == DxfVersion.Unknown)
            {
                Console.WriteLine("THE FILE {0} IS NOT A VALID DXF OR THE DXF DOES NOT INCLUDE VERSION INFORMATION IN THE HEADER SECTION", file);
                Console.WriteLine();

                if (outputLog)
                {
                    writer.Flush();
                    writer.Close();
                }
                else
                {
                    Console.WriteLine("Press a key to continue...");
                    Console.ReadLine();
                }
                return null;
            }

            // check if the dxf file version is supported
            if (dxfVersion < DxfVersion.AutoCad2000)
            {
                Console.WriteLine("THE FILE {0} IS NOT A SUPPORTED DXF", file);
                Console.WriteLine();

                Console.WriteLine("FILE VERSION: {0}", dxfVersion);
                Console.WriteLine();

                if (outputLog)
                {
                    writer.Flush();
                    writer.Close();
                }
                else
                {
                    Console.WriteLine("Press a key to continue...");
                    Console.ReadLine();
                }
                return null;
            }

            Stopwatch watch = new Stopwatch();
            watch.Start();
            DxfDocument dxf = DxfDocument.Load(file, new List<string> { @".\Support" });
            watch.Stop();

            // check if there has been any problems loading the file,
            // this might be the case of a corrupt file or a problem in the library
            if (dxf == null)
            {
                Console.WriteLine("ERROR LOADING {0}", file);
                Console.WriteLine();

                Console.WriteLine("Press a key to continue...");
                Console.ReadLine();

                if (outputLog)
                {
                    writer.Flush();
                    writer.Close();
                }
                else
                {
                    Console.WriteLine("Press a key to continue...");
                    Console.ReadLine();
                }
                return null;
            }

            // the dxf has been properly loaded, let's show some information about it
            Console.WriteLine("FILE NAME: {0}", file);
            Console.WriteLine("\tbinary DXF: {0}", isBinary);
            Console.WriteLine("\tloading time: {0} seconds", watch.ElapsedMilliseconds / 1000.0);
            Console.WriteLine();
            Console.WriteLine("FILE VERSION: {0}", dxf.DrawingVariables.AcadVer);
            Console.WriteLine();
            Console.WriteLine("FILE COMMENTS: {0}", dxf.Comments.Count);
            foreach (var o in dxf.Comments)
            {
                Console.WriteLine("\t{0}", o);
            }
            Console.WriteLine();
            Console.WriteLine("FILE TIME:");
            Console.WriteLine("\tdrawing created (UTC): {0}.{1}", dxf.DrawingVariables.TduCreate, dxf.DrawingVariables.TduCreate.Millisecond.ToString("000"));
            Console.WriteLine("\tdrawing last update (UTC): {0}.{1}", dxf.DrawingVariables.TduUpdate, dxf.DrawingVariables.TduUpdate.Millisecond.ToString("000"));
            Console.WriteLine("\tdrawing edition time: {0}", dxf.DrawingVariables.TdinDwg);
            Console.WriteLine();
            Console.WriteLine("APPLICATION REGISTRIES: {0}", dxf.ApplicationRegistries.Count);
            foreach (var o in dxf.ApplicationRegistries)
            {
                Console.WriteLine("\t{0}; References count: {1}", o.Name, dxf.ApplicationRegistries.GetReferences(o.Name).Count);
            }
            Console.WriteLine();

            Console.WriteLine("LAYERS: {0}", dxf.Layers.Count);
            foreach (var o in dxf.Layers)
            {
                Console.WriteLine("\t{0}; References count: {1}", o.Name, dxf.Layers.GetReferences(o).Count);
                Debug.Assert(ReferenceEquals(o.Linetype, dxf.Linetypes[o.Linetype.Name]), "Object reference not equal.");
            }
            Console.WriteLine();

            Console.WriteLine("LINE TYPES: {0}", dxf.Linetypes.Count);
            foreach (var o in dxf.Linetypes)
            {
                Console.WriteLine("\t{0}; References count: {1}", o.Name, dxf.Linetypes.GetReferences(o.Name).Count);
            }
            Console.WriteLine();

            Console.WriteLine("TEXT STYLES: {0}", dxf.TextStyles.Count);
            foreach (var o in dxf.TextStyles)
            {
                Console.WriteLine("\t{0}; References count: {1}", o.Name, dxf.TextStyles.GetReferences(o.Name).Count);
            }
            Console.WriteLine();

            Console.WriteLine("SHAPE STYLES: {0}", dxf.ShapeStyles.Count);
            foreach (var o in dxf.ShapeStyles)
            {
                Console.WriteLine("\t{0}; References count: {1}", o.Name, dxf.ShapeStyles.GetReferences(o.Name).Count);
            }
            Console.WriteLine();

            Console.WriteLine("DIMENSION STYLES: {0}", dxf.DimensionStyles.Count);
            foreach (var o in dxf.DimensionStyles)
            {
                Console.WriteLine("\t{0}; References count: {1}", o.Name, dxf.DimensionStyles.GetReferences(o.Name).Count);

                Debug.Assert(ReferenceEquals(o.TextStyle, dxf.TextStyles[o.TextStyle.Name]), "Object reference not equal.");
                Debug.Assert(ReferenceEquals(o.DimLineLinetype, dxf.Linetypes[o.DimLineLinetype.Name]), "Object reference not equal.");
                Debug.Assert(ReferenceEquals(o.ExtLine1Linetype, dxf.Linetypes[o.ExtLine1Linetype.Name]), "Object reference not equal.");
                Debug.Assert(ReferenceEquals(o.ExtLine2Linetype, dxf.Linetypes[o.ExtLine2Linetype.Name]), "Object reference not equal.");
                if (o.DimArrow1 != null) Debug.Assert(ReferenceEquals(o.DimArrow1, dxf.Blocks[o.DimArrow1.Name]), "Object reference not equal.");
                if (o.DimArrow2 != null) Debug.Assert(ReferenceEquals(o.DimArrow2, dxf.Blocks[o.DimArrow2.Name]), "Object reference not equal.");
            }
            Console.WriteLine();

            Console.WriteLine("MLINE STYLES: {0}", dxf.MlineStyles.Count);
            foreach (var o in dxf.MlineStyles)
            {
                Console.WriteLine("\t{0}; References count: {1}", o.Name, dxf.MlineStyles.GetReferences(o.Name).Count);
                foreach (var e in o.Elements)
                {
                    Debug.Assert(ReferenceEquals(e.Linetype, dxf.Linetypes[e.Linetype.Name]), "Object reference not equal.");
                }
            }
            Console.WriteLine();

            Console.WriteLine("UCSs: {0}", dxf.UCSs.Count);
            foreach (var o in dxf.UCSs)
            {
                Console.WriteLine("\t{0}", o.Name);
            }
            Console.WriteLine();

            Console.WriteLine("BLOCKS: {0}", dxf.Blocks.Count);
            foreach (var o in dxf.Blocks)
            {
                Console.WriteLine("\t{0}; References count: {1}", o.Name, dxf.Blocks.GetReferences(o.Name).Count);
                Debug.Assert(ReferenceEquals(o.Layer, dxf.Layers[o.Layer.Name]), "Object reference not equal.");

                foreach (var e in o.Entities)
                {
                    Debug.Assert(ReferenceEquals(e.Layer, dxf.Layers[e.Layer.Name]), "Object reference not equal.");
                    Debug.Assert(ReferenceEquals(e.Linetype, dxf.Linetypes[e.Linetype.Name]), "Object reference not equal.");
                    Debug.Assert(ReferenceEquals(e.Owner, dxf.Blocks[o.Name]), "Object reference not equal.");
                    foreach (var x in e.XData.Values)
                    {
                        Debug.Assert(ReferenceEquals(x.ApplicationRegistry, dxf.ApplicationRegistries[x.ApplicationRegistry.Name]), "Object reference not equal.");
                    }

                    if (e is Text txt)
                    {
                        Debug.Assert(ReferenceEquals(txt.Style, dxf.TextStyles[txt.Style.Name]), "Object reference not equal.");
                    }

                    if (e is MText mtxt)
                    {
                        Debug.Assert(ReferenceEquals(mtxt.Style, dxf.TextStyles[mtxt.Style.Name]), "Object reference not equal.");
                    }

                    if (e is Dimension dim)
                    {
                        Debug.Assert(ReferenceEquals(dim.Style, dxf.DimensionStyles[dim.Style.Name]), "Object reference not equal.");
                        Debug.Assert(ReferenceEquals(dim.Block, dxf.Blocks[dim.Block.Name]), "Object reference not equal.");
                    }

                    if (e is MLine mline)
                    {
                        Debug.Assert(ReferenceEquals(mline.Style, dxf.MlineStyles[mline.Style.Name]), "Object reference not equal.");
                    }

                    if (e is Image img)
                    {
                        Debug.Assert(ReferenceEquals(img.Definition, dxf.ImageDefinitions[img.Definition.Name]), "Object reference not equal.");
                    }

                    if (e is Insert ins)
                    {
                        Debug.Assert(ReferenceEquals(ins.Block, dxf.Blocks[ins.Block.Name]), "Object reference not equal.");
                        foreach (var a in ins.Attributes)
                        {
                            Debug.Assert(ReferenceEquals(a.Layer, dxf.Layers[a.Layer.Name]), "Object reference not equal.");
                            Debug.Assert(ReferenceEquals(a.Linetype, dxf.Linetypes[a.Linetype.Name]), "Object reference not equal.");
                            Debug.Assert(ReferenceEquals(a.Style, dxf.TextStyles[a.Style.Name]), "Object reference not equal.");
                        }
                    }
                }

                foreach (var a in o.AttributeDefinitions.Values)
                {
                    Debug.Assert(ReferenceEquals(a.Layer, dxf.Layers[a.Layer.Name]), "Object reference not equal.");
                    Debug.Assert(ReferenceEquals(a.Linetype, dxf.Linetypes[a.Linetype.Name]), "Object reference not equal.");
                    foreach (var x in a.XData.Values)
                    {
                        Debug.Assert(ReferenceEquals(x.ApplicationRegistry, dxf.ApplicationRegistries[x.ApplicationRegistry.Name]), "Object reference not equal.");
                    }
                }
            }
            Console.WriteLine();

            Console.WriteLine("LAYOUTS: {0}", dxf.Layouts.Count);
            foreach (var o in dxf.Layouts)
            {
                Debug.Assert(ReferenceEquals(o.AssociatedBlock, dxf.Blocks[o.AssociatedBlock.Name]), "Object reference not equal.");

                Console.WriteLine("\t{0}; References count: {1}", o.Name, dxf.Layouts.GetReferences(o.Name).Count);
                EntityCollection entities = dxf.Layouts[o.Name].AssociatedBlock.Entities;
                foreach (var e in entities)
                {
                    Debug.Assert(ReferenceEquals(e.Layer, dxf.Layers[e.Layer.Name]), "Object reference not equal.");
                    Debug.Assert(ReferenceEquals(e.Linetype, dxf.Linetypes[e.Linetype.Name]), "Object reference not equal.");
                    Debug.Assert(ReferenceEquals(e.Owner, dxf.Blocks[o.AssociatedBlock.Name]), "Object reference not equal.");
                    foreach (var x in e.XData.Values)
                    {
                        Debug.Assert(ReferenceEquals(x.ApplicationRegistry, dxf.ApplicationRegistries[x.ApplicationRegistry.Name]), "Object reference not equal.");
                    }

                    if (e is Text txt)
                    {
                        Debug.Assert(ReferenceEquals(txt.Style, dxf.TextStyles[txt.Style.Name]), "Object reference not equal.");
                    }

                    if (e is MText mtxt)
                    {
                        Debug.Assert(ReferenceEquals(mtxt.Style, dxf.TextStyles[mtxt.Style.Name]), "Object reference not equal.");
                    }

                    if (e is Dimension dim)
                    {
                        Debug.Assert(ReferenceEquals(dim.Style, dxf.DimensionStyles[dim.Style.Name]), "Object reference not equal.");
                        Debug.Assert(ReferenceEquals(dim.Block, dxf.Blocks[dim.Block.Name]), "Object reference not equal.");
                    }

                    if (e is MLine mline)
                    {
                        Debug.Assert(ReferenceEquals(mline.Style, dxf.MlineStyles[mline.Style.Name]), "Object reference not equal.");
                    }

                    if (e is Image img)
                    {
                        Debug.Assert(ReferenceEquals(img.Definition, dxf.ImageDefinitions[img.Definition.Name]), "Object reference not equal.");
                    }

                    if (e is Insert ins)
                    {
                        Debug.Assert(ReferenceEquals(ins.Block, dxf.Blocks[ins.Block.Name]), "Object reference not equal.");
                        foreach (var a in ins.Attributes)
                        {
                            Debug.Assert(ReferenceEquals(a.Layer, dxf.Layers[a.Layer.Name]), "Object reference not equal.");
                            Debug.Assert(ReferenceEquals(a.Linetype, dxf.Linetypes[a.Linetype.Name]), "Object reference not equal.");
                            Debug.Assert(ReferenceEquals(a.Style, dxf.TextStyles[a.Style.Name]), "Object reference not equal.");
                        }
                    }
                }
            }
            Console.WriteLine();

            Console.WriteLine("IMAGE DEFINITIONS: {0}", dxf.ImageDefinitions.Count);
            foreach (var o in dxf.ImageDefinitions)
            {
                Console.WriteLine("\t{0}; File name: {1}; References count: {2}", o.Name, o.File, dxf.ImageDefinitions.GetReferences(o.Name).Count);
            }
            Console.WriteLine();

            Console.WriteLine("DGN UNDERLAY DEFINITIONS: {0}", dxf.UnderlayDgnDefinitions.Count);
            foreach (var o in dxf.UnderlayDgnDefinitions)
            {
                Console.WriteLine("\t{0}; File name: {1}; References count: {2}", o.Name, o.File, dxf.UnderlayDgnDefinitions.GetReferences(o.Name).Count);
            }
            Console.WriteLine();

            Console.WriteLine("DWF UNDERLAY DEFINITIONS: {0}", dxf.UnderlayDwfDefinitions.Count);
            foreach (var o in dxf.UnderlayDwfDefinitions)
            {
                Console.WriteLine("\t{0}; File name: {1}; References count: {2}", o.Name, o.File, dxf.UnderlayDwfDefinitions.GetReferences(o.Name).Count);
            }
            Console.WriteLine();

            Console.WriteLine("PDF UNDERLAY DEFINITIONS: {0}", dxf.UnderlayPdfDefinitions.Count);
            foreach (var o in dxf.UnderlayPdfDefinitions)
            {
                Console.WriteLine("\t{0}; File name: {1}; References count: {2}", o.Name, o.File, dxf.UnderlayPdfDefinitions.GetReferences(o.Name).Count);
            }
            Console.WriteLine();

            Console.WriteLine("GROUPS: {0}", dxf.Groups.Count);
            foreach (var o in dxf.Groups)
            {
                Console.WriteLine("\t{0}; Entities count: {1}", o.Name, o.Entities.Count);
            }
            Console.WriteLine();

            Console.WriteLine("ATTRIBUTE DEFINITIONS for the \"Model\" Layout: {0}", dxf.Layouts[Layout.ModelSpaceName].AssociatedBlock.AttributeDefinitions.Count);
            foreach (var o in dxf.Layouts[Layout.ModelSpaceName].AssociatedBlock.AttributeDefinitions)
            {
                Console.WriteLine("\tTag: {0}", o.Value.Tag);
            }
            Console.WriteLine();

            Console.WriteLine("ENTITIES for the Active Layout = {0}:", dxf.Entities.ActiveLayout);
            Console.WriteLine("\t{0}; count: {1}", EntityType.Arc, dxf.Entities.Arcs.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Circle, dxf.Entities.Circles.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Dimension, dxf.Entities.Dimensions.Count());
            foreach (var a in dxf.Entities.Dimensions)
            {
                foreach (var styleOverride in a.StyleOverrides.Values)
                {
                    switch (styleOverride.Type)
                    {
                        case DimensionStyleOverrideType.DimLineLinetype:
                            Debug.Assert(ReferenceEquals((Linetype)styleOverride.Value, dxf.Linetypes[((Linetype)styleOverride.Value).Name]), "Object reference not equal.");
                            break;
                        case DimensionStyleOverrideType.ExtLine1Linetype:
                            Debug.Assert(ReferenceEquals((Linetype)styleOverride.Value, dxf.Linetypes[((Linetype)styleOverride.Value).Name]), "Object reference not equal.");
                            break;
                        case DimensionStyleOverrideType.ExtLine2Linetype:
                            Debug.Assert(ReferenceEquals((Linetype)styleOverride.Value, dxf.Linetypes[((Linetype)styleOverride.Value).Name]), "Object reference not equal.");
                            break;
                        case DimensionStyleOverrideType.TextStyle:
                            Debug.Assert(ReferenceEquals((TextStyle)styleOverride.Value, dxf.TextStyles[((TextStyle)styleOverride.Value).Name]), "Object reference not equal.");
                            break;
                        case DimensionStyleOverrideType.LeaderArrow:
                            Debug.Assert(ReferenceEquals((netDxf.Blocks.Block)styleOverride.Value, dxf.Blocks[((netDxf.Blocks.Block)styleOverride.Value).Name]), "Object reference not equal.");
                            break;
                        case DimensionStyleOverrideType.DimArrow1:
                            if (styleOverride.Value == null) break;
                            Debug.Assert(ReferenceEquals((netDxf.Blocks.Block)styleOverride.Value, dxf.Blocks[((netDxf.Blocks.Block)styleOverride.Value).Name]), "Object reference not equal.");
                            break;
                        case DimensionStyleOverrideType.DimArrow2:
                            if (styleOverride.Value == null) break;
                            Debug.Assert(ReferenceEquals((netDxf.Blocks.Block)styleOverride.Value, dxf.Blocks[((netDxf.Blocks.Block)styleOverride.Value).Name]), "Object reference not equal.");
                            break;
                    }
                }
            }
            Console.WriteLine("\t{0}; count: {1}", EntityType.Ellipse, dxf.Entities.Ellipses.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Face3D, dxf.Entities.Faces3D.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Hatch, dxf.Entities.Hatches.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Image, dxf.Entities.Images.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Insert, dxf.Entities.Inserts.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Leader, dxf.Entities.Leaders.Count());
            foreach (var a in dxf.Entities.Leaders)
            {
                foreach (var styleOverride in a.StyleOverrides.Values)
                {
                    switch (styleOverride.Type)
                    {
                        case DimensionStyleOverrideType.DimLineLinetype:
                            Debug.Assert(ReferenceEquals((Linetype)styleOverride.Value, dxf.Linetypes[((Linetype)styleOverride.Value).Name]), "Object reference not equal.");
                            break;
                        case DimensionStyleOverrideType.ExtLine1Linetype:
                            Debug.Assert(ReferenceEquals((Linetype)styleOverride.Value, dxf.Linetypes[((Linetype)styleOverride.Value).Name]), "Object reference not equal.");
                            break;
                        case DimensionStyleOverrideType.ExtLine2Linetype:
                            Debug.Assert(ReferenceEquals((Linetype)styleOverride.Value, dxf.Linetypes[((Linetype)styleOverride.Value).Name]), "Object reference not equal.");
                            break;
                        case DimensionStyleOverrideType.TextStyle:
                            Debug.Assert(ReferenceEquals((TextStyle)styleOverride.Value, dxf.TextStyles[((TextStyle)styleOverride.Value).Name]), "Object reference not equal.");
                            break;
                        case DimensionStyleOverrideType.LeaderArrow:
                            Debug.Assert(ReferenceEquals((netDxf.Blocks.Block)styleOverride.Value, dxf.Blocks[((netDxf.Blocks.Block)styleOverride.Value).Name]), "Object reference not equal.");
                            break;
                        case DimensionStyleOverrideType.DimArrow1:
                            Debug.Assert(ReferenceEquals((netDxf.Blocks.Block)styleOverride.Value, dxf.Blocks[((netDxf.Blocks.Block)styleOverride.Value).Name]), "Object reference not equal.");
                            break;
                        case DimensionStyleOverrideType.DimArrow2:
                            Debug.Assert(ReferenceEquals((netDxf.Blocks.Block)styleOverride.Value, dxf.Blocks[((netDxf.Blocks.Block)styleOverride.Value).Name]), "Object reference not equal.");
                            break;
                    }
                }
            }
            Console.WriteLine("\t{0}; count: {1}", EntityType.Line, dxf.Entities.Lines.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Mesh, dxf.Entities.Meshes.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.MLine, dxf.Entities.MLines.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.MText, dxf.Entities.MTexts.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Point, dxf.Entities.Points.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.PolyfaceMesh, dxf.Entities.PolyfaceMeshes.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.PolygonMesh, dxf.Entities.PolygonMeshes.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Polyline2D, dxf.Entities.Polylines2D.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Polyline3D, dxf.Entities.Polylines3D.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Shape, dxf.Entities.Shapes.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Solid, dxf.Entities.Solids.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Spline, dxf.Entities.Splines.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Text, dxf.Entities.Texts.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Ray, dxf.Entities.Rays.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Underlay, dxf.Entities.Underlays.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Viewport, dxf.Entities.Viewports.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Wipeout, dxf.Entities.Wipeouts.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.XLine, dxf.Entities.XLines.Count());
            Console.WriteLine();


            Console.WriteLine("{0} Info : ", EntityType.Line);
            foreach (var ln in dxf.Entities.Lines)
            {
                Console.WriteLine("\tStartPoint : {0}; EndPoint : {1}, Direction : {2}, Thickness : {3}, LineType : {4}", ln.StartPoint, ln.EndPoint, ln.Direction, ln.Thickness, ln.Linetype);
            }

            // the dxf version is controlled by the DrawingVariables property of the dxf document,
            // also a HeaderVariables instance or a DxfVersion can be passed to the constructor to initialize a new DxfDocument.
            dxf.DrawingVariables.AcadVer = DxfVersion.AutoCad2018;
            watch.Reset();
            watch.Start();
            dxf.Save("sample 2018.dxf");
            watch.Stop();
            Console.WriteLine();
            Console.WriteLine("DXF version AutoCad2018 saved in {0} seconds", watch.ElapsedMilliseconds / 1000.0);

            dxf.DrawingVariables.AcadVer = DxfVersion.AutoCad2013;
            watch.Reset();
            watch.Start();
            dxf.Save("sample 2013.dxf");
            watch.Stop();
            Console.WriteLine();
            Console.WriteLine("DXF version AutoCad2013 saved in {0} seconds", watch.ElapsedMilliseconds / 1000.0);

            dxf.DrawingVariables.AcadVer = DxfVersion.AutoCad2010;
            watch.Reset();
            watch.Start();
            dxf.Save("sample 2010.dxf");
            watch.Stop();
            Console.WriteLine();
            Console.WriteLine("DXF version AutoCad2010 saved in {0} seconds", watch.ElapsedMilliseconds / 1000.0);

            dxf.DrawingVariables.AcadVer = DxfVersion.AutoCad2007;
            watch.Reset();
            watch.Start();
            dxf.Save("sample 2007.dxf");
            watch.Stop();
            Console.WriteLine();
            Console.WriteLine("DXF version AutoCad2007 saved in {0} seconds", watch.ElapsedMilliseconds / 1000.0);

            dxf.DrawingVariables.AcadVer = DxfVersion.AutoCad2004;
            watch.Reset();
            watch.Start();
            dxf.Save("sample 2004.dxf");
            watch.Stop();
            Console.WriteLine();
            Console.WriteLine("DXF version AutoCad2004 saved in {0} seconds", watch.ElapsedMilliseconds / 1000.0);

            dxf.DrawingVariables.AcadVer = DxfVersion.AutoCad2000;
            watch.Reset();
            watch.Start();
            dxf.Save("sample 2000.dxf");
            watch.Stop();
            Console.WriteLine();
            Console.WriteLine("DXF version AutoCad2000 saved in {0} seconds", watch.ElapsedMilliseconds / 1000.0);

            // saving to binary dxf
            dxf.DrawingVariables.AcadVer = DxfVersion.AutoCad2010;
            watch.Reset();
            watch.Start();
            dxf.Save("binary test.dxf", true);
            watch.Stop();
            Console.WriteLine();
            Console.WriteLine("Binary DXF version AutoCad2010 saved in {0} seconds", watch.ElapsedMilliseconds / 1000.0);

            watch.Reset();
            watch.Start();
            DxfDocument test = DxfDocument.Load("binary test.dxf", new List<string> { @".\Support" });
            watch.Stop();
            Console.WriteLine();
            Console.WriteLine("Binary DXF version AutoCad2010 loaded in {0} seconds", watch.ElapsedMilliseconds / 1000.0);

            if (outputLog)
            {
                writer.Flush();
                writer.Close();
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("Press a key to continue...");
                Console.ReadLine();
            }
            return dxf;
        }
        #endregion

        #region TCPIP 관련 함수들
        public class AsyncObject
        {
            public Byte[] Buffer;
            public Socket WorkingSocket;
            public AsyncObject(Int32 bufferSize)
            {
                this.Buffer = new Byte[bufferSize];
            }
        }

        private Socket m_ConnectedClient = null;
        private Socket m_ServerSocket = null;
        private AsyncCallback m_fnReceiveHandler;
        private AsyncCallback m_fnSendHandler;
        private AsyncCallback m_fnAcceptHandler;

        // TCP 연결 대기
        public void TcpConnection(object obj)
        {

            //TODO : 구현
            //저장하지 않은 내용 확인 필수!!

            // TCP 통신을 위한 소켓을 생성합니다.
            m_ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

            // 특정 포트에서 모든 주소로부터 들어오는 연결을 받기 위해 포트를 바인딩합니다.
            m_ServerSocket.Bind(new IPEndPoint(IPAddress.Any, 1004));

            // 연결 요청을 받기 시작합니다.
            m_ServerSocket.Listen(5);

            // 비동기 작업에 사용될 대리자를 초기화합니다.
            m_fnReceiveHandler = new AsyncCallback(handleDataReceive);
            m_fnSendHandler = new AsyncCallback(handleDataSend);
            m_fnAcceptHandler = new AsyncCallback(handleClientConnectionRequest);

            // BeginAccept 메서드를 이용해 들어오는 연결 요청을 비동기적으로 처리합니다.
            // 연결 요청을 처리하는 함수는 handleClientConnectionRequest 입니다.
            m_ServerSocket.BeginAccept(m_fnAcceptHandler, null);

        }

        private void handleDataReceive(IAsyncResult ar)
        {

            // 넘겨진 추가 정보를 가져옵니다.
            // AsyncState 속성의 자료형은 Object 형식이기 때문에 형 변환이 필요합니다~!
            AsyncObject ao = (AsyncObject)ar.AsyncState;

            // 받은 바이트 수 저장할 변수 선언
            Int32 recvBytes;

            try
            {
                // 자료를 수신하고, 수신받은 바이트를 가져옵니다.
                recvBytes = ao.WorkingSocket.EndReceive(ar);
            }
            catch
            {
                // 예외가 발생하면 함수 종료!
                return;
            }

            // 수신받은 자료의 크기가 1 이상일 때에만 자료 처리
            if (recvBytes > 0)
            {
                // 공백 문자들이 많이 발생할 수 있으므로, 받은 바이트 수 만큼 배열을 선언하고 복사한다.
                Byte[] msgByte = new Byte[recvBytes];
                Array.Copy(ao.Buffer, msgByte, recvBytes);

                // 데이터 파싱
                string now_data = Encoding.Unicode.GetString(msgByte);
                string[] parts = now_data.Split(';');

                foreach(string part in parts)
                {
                    System.Windows.MessageBox.Show(part);
                }

                if(parts[0] == "CMDREADY")
                {
                    if(parts[1] == "createNew=0")
                    {
                        if(parts[2] == "complete=1")
                        {
                            SendMessage("CMDREADY;ACK;Path=CAD\\cadFile.cad");
                        }
                    }
                }
            }

            try
            {
                // 비동기적으로 들어오는 자료를 수신하기 위해 BeginReceive 메서드 사용!
                ao.WorkingSocket.BeginReceive(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, m_fnReceiveHandler, ao);
            }
            catch (Exception ex)
            {
                // 예외가 발생하면 예외 정보 출력 후 함수를 종료한다.
                System.Windows.MessageBox.Show($"자료 수신 대기 도중 오류 발생! 메세지: {ex.Message}");
                return;
            }
        }

        private void handleDataSend(IAsyncResult ar)
        {

            // 넘겨진 추가 정보를 가져옵니다.
            AsyncObject ao = (AsyncObject)ar.AsyncState;

            // 보낸 바이트 수를 저장할 변수 선언
            Int32 sentBytes;

            try
            {
                // 자료를 전송하고, 전송한 바이트를 가져옵니다.
                sentBytes = ao.WorkingSocket.EndSend(ar);
            }
            catch (Exception ex)
            {
                // 예외가 발생하면 예외 정보 출력 후 함수를 종료한다.
                System.Windows.MessageBox.Show($"자료 송신 도중 오류 발생! 메세지: {ex.Message}");
                return;
            }

            if (sentBytes > 0)
            {
                // 여기도 마찬가지로 보낸 바이트 수 만큼 배열 선언 후 복사한다.
                Byte[] msgByte = new Byte[sentBytes];
                Array.Copy(ao.Buffer, msgByte, sentBytes);

                System.Windows.MessageBox.Show($"메세지 보냄: {Encoding.Unicode.GetString(msgByte)}");
            }
        }

        private void handleClientConnectionRequest(IAsyncResult ar)
        {
            Socket sockClient;
            try
            {
                // 클라이언트의 연결 요청을 수락합니다.
                sockClient = m_ServerSocket.EndAccept(ar);
                System.Windows.MessageBox.Show("연결 성공 !");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"연결 수락 도중 오류 발생! 메세지: {ex.Message}");
                return;
            }

            // 4096 바이트의 크기를 갖는 바이트 배열을 가진 AsyncObject 클래스 생성
            AsyncObject ao = new AsyncObject(4096);

            // 작업 중인 소켓을 저장하기 위해 sockClient 할당
            ao.WorkingSocket = sockClient;

            // 클라이언트 소켓 저장
            m_ConnectedClient = sockClient;

            try
            {
                // 비동기적으로 들어오는 자료를 수신하기 위해 BeginReceive 메서드 사용!
                sockClient.BeginReceive(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, m_fnReceiveHandler, ao);
            }
            catch (Exception ex)
            {
                // 예외가 발생하면 예외 정보 출력 후 함수를 종료한다.
                System.Windows.MessageBox.Show($"자료 수신 대기 도중 오류 발생! 메세지: {ex.Message}");
                return;
            }
        }
        public void SendMessage(String message)
        {
            // 추가 정보를 넘기기 위한 변수 선언
            // 크기를 설정하는게 의미가 없습니다.
            // 왜냐하면 바로 밑의 코드에서 문자열을 유니코드 형으로 변환한 바이트 배열을 반환하기 때문에
            // 최소한의 크기르 배열을 초기화합니다.
            AsyncObject ao = new AsyncObject(1);

            // 문자열을 바이트 배열으로 변환
            ao.Buffer = Encoding.Unicode.GetBytes(message);

            ao.WorkingSocket = m_ConnectedClient;

            // 전송 시작!
            try
            {
                m_ConnectedClient.BeginSend(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, m_fnSendHandler, ao);
            }
            catch (Exception ex)
            {
                Console.WriteLine("전송 중 오류 발생!\n메세지: {0}", ex.Message);
            }
        }
        #endregion
    }
}
