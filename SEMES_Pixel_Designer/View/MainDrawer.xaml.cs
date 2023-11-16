using netDxf;
using netDxf.Entities;
using SEMES_Pixel_Designer.Utils;
using SEMES_Pixel_Designer.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static SEMES_Pixel_Designer.Utils.PolygonEntity;
using Ellipse = System.Windows.Shapes.Ellipse;

namespace SEMES_Pixel_Designer
{
    /// <summary>
    /// MainDrawer.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainDrawer : Page
    {
        public MainDrawer()
        {
            InitializeComponent();
            mainCanvas.ClipToBounds = true;

            // 성능 체크 해봐야함. 화면이 제대로 업데이트 되지 않는 문제가 있음
            //mainCanvas.CacheMode = new BitmapCache
            //{
            //    EnableClearType = false,
            //    RenderAtScale = 1,
            //    SnapsToDevicePixels = false,
            //};
        }

    }

    public class MainCanvas : Canvas
    {
        public bool darkMode = false;
        public List<Cell> cells = new List<Cell>();
        //public Cell selectedCell;
        public List<PolygonEntity> DrawingEntities = new List<PolygonEntity>();
        public double[] offset = null;
        public Polygon drawingPolygon = null;
        public Ellipse drawingEllipse = null;
        public readonly double PASTE_OFFSET = 5, MIN_SELECT_LENGTH = -1;
        public int pasteCount = 0;

        public int zoomCount = 0;

        SEMES_Pixel_Designer.View.MakeCell MakeCell_Test;
        SEMES_Pixel_Designer.View.GlassSetting SetGlass;
        SEMES_Pixel_Designer.View.SetCell SetCell_Test;
        private Cell selectedCell;

        public MainCanvas()
        {
            // 초기설정
            Coordinates.CanvasRef = this;
            SizeChanged += new SizeChangedEventHandler(ResizeWindow);


            Coordinates.BindCanvasAction = Children.Add;
            Coordinates.UnbindCanvasAction = Children.Remove;
            Coordinates.SetZIndexAction = SetZIndex;
            Coordinates.SetLeftAction = SetLeft;
            Coordinates.SetTopAction = SetTop;
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MainCanvas), new FrameworkPropertyMetadata(typeof(MainCanvas)));
            ClipToBounds = true;
            Background = Coordinates.backgroundColorBrush;
            Coordinates.glassBorderPath = new Path();
            Coordinates.glassBorderPath.Data = Coordinates.glassBorderGeometry = new StreamGeometry();
            Coordinates.glassBorderPath.Fill = Brushes.Gray;
            Coordinates.glassBorderGeometry.FillRule = FillRule.Nonzero;
            Coordinates.cellBorderPath = new Path();
            Coordinates.cellBorderPath.Data = Coordinates.cellBorderGeometry = new StreamGeometry();
            Coordinates.cellBorderPath.Fill = Brushes.White; 
            Coordinates.cellBorderGeometry.FillRule = FillRule.Nonzero;
            Children.Add(Coordinates.gridInfoText);
            Children.Add(Coordinates.glassBorderPath);
            Children.Add(Coordinates.cellBorderPath);
            SetZIndex(Coordinates.gridInfoText, -1);
            SetZIndex(Coordinates.cellBorderPath, -2);
            //SetZIndex(this, -1);

            Utils.Mediator.Register("MainDrawer.DrawCanvas", DrawCanvas);
            Utils.Mediator.Register("MainDrawer.FitScreen", FitScreen);
            Utils.Mediator.Register("MainDrawer.OpenMinimap", OpenMinimap);
            Utils.Mediator.Register("MainDrawer.DrawLine", DrawLine);
            Utils.Mediator.Register("MainDrawer.DrawRectangle", DrawRectangle);
            Utils.Mediator.Register("MainDrawer.DrawPolygon", DrawPolygon);
            Utils.Mediator.Register("MainDrawer.ColorBackground", ColorBackground);
            Utils.Mediator.Register("MainDrawer.Zoom", Zoom);
            Utils.Mediator.Register("MainDrawer.Paste", Paste);
            //Utils.Mediator.Register("MainDrawer.CloneEntities", CloneEntities);
            Utils.Mediator.Register("MainDrawer.DeleteEntities", (obj) => {
                DeleteEntities(selectedEntities);
            });
            Utils.Mediator.Register("MainDrawer.MakeNewcell", MakeNewcell_Input);
            Utils.Mediator.Register("MainDrawer.MakeNewcell_click", MakeNewcell_Clicked);
            Utils.Mediator.Register("MainDrawer.SetGlass", SetGlass_Input);
            Utils.Mediator.Register("MainDrawer.SetGlass_click", SetGlass_Clicked);
            Utils.Mediator.Register("MainDrawer.SetCell", SetCell_Input);
            Utils.Mediator.Register("MainDrawer.SetCell_Clicked", SetCell_Clicked);

            MouseMove += Info_MouseMove;

            MouseLeftButtonDown += Select_MouseLeftButtonDown;
            MouseWheel += Zoom_MouseWheel;
            MouseRightButtonDown += MoveCanvas_MouseRightButtonDown;

            //for (int r = 0; r < 5; r++)
            //{
            //    for (int c = 0; c < 4; c++)
            //    {
            //        cells.Add(new Cell("cell " + (r * 5 + c), 500000 * c + 160000, 500000 * r + 60000, 372, 372, 1000, 1000));
            //    }
            //}
            //selectedCell = cells[0];
            //cells.Add(new Cell("cell 0", 0, 0, 372, 372, 1000, 1000));
            //selectedCell = cells[0];
            Loaded += (obj, e) => { OpenMinimap(null); };
            
            // Test();

        }
        public void OpenMinimap(object obj)
        {
            if (Coordinates.MinimapRef != null) return;
            new Minimap().Show();
        }

        public void MakeNewcell_Input(object obj)
        {
            MakeCell_Test = new SEMES_Pixel_Designer.View.MakeCell();
            MakeCell_Test.ShowDialog();
        }

        public void MakeNewcell_Clicked(object obj)
        {
            double left, bottom, width, height;
            int rows, cols;

            if (MakeCell_Test.cell_name.Text.Length == 0)
            {
                MessageBox.Show("셀 이름을 입력해주세요");
                return;
            }
            if (!double.TryParse(MakeCell_Test.left_info.Text, out left))
            {
                MessageBox.Show("시작 좌표 왼쪽 값으로 숫자를 입력해주세요");
                return;
            }
            if (!double.TryParse(MakeCell_Test.bottom_info.Text, out bottom))
            {
                MessageBox.Show("시작 좌표 아랫쪽 값으로 숫자를 입력해주세요");
                return;
            }
            if (!double.TryParse(MakeCell_Test.width_info.Text, out width))
            {
                MessageBox.Show("패턴 가로 크기 값으로 숫자를 입력해주세요");
                return;
            }
            if (!double.TryParse(MakeCell_Test.height_info.Text, out height))
            {
                MessageBox.Show("패턴 세로 크기 값으로 숫자를 입력해주세요");
                return;
            }
            if (!int.TryParse(MakeCell_Test.col_info.Text, out cols))
            {
                MessageBox.Show("패턴 가로 반복 횟수 값으로 정수를 입력해주세요");
                return;
            }
            if (!int.TryParse(MakeCell_Test.row_info.Text, out rows))
            {
                MessageBox.Show("패턴 세로 반복 횟수 값으로 정수를 입력해주세요");
                return;
            }
            if (!int.TryParse(MakeCell_Test.row_info.Text, out rows))
            {
                MessageBox.Show("패턴 세로 반복 횟수 값으로 정수를 입력해주세요");
                return;
            }
            if (FindCellByName(MakeCell_Test.cell_name.Text) != null)
            {
                MessageBox.Show(MakeCell_Test.cell_name.Text+"는 이미 존재하는 셀 이름입니다.");
                return;
            }
            Cell collidingCell;
            if ((collidingCell = CheckCellCollision(left, bottom, width, height, rows, cols))!=null)
            {
                MessageBox.Show("다른 셀("+ collidingCell.name+ ")과 영역이 겹칩니다.");
                return;
            }
            if (!Cell.IsInGlass(left, bottom, width, height, rows, cols))
            {
                MessageBox.Show("셀 영역이 글라스 바깥으로 넘어갑니다.");
                return;
            }
            Cell c = new Cell(MakeCell_Test.cell_name.Text, left, bottom, width, height, rows, cols);
            netDxf.Tables.Layer layer = new netDxf.Tables.Layer(c.name);
            layer.Description = string.Format("{0},{1},{2},{3},{4},{5}", c.patternLeft, c.patternBottom, c.patternWidth, c.patternHeight, c.patternRows, c.patternCols);
            
            Mediator.ExecuteUndoableAction(new Mediator.UndoableAction
            (
                () => {
                    MainWindow.doc.Layers.Remove(layer);
                    cells.Remove(c);
                    Mediator.NotifyColleagues("EntityDetails.ShowEntityComboBox", null);
                    UpdateCanvas();
                },
                () => {
                    MainWindow.doc.Layers.Add(layer);
                    cells.Add(c);
                    Mediator.NotifyColleagues("EntityDetails.ShowEntityComboBox", null);
                    UpdateCanvas();
                },
                () =>
                {
                }
            ));

            MakeCell_Test.Close();
        }

        public void SetGlass_Input(object obj)
        {
            SetGlass = new SEMES_Pixel_Designer.View.GlassSetting();
            SetGlass.ShowDialog();
        }

        public void SetGlass_Clicked(object obj)
        {
            string glass_size = SetGlass.glass_size.Text;
            double toWidth, toHeight, fromWidth = Coordinates.glassRight, fromHeight = Coordinates.glassTop;

            if (glass_size=="사용자 지정" || (SetGlass.glass_width.Text!="" && SetGlass.glass_height.Text!=""))
            {
                if (!double.TryParse(SetGlass.glass_width.Text, out toWidth))
                {
                    MessageBox.Show("글라스 너비를 숫자로 입력해주세요");
                    return;
                }
                if (!double.TryParse(SetGlass.glass_height.Text, out toHeight))
                {
                    MessageBox.Show("글라스 높이를 숫자로 입력해주세요");
                    return;
                }

            }
            else
            {
                if (glass_size == "") return;
                string[] xy = SetGlass.glass_size.Text.Split('x');
                toWidth = double.Parse(xy[0]);
                toHeight = double.Parse(xy[1]);

            }
            toWidth *= 1000;
            toHeight *= 1000;

            Coordinates.glassRight = toWidth;
            Coordinates.glassTop = toHeight;

            foreach(Cell c in cells)
            {
                if (c.IsInGlass()) continue;
                Coordinates.glassRight = fromWidth;
                Coordinates.glassTop = fromHeight;

                MessageBox.Show(c.name+"셀이 글라스 바깥으로 벗어납니다.\n더 큰 범위를 지정하거나 "+c.name+"셀을 삭제한 후 다시 시도해주세요.");
                return;
            }


            Mediator.ExecuteUndoableAction(new Mediator.UndoableAction
            (
                () => {
                    MainWindow.doc.Layers["0"].Description = fromWidth + "," + fromHeight;
                    Coordinates.glassRight = fromWidth;
                    Coordinates.glassTop = fromHeight;
                    UpdateCanvas();
                },
                () => {
                    MainWindow.doc.Layers["0"].Description = toWidth + "," + toHeight;
                    Coordinates.glassRight = toWidth;
                    Coordinates.glassTop = toHeight;
                    UpdateCanvas();
                },
                () =>
                {
                }
            ));
            SetGlass.Close();

        }

        public void SetCell_Input(object obj)
        {
            selectedCell = (Cell)obj;
            SetCell_Test = new SEMES_Pixel_Designer.View.SetCell();


            if(selectedCell == null)
            {
                MessageBox.Show("해당 셀이 존재 하지 않습니다");
                return;
            }

            SetCell_Test.cell_name_init.Text = (SetCell_Test.cell_name_info.Text = selectedCell.Name) +" → ";
            SetCell_Test.left_init.Text = (SetCell_Test.left_info.Text = selectedCell.PatternLeft.ToString()) + " → ";
            SetCell_Test.bottom_init.Text = (SetCell_Test.bottom_info.Text = selectedCell.PatternBottom.ToString()) + " → ";
            SetCell_Test.width_init.Text = (SetCell_Test.width_info.Text = selectedCell.PatternWidth.ToString()) + " → ";
            SetCell_Test.height_init.Text = (SetCell_Test.height_info.Text = selectedCell.PatternHeight.ToString()) + " → ";
            SetCell_Test.col_init.Text = (SetCell_Test.col_info.Text = selectedCell.PatternCols.ToString()) + " → ";
            SetCell_Test.row_init.Text = (SetCell_Test.row_info.Text = selectedCell.PatternRows.ToString()) + " → ";

            SetCell_Test.ShowDialog();
        }

        public void SetCell_Clicked(object obj)
        {
            string fromName = selectedCell.name, toName = SetCell_Test.cell_name_info.Text;
            double fromLeft = selectedCell.PatternLeft, fromBottom = selectedCell.patternBottom, fromWidth = selectedCell.patternWidth, fromHeight = selectedCell.patternHeight, toLeft, toBottom, toWidth, toHeight;
            int fromRows = selectedCell.patternRows, fromCols = selectedCell.patternCols, toRows, toCols;

            if (toName.Length == 0)
            {
                MessageBox.Show("셀 이름을 입력해주세요");
                return;
            }
            if (!double.TryParse(SetCell_Test.left_info.Text, out toLeft))
            {
                MessageBox.Show("시작 좌표 왼쪽 값으로 숫자를 입력해주세요");
                return;
            }
            if (!double.TryParse(SetCell_Test.bottom_info.Text, out toBottom))
            {
                MessageBox.Show("시작 좌표 아랫쪽 값으로 숫자를 입력해주세요");
                return;
            }
            if (!double.TryParse(SetCell_Test.width_info.Text, out toWidth))
            {
                MessageBox.Show("패턴 가로 크기 값으로 숫자를 입력해주세요");
                return;
            }
            if (!double.TryParse(SetCell_Test.height_info.Text, out toHeight))
            {
                MessageBox.Show("패턴 세로 크기 값으로 숫자를 입력해주세요");
                return;
            }
            if (!int.TryParse(SetCell_Test.col_info.Text, out toRows))
            {
                MessageBox.Show("패턴 가로 반복 횟수 값으로 정수를 입력해주세요");
                return;
            }
            if (!int.TryParse(SetCell_Test.row_info.Text, out toCols))
            {
                MessageBox.Show("패턴 세로 반복 횟수 값으로 정수를 입력해주세요");
                return;
            }

            if (FindCellByName(selectedCell, SetCell_Test.cell_name_info.Text) != null)
            {
                MessageBox.Show(SetCell_Test.cell_name_info.Text + "는 이미 존재하는 셀 이름입니다.");
                return;
            }
            Cell collidingCell;
            if ((collidingCell = CheckCellCollision(selectedCell, toLeft, toBottom, toWidth, toHeight, toRows, toCols)) != null)
            {
                MessageBox.Show("다른 셀(" + collidingCell.name + ")과 영역이 겹칩니다.");
                return;
            }
            if (!Cell.IsInGlass(toLeft, toBottom, toWidth, toHeight, toRows, toCols))
            {
                MessageBox.Show("셀 영역이 글라스 바깥으로 넘어갑니다.");
                return;
            }
            Mediator.ExecuteUndoableAction(new Mediator.UndoableAction
            (
                () => {
                    FindCellByName(toName).changeCell(fromName, fromLeft, fromBottom, fromWidth, fromHeight,fromRows, fromCols);
                    Mediator.NotifyColleagues("EntityDetails.ShowEntityComboBox", null);
                    UpdateCanvas();
                },
                () => {
                    FindCellByName(fromName).changeCell(toName, toLeft, toBottom, toWidth, toHeight, toRows, toCols);
                    Mediator.NotifyColleagues("EntityDetails.ShowEntityComboBox", null);
                    UpdateCanvas();
                },
                () =>
                {
                }
            ));

            selectedCell = null;
            SetCell_Test.Close();
        }

        public Cell FindCellByName(string name)
        {
            return FindCellByName(null, name);
        }
        public Cell FindCellByName(Cell self, string name)
        {
            foreach (Cell c in cells)
            {
                // Console.WriteLine(c.name);
                if (c == self) continue;
                if (c.name.Equals(name)) return c;
            }
            // Console.WriteLine("Alert : cannot find cell by name");
            return null;
        }

        public void UpdateCanvas()
        {
            pasteCount = 0;
            Coordinates.DrawGrid();
            foreach (PolygonEntity entity in DrawingEntities) entity.ReDraw();
            foreach (System.Windows.Shapes.Line gridLine in Coordinates.gridLines)
            {
                gridLine.MouseLeftButtonDown += Select_MouseLeftButtonDown;
                gridLine.MouseWheel += Zoom_MouseWheel;
                gridLine.MouseRightButtonDown += MoveCanvas_MouseRightButtonDown;
            }
            if(Coordinates.MinimapRef!= null)
            Coordinates.MinimapRef.AdjustRatio();
            UpdateLayout();
        }

        public void ResizeWindow(object sender, SizeChangedEventArgs e)
        {
            //Coordinates.maxX = Coordinates.minX + e.NewSize.Width / Coordinates.ratio;
            //Coordinates.minY = Coordinates.maxY - e.NewSize.Height / Coordinates.ratio;
            Coordinates.AdjustRatio();
            if(Coordinates.MinimapRef!= null)
            Coordinates.MinimapRef.AdjustRatio();
            UpdateCanvas();
        }

        public void FitScreen(object obj)
        {
            Coordinates.UpdateRange(MainWindow.doc.Entities);
            UpdateCanvas();
        }

        public void DrawCanvas(object obj)
        {
            Children.Clear();
            cells.Clear();
            Children.Add(Coordinates.gridInfoText);
            Children.Add(Coordinates.glassBorderPath);
            Children.Add(Coordinates.cellBorderPath);
            SetZIndex(Coordinates.gridInfoText, -1);
            UpdateLayout();

            foreach (var layer in MainWindow.doc.Layers)
            {
                string[] args = layer.Description.Split(',');
                if (layer.Name.Equals("0"))
                {
                    if(args.Length == 2) {
                        Coordinates.glassRight = double.Parse(args[0]);
                        Coordinates.glassTop = double.Parse(args[1]);
                    }
                    else
                    {
                        cells.Add(new Cell("0",0,0,Coordinates.glassRight, Coordinates.glassTop, 1, 1));
                    }
                }
                else if (args.Length == 6) cells.Add(new Cell(layer.Name, double.Parse(args[0]), double.Parse(args[1]), double.Parse(args[2]), double.Parse(args[3]), int.Parse(args[4]), int.Parse(args[5])));
            }
            Coordinates.UpdateRange(MainWindow.doc.Entities);
            Coordinates.DrawGrid();
            DrawingEntities.Clear();

            foreach (netDxf.Entities.Line line in MainWindow.doc.Entities.Lines)
            {
                Cell c = FindCellByName(line.Layer.Name);
                if (!c.Contains(line)) continue;
                DrawingEntities.Add(new PolygonEntity(c, line));
            }

            foreach (Polyline2D polyline in MainWindow.doc.Entities.Polylines2D)
            {
                Cell c = FindCellByName(polyline.Layer.Name);
                if (!c.Contains(polyline)) continue;
                DrawingEntities.Add(new PolygonEntity(c, polyline));
            }
            UpdateCanvas();
            Mediator.NotifyColleagues("EntityDetails.ShowEntityComboBox", null);
        }

        public Cell CheckCellCollision(double patternLeft, double patternBottom, double patternWidth, double patternHeight, int patternRows, int patternCols)
        {
            return CheckCellCollision(null,patternLeft, patternBottom, patternWidth, patternHeight, patternRows, patternCols);
        }
        public Cell CheckCellCollision(Cell self, double patternLeft, double patternBottom, double patternWidth, double patternHeight, int patternRows, int patternCols)
        {
            foreach (Cell c in cells)
            {
                if (c == self) continue;
                if (c.Collide(patternLeft, patternBottom, patternWidth, patternHeight, patternRows, patternCols)) return c;
            }
            return null;
        }

        public void ColorBackground(object obj)
        {
            if (darkMode)
            {
                Coordinates.defaultColorBrush = Brushes.Black;
                Coordinates.backgroundColorBrush = Brushes.White;
                darkMode = false;
            }
            else
            {
                Coordinates.defaultColorBrush = Brushes.White;
                Coordinates.backgroundColorBrush = Brushes.Black;
                darkMode = true;
            }
            Coordinates.cellBorderPath.Fill = Coordinates.backgroundColorBrush;
            foreach (PolygonEntity child in DrawingEntities)
            {
                child.ReColor();
            }
            UpdateCanvas();
        }

        public void Paste(object obj)
        {
            if (clipboard.Count == 0) return;
            Cell c = Coordinates.GetCurrentCell();
            if(c == null)
            {
                MessageBox.Show("셀 밖으로는 도형을 복사할 수 없습니다");
                return;
            }
            double offset = pasteCount * PASTE_OFFSET / Coordinates.ratio;
            List<PolygonEntity> pasted = new List<PolygonEntity>();
            foreach (CopyData data in clipboard)
            {
                EntityObject entity = data.CreateEntityObject();
                entity.TransformBy(Matrix3.Identity, new Vector3(c.patternLeft, c.patternBottom, 0) - data.offset);
                if (data.type == PolygonEntityType.LINE)
                {
                    pasted.Add(new PolygonEntity(c, entity as netDxf.Entities.Line));
                }
                else if (data.type == PolygonEntityType.POLYLINE)
                {
                    pasted.Add(new PolygonEntity(c, entity as Polyline2D));
                }

            }
            Mediator.ExecuteUndoableAction(new Mediator.UndoableAction
            (
                () => {
                    foreach (PolygonEntity entity in pasted)
                    {
                        DrawingEntities.Add(entity);
                        MainWindow.doc.Entities.Add(entity.GetEntityObject());
                    }
                },
                () => {
                    foreach (PolygonEntity entity in pasted)
                    {
                        DrawingEntities.Remove(entity);
                        MainWindow.doc.Entities.Remove(entity.GetEntityObject());
                        entity.Delete();
                    }
                },
                () =>
                {
                    foreach (PolygonEntity entity in pasted)
                    {
                        DrawingEntities.Add(entity);
                        MainWindow.doc.Entities.Add(entity.GetEntityObject());
                        entity.Restore();
                    }
                },
                () =>
                {
                    foreach (PolygonEntity entity in pasted) entity.Remove();
                }
            ));
            pasteCount++;
        }

        //public void CloneEntities(object obj)
        //{
        //    //Window.GetWindow(this).Close();
        //    var param = (Tuple<int, int, double, double>)obj;
        //    int R = param.Item1;
        //    int C = param.Item2;
        //    double intervalX = param.Item4;
        //    double intervalY = -param.Item3;

        //    //int R = 30, C = 30;
        //    //double intervalX = 100, intervalY = -100;

        //    List<CopyData> clipboardBackup = new List<CopyData>();
        //    foreach (CopyData copyData in clipboard) clipboardBackup.Add(copyData);
        //    CopySelected();

        //    List<PolygonEntity> cloned = new List<PolygonEntity>();
        //    foreach (CopyData data in clipboard)
        //    {
        //        for (int r = 0; r < R; r++)
        //        {
        //            for (int c = 0; c < C; c++)
        //            {
        //                if (r == 0 && c == 0) continue;

        //                EntityObject entity = data.CreateEntityObject(); ;
        //                entity.TransformBy(Matrix3.Identity, new Vector3(r*intervalX, c*intervalY, 0));
        //                MainWindow.doc.Entities.Add(entity);
        //                if (data.type == PolygonEntityType.LINE)
        //                {
        //                    cloned.Add(new PolygonEntity(FindCellByName(entity.Layer.Name), entity as netDxf.Entities.Line));
        //                }
        //                else if (data.type == PolygonEntityType.POLYLINE)
        //                {
        //                    cloned.Add(new PolygonEntity(FindCellByName(entity.Layer.Name), entity as Polyline2D));
        //                }
        //            }
        //        }
        //    }
        //    Mediator.ExecuteUndoableAction(new Mediator.UndoableAction
        //    (
        //        () => {
        //            foreach (PolygonEntity entity in cloned)
        //            {
        //                DrawingEntities.Add(entity);
        //            }
        //        },
        //        () => {
        //            foreach (PolygonEntity entity in cloned)
        //            {
        //                DrawingEntities.Remove(entity);
        //                entity.Delete();
        //            }
        //        },
        //        () =>
        //        {
        //            foreach (PolygonEntity entity in cloned)
        //            {
        //                DrawingEntities.Add(entity);
        //                entity.Restore();
        //            }
        //        },
        //        () =>
        //        {
        //            foreach (PolygonEntity entity in cloned) entity.Remove();
        //        }
        //    ));
        //    clipboard = clipboardBackup;
        //}

        public void Zoom(object scaleFactor)
        {
            Zoom((double)scaleFactor, new System.Windows.Point(ActualWidth / 2, ActualHeight / 2));
        }

        public void Zoom(double scaleFactor, System.Windows.Point center)
        {
            if (scaleFactor>0
                &&(Coordinates.maxX - Coordinates.minX >= Coordinates.DEFAULT_PATTERN_SIZE * Coordinates.MAX_PATTERN_VIEW ||
                Coordinates.maxY - Coordinates.minY >= Coordinates.DEFAULT_PATTERN_SIZE * Coordinates.MAX_PATTERN_VIEW))
                return;
            double xFactor = (Coordinates.maxX - Coordinates.minX) * scaleFactor,
                yFactor = (Coordinates.maxY - Coordinates.minY) * scaleFactor;
            Coordinates.maxX += xFactor * (ActualWidth - center.X) / ActualWidth;
            Coordinates.minX -= xFactor * center.X / ActualWidth;
            Coordinates.maxY += yFactor * center.Y / ActualHeight;
            Coordinates.minY -= yFactor * (ActualHeight - center.Y) / ActualHeight;
            Coordinates.AdjustRatio();
            UpdateCanvas();
        }

        private void DeleteEntities(IEnumerable<PolygonEntity> entities)
        {
            List<PolygonEntity> target = new List<PolygonEntity>(entities);
            if (target.Count == 0) return;
            Mediator.ExecuteUndoableAction(new Mediator.UndoableAction
            (
                () => {
                    foreach (PolygonEntity entity in target) entity.Restore();
                },
                () => {
                    foreach (PolygonEntity entity in target) entity.Delete();
                },
                () => {
                    foreach (PolygonEntity entity in target) entity.Remove();
                }
            ));

            Mediator.NotifyColleagues("EntityDetails.ShowCells", null);
        }

        private void DrawLine(object obj)
        {
            if (Coordinates.mouseCaptured)
            {
                MessageBox.Show("다른 작업 중입니다.");
                return;
            }
            Coordinates.mouseCaptured = true;
            ClearSelected();
            drawingPolygon = new Polygon
            {
                Fill = Coordinates.transparentBrush,
                Stroke = Coordinates.defaultColorBrush,
            };
            drawingPolygon.StrokeDashArray.Add(5);
            drawingPolygon.StrokeDashArray.Add(5);

            drawingEllipse = new Ellipse
            {
                Fill = Coordinates.transparentBrush,
                Stroke = Coordinates.defaultColorBrush,
                Width = 5,
                Height = 5,
            };

            drawingPolygon.Points.Add(new System.Windows.Point());
            SetLeft(drawingEllipse, -10);
            SetTop(drawingEllipse, -10);

            Children.Add(drawingPolygon);
            Children.Add(drawingEllipse);



            MouseLeftButtonDown -= Select_MouseLeftButtonDown;
            MouseRightButtonDown -= MoveCanvas_MouseRightButtonDown;

            MouseMove += DrawLine_MouseMove;
            MouseLeftButtonUp += DrawLine_MouseLeftButtonUp;
            MouseRightButtonUp += DrawLine_MouseRightButtonUp;

        }
        private void DrawRectangle(object obj)
        {
            if (Coordinates.mouseCaptured)
            {
                MessageBox.Show("다른 작업 중입니다.");
                return;
            }
            Coordinates.mouseCaptured = true;
            ClearSelected();
            drawingPolygon = new Polygon
            {
                Fill = Coordinates.transparentBrush,
                Stroke = Coordinates.defaultColorBrush,
            };
            drawingPolygon.StrokeDashArray.Add(5);
            drawingPolygon.StrokeDashArray.Add(5);

            drawingEllipse = new Ellipse
            {
                Fill = Coordinates.transparentBrush,
                Stroke = Coordinates.defaultColorBrush,
                Width = 5,
                Height = 5,
            };

            drawingPolygon.Points.Add(new System.Windows.Point());
            SetLeft(drawingEllipse, -10);
            SetTop(drawingEllipse, -10);

            Children.Add(drawingPolygon);
            Children.Add(drawingEllipse);



            MouseLeftButtonDown -= Select_MouseLeftButtonDown;
            MouseRightButtonDown -= MoveCanvas_MouseRightButtonDown;

            MouseMove += DrawRectangle_MouseMove;
            MouseLeftButtonUp += DrawRectangle_MouseLeftButtonUp;
            MouseRightButtonUp += DrawRectangle_MouseRightButtonUp;

        }

        private void DrawPolygon(object obj)
        {
            if (Coordinates.mouseCaptured)
            {
                MessageBox.Show("다른 작업 중입니다.");
                return;
            }
            Coordinates.mouseCaptured = true;

            ClearSelected();
            drawingPolygon = new Polygon
            {
                Fill = Coordinates.transparentBrush,
                Stroke = Coordinates.defaultColorBrush,
            };
            drawingPolygon.StrokeDashArray.Add(5);
            drawingPolygon.StrokeDashArray.Add(5);


            drawingEllipse = new Ellipse
            {
                Fill = Coordinates.transparentBrush,
                Stroke = Coordinates.defaultColorBrush,
                Width = 5,
                Height = 5,
            };

            drawingPolygon.Points.Add(new System.Windows.Point());
            SetLeft(drawingEllipse, -10);
            SetTop(drawingEllipse, -10);

            Children.Add(drawingPolygon);
            Children.Add(drawingEllipse);



            MouseLeftButtonDown -= Select_MouseLeftButtonDown;
            MouseRightButtonDown -= MoveCanvas_MouseRightButtonDown;

            MouseMove += DrawPolygon_MouseMove;
            MouseLeftButtonUp += DrawPolygon_MouseLeftButtonUp;
            MouseRightButtonUp += DrawPolygon_MouseRightButtonUp;

        }




        #region 마우스 이벤트

        private void Info_MouseMove(object sender, MouseEventArgs e)
        {
            Mediator.NotifyColleagues("StatusBar.ShowMousePosition", new System.Windows.Point(Coordinates.ToDxfX(e.GetPosition(this).X), Coordinates.ToDxfY(e.GetPosition(this).Y)));
        }

        private void Zoom_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double scaleFactor = 0.1;
            Zoom(e.Delta < 0 ? scaleFactor * 1.1 : -scaleFactor, e.GetPosition(this));

        }


        public void Select_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            if (Coordinates.mouseCaptured) return;
            if (Children.Contains(drawingPolygon)) Children.Remove(drawingPolygon);
            drawingPolygon = new Polygon
            {
                Fill = Coordinates.transparentBrush,
                Stroke = Coordinates.defaultColorBrush,
                StrokeThickness = 0.5
            };
            drawingPolygon.StrokeDashArray.Add(5);
            drawingPolygon.StrokeDashArray.Add(5);

            drawingPolygon.Points.Add(new System.Windows.Point(e.GetPosition(this).X, e.GetPosition(this).Y));
            drawingPolygon.Points.Add(new System.Windows.Point(e.GetPosition(this).X, e.GetPosition(this).Y));
            drawingPolygon.Points.Add(new System.Windows.Point(e.GetPosition(this).X, e.GetPosition(this).Y));
            drawingPolygon.Points.Add(new System.Windows.Point(e.GetPosition(this).X, e.GetPosition(this).Y));




            Children.Add(drawingPolygon);

            MouseMove += Select_MouseMove;
            MouseLeftButtonUp += Select_MouseLeftButtonUp;
        }
        private void Select_MouseMove(object sender, MouseEventArgs e)
        {


            if(e.LeftButton == MouseButtonState.Released)
            {
                Select_MouseLeftButtonUp(sender, e);
                return;
            }
            drawingPolygon.Points[1] = new System.Windows.Point(drawingPolygon.Points[0].X, e.GetPosition(this).Y);
            drawingPolygon.Points[2] = new System.Windows.Point(e.GetPosition(this).X, e.GetPosition(this).Y);
            drawingPolygon.Points[3] = new System.Windows.Point(e.GetPosition(this).X, drawingPolygon.Points[0].Y);
        }
        private void Select_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            if (!Coordinates.mouseActionDone)
            {
                double minSelX = Math.Min(drawingPolygon.Points[2].X, drawingPolygon.Points[0].X),
                       maxSelX = Math.Max(drawingPolygon.Points[2].X, drawingPolygon.Points[0].X),
                       minSelY = Math.Min(drawingPolygon.Points[2].Y, drawingPolygon.Points[0].Y),
                       maxSelY = Math.Max(drawingPolygon.Points[2].Y, drawingPolygon.Points[0].Y);
                if (maxSelX + maxSelY - minSelX - minSelY >= MIN_SELECT_LENGTH && !Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl) && !Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                {
                    ClearSelected();
                }
                // TODO: 영역 안의 폴리곤 선택
                StreamGeometry selectedAreaGeometry = new StreamGeometry();
                using (StreamGeometryContext ctx = selectedAreaGeometry.Open())
                {
                    ctx.BeginFigure(drawingPolygon.Points[0], true /* is filled */, true /* is closed */);
                    ctx.LineTo(drawingPolygon.Points[1], true /* is stroked */, false /* is smooth join */);
                    ctx.LineTo(drawingPolygon.Points[2], true /* is stroked */, false /* is smooth join */);
                    ctx.LineTo(drawingPolygon.Points[3], true /* is stroked */, false /* is smooth join */);
                }

                //List<double> x = new List<double>(), y = new List<double>();
                foreach (PolygonEntity entity in DrawingEntities)
                {
                    if (selectedAreaGeometry.FillContainsWithDetail(entity.geometry, 1, ToleranceType.Relative) == IntersectionDetail.Empty) continue;

                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        entity.ToggleSelected(!entity.selected);
                    }
                    else
                    {
                        entity.ToggleSelected(true);
                    }
                }
            }
            else
            {

                Coordinates.mouseActionDone = false;
            }
            Children.Remove(drawingPolygon);

            MouseMove -= Select_MouseMove;
            MouseLeftButtonUp -= Select_MouseLeftButtonUp;
        }


        private void MoveCanvas_MouseRightButtonDown(object sender, MouseEventArgs e)
        {
            offset = new double[] { e.GetPosition(this).X, e.GetPosition(this).Y, Coordinates.minX, Coordinates.minY };
            MouseMove += MoveCanvas_MouseMove;
            MouseRightButtonUp += MoveCanvas_MouseRightButtonUp;
        }

        private void MoveCanvas_MouseMove(object sender, MouseEventArgs e)
        {

            if (e.RightButton == MouseButtonState.Released)
            {
                MoveCanvas_MouseRightButtonUp(sender, e);
                return;
            }
            if (offset == null) return;
            double dx = (Coordinates.maxX - Coordinates.minX) * (offset[0] - e.GetPosition(this).X) / ActualWidth,
                dy = (Coordinates.maxY - Coordinates.minY) * (e.GetPosition(this).Y - offset[1]) / ActualHeight;

            if ((dx > 0 || Coordinates.CANVAS_MARGIN / Coordinates.ratio + dx + offset[2] > Coordinates.glassLeft)
                && (dx < 0 || dx + Coordinates.maxX - Coordinates.minX + offset[2] < Coordinates.CANVAS_MARGIN / Coordinates.ratio + Coordinates.glassRight))
            {
                Coordinates.maxX = dx + Coordinates.maxX - Coordinates.minX + offset[2];
                Coordinates.minX = dx + offset[2];
            }

            if ((dy > 0 || Coordinates.CANVAS_MARGIN / Coordinates.ratio + dy + offset[3] > Coordinates.glassBottom )
                && ( dy < 0 || dy + Coordinates.maxY - Coordinates.minY + offset[3] < Coordinates.CANVAS_MARGIN / Coordinates.ratio + Coordinates.glassTop))
            {
                Coordinates.maxY = dy + Coordinates.maxY - Coordinates.minY + offset[3];
                Coordinates.minY = dy + offset[3];
            }
            UpdateCanvas();
        }

        private void MoveCanvas_MouseRightButtonUp(object sender, MouseEventArgs e)
        {
            MouseMove -= MoveCanvas_MouseMove;
            offset = null;
        }





        private void DrawLine_MouseMove(object sender, MouseEventArgs e)
        {
            SetLeft(drawingEllipse, e.GetPosition(this).X - 2.5);
            SetTop(drawingEllipse, e.GetPosition(this).Y - 2.5);
            drawingPolygon.Points[drawingPolygon.Points.Count - 1] = new System.Windows.Point(e.GetPosition(this).X, e.GetPosition(this).Y);
        }
        private void DrawLine_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            drawingPolygon.Points.Add(new System.Windows.Point(e.GetPosition(this).X, e.GetPosition(this).Y));

            if (drawingPolygon.Points.Count != 3) return;


            
            MouseMove -= DrawLine_MouseMove;
            MouseLeftButtonUp -= DrawLine_MouseLeftButtonUp;
            MouseRightButtonUp -= DrawLine_MouseRightButtonUp;


            Children.Remove(drawingPolygon);
            Children.Remove(drawingEllipse);
            double minX = Math.Min(Coordinates.ToDxfX(drawingPolygon.Points[0].X), Coordinates.ToDxfX(drawingPolygon.Points[1].X)),
                maxX = Math.Max(Coordinates.ToDxfX(drawingPolygon.Points[0].X), Coordinates.ToDxfX(drawingPolygon.Points[1].X)),
                minY = Math.Min(Coordinates.ToDxfY(drawingPolygon.Points[0].Y), Coordinates.ToDxfY(drawingPolygon.Points[1].Y)),
                maxY = Math.Max(Coordinates.ToDxfY(drawingPolygon.Points[0].Y), Coordinates.ToDxfY(drawingPolygon.Points[1].Y));
            Cell matchingCell = null;
            foreach (Cell cell in cells)
            {
                if (cell.patternLeft > minX || cell.GetPatternRight() < maxX
                    || cell.patternBottom > minY || cell.GetPatternTop() < maxY) continue;
                matchingCell = cell;

                int r = 0, c = 0;
                while (cell.getPatternOffsetX(c+1) < minX - cell.patternLeft) c++;
                while (cell.getPatternOffsetY(r+1) < minY - cell.patternBottom) r++;
                for(int i = 0; i < drawingPolygon.Points.Count; i++)
                {
                    drawingPolygon.Points[i] = new System.Windows.Point(Coordinates.ToDxfX(drawingPolygon.Points[i].X) - cell.getPatternOffsetX(c), Coordinates.ToDxfY(drawingPolygon.Points[i].Y) - cell.getPatternOffsetY(r));
                }

                break;
            }

            if(matchingCell == null)
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("셀 밖에는 도형을 그릴 수 없습니다.");
            }
            else { 
                PolygonEntity polygonEntity = new PolygonEntity(matchingCell, drawingPolygon, PolygonEntityType.LINE);
                Mediator.ExecuteUndoableAction(new Mediator.UndoableAction
                (
                    () => {
                        DrawingEntities.Add(polygonEntity);
                    },
                    () => {
                        DrawingEntities.Remove(polygonEntity);
                        polygonEntity.Delete();
                    },
                    () =>
                    {
                        DrawingEntities.Add(polygonEntity);
                        polygonEntity.Restore();
                    },
                    () =>
                    {
                        polygonEntity.Remove();
                    }
                ));
            }

            UpdateLayout();
            Coordinates.mouseCaptured = false;
            MouseLeftButtonDown += Select_MouseLeftButtonDown;
            MouseRightButtonDown += MoveCanvas_MouseRightButtonDown;

            Mediator.NotifyColleagues("EntityDetails.ShowCells", null);
        }

        private void DrawLine_MouseRightButtonUp(object sender, MouseEventArgs e)
        {

            MouseMove -= DrawLine_MouseMove;
            MouseLeftButtonUp -= DrawLine_MouseLeftButtonUp;
            MouseRightButtonUp -= DrawLine_MouseRightButtonUp;


            Children.Remove(drawingPolygon);
            Children.Remove(drawingEllipse);
            drawingPolygon.Points.Clear();

            UpdateLayout();
            Coordinates.mouseCaptured = false;
            MouseLeftButtonDown += Select_MouseLeftButtonDown;
            MouseRightButtonDown += MoveCanvas_MouseRightButtonDown;
        }



        private void DrawRectangle_MouseMove(object sender, MouseEventArgs e)
        {
            SetLeft(drawingEllipse, e.GetPosition(this).X - 2.5);
            SetTop(drawingEllipse, e.GetPosition(this).Y - 2.5);

            if(drawingPolygon.Points.Count == 1)
            {
                drawingPolygon.Points[0] = new System.Windows.Point(e.GetPosition(this).X, e.GetPosition(this).Y);
            }
            else
            {
                drawingPolygon.Points[1] = new System.Windows.Point(drawingPolygon.Points[0].X, e.GetPosition(this).Y);
                drawingPolygon.Points[2] = new System.Windows.Point(e.GetPosition(this).X, e.GetPosition(this).Y);
                drawingPolygon.Points[3] = new System.Windows.Point(e.GetPosition(this).X, drawingPolygon.Points[0].Y);
            }
        }
        private void DrawRectangle_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            if(drawingPolygon.Points.Count == 1)
            {
                drawingPolygon.Points.Add(new System.Windows.Point(e.GetPosition(this).X, e.GetPosition(this).Y));
                drawingPolygon.Points.Add(new System.Windows.Point(e.GetPosition(this).X, e.GetPosition(this).Y));
                drawingPolygon.Points.Add(new System.Windows.Point(e.GetPosition(this).X, e.GetPosition(this).Y));
            }
            else if(drawingPolygon.Points.Count == 4)
            {
                MouseMove -= DrawRectangle_MouseMove;
                MouseLeftButtonUp -= DrawRectangle_MouseLeftButtonUp;
                MouseRightButtonUp -= DrawRectangle_MouseRightButtonUp;


                double minX = Math.Min(Coordinates.ToDxfX(drawingPolygon.Points[0].X), Coordinates.ToDxfX(drawingPolygon.Points[2].X)),
                    maxX = Math.Max(Coordinates.ToDxfX(drawingPolygon.Points[0].X), Coordinates.ToDxfX(drawingPolygon.Points[2].X)),
                    minY = Math.Min(Coordinates.ToDxfY(drawingPolygon.Points[0].Y), Coordinates.ToDxfY(drawingPolygon.Points[2].Y)),
                    maxY = Math.Max(Coordinates.ToDxfY(drawingPolygon.Points[0].Y), Coordinates.ToDxfY(drawingPolygon.Points[2].Y));
                Cell matchingCell = null;
                foreach (Cell cell in cells)
                {
                    if (cell.patternLeft > minX || cell.GetPatternRight() < maxX
                        || cell.patternBottom > minY || cell.GetPatternTop() < maxY) continue;
                    matchingCell = cell;

                    int r = 0, c = 0;
                    while (cell.getPatternOffsetX(c + 1) < minX - cell.patternLeft) c++;
                    while (cell.getPatternOffsetY(r + 1) < minY - cell.patternBottom) r++;
                    for (int i = 0; i < drawingPolygon.Points.Count; i++)
                    {
                        drawingPolygon.Points[i] = new System.Windows.Point(Coordinates.ToDxfX(drawingPolygon.Points[i].X) - cell.getPatternOffsetX(c), Coordinates.ToDxfY(drawingPolygon.Points[i].Y) - cell.getPatternOffsetY(r));
                    }

                    break;
                }

                if (matchingCell == null)
                {
                    MessageBoxResult result = System.Windows.MessageBox.Show("셀 밖에는 도형을 그릴 수 없습니다.");
                }
                else { 
                PolygonEntity polygonEntity = new PolygonEntity(matchingCell, drawingPolygon, PolygonEntityType.POLYLINE);
                Mediator.ExecuteUndoableAction(new Mediator.UndoableAction
                (
                    () => {
                        DrawingEntities.Add(polygonEntity);
                    },
                    () => {
                        DrawingEntities.Remove(polygonEntity);
                        polygonEntity.Delete();
                    },
                    () =>
                    {
                        DrawingEntities.Add(polygonEntity);
                        polygonEntity.Restore();
                    },
                    () =>
                    {
                        polygonEntity.Remove();
                    }
                ));
                }
                Children.Remove(drawingPolygon);
                Children.Remove(drawingEllipse);
                UpdateLayout();
                Coordinates.mouseCaptured = false;
                MouseLeftButtonDown += Select_MouseLeftButtonDown;
                MouseRightButtonDown += MoveCanvas_MouseRightButtonDown;
            }

            Mediator.NotifyColleagues("EntityDetails.ShowCells", null);
        }

        private void DrawRectangle_MouseRightButtonUp(object sender, MouseEventArgs e)
        {

            MouseMove -= DrawRectangle_MouseMove;
            MouseLeftButtonUp -= DrawRectangle_MouseLeftButtonUp;
            MouseRightButtonUp -= DrawRectangle_MouseRightButtonUp;


            Children.Remove(drawingPolygon);
            Children.Remove(drawingEllipse);
            drawingPolygon.Points.Clear();


            UpdateLayout();
            Coordinates.mouseCaptured = false;
            MouseLeftButtonDown += Select_MouseLeftButtonDown;
            MouseRightButtonDown += MoveCanvas_MouseRightButtonDown;
        }


        private void DrawPolygon_MouseMove(object sender, MouseEventArgs e)
        {
            SetLeft(drawingEllipse, e.GetPosition(this).X - 2.5);
            SetTop(drawingEllipse, e.GetPosition(this).Y - 2.5);
            drawingPolygon.Points[drawingPolygon.Points.Count - 1] = new System.Windows.Point(e.GetPosition(this).X, e.GetPosition(this).Y);
        }
        private void DrawPolygon_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            drawingPolygon.Points.Add(new System.Windows.Point(e.GetPosition(this).X, e.GetPosition(this).Y));
        }

        private void DrawPolygon_MouseRightButtonUp(object sender, MouseEventArgs e)
        {

            MouseMove -= DrawPolygon_MouseMove;
            MouseLeftButtonUp -= DrawPolygon_MouseLeftButtonUp;
            MouseRightButtonUp -= DrawPolygon_MouseRightButtonUp;

            Coordinates.mouseCaptured = false;
            MouseLeftButtonDown += Select_MouseLeftButtonDown;
            MouseRightButtonDown += MoveCanvas_MouseRightButtonDown;

            Children.Remove(drawingPolygon);
            Children.Remove(drawingEllipse);
            drawingPolygon.Points.RemoveAt(drawingPolygon.Points.Count - 1);
            if (drawingPolygon.Points.Count <= 1) return;


            double minX = Coordinates.ToDxfX(drawingPolygon.Points[0].X),
                maxX = Coordinates.ToDxfX(drawingPolygon.Points[0].X),
                minY = Coordinates.ToDxfY(drawingPolygon.Points[0].Y),
                maxY = Coordinates.ToDxfY(drawingPolygon.Points[0].Y);
            for (int i = 1; i < drawingPolygon.Points.Count; i++)
            {
                minX = Math.Min(minX, Coordinates.ToDxfX(drawingPolygon.Points[i].X));
                maxX = Math.Max(maxX, Coordinates.ToDxfX(drawingPolygon.Points[i].X));
                minY = Math.Min(minY, Coordinates.ToDxfY(drawingPolygon.Points[i].Y));
                maxY = Math.Max(maxY, Coordinates.ToDxfY(drawingPolygon.Points[i].Y));
            }
            Cell matchingCell = null;
            foreach (Cell cell in cells)
            {
                if (cell.patternLeft > minX || cell.GetPatternRight() < maxX
                    || cell.patternBottom > minY || cell.GetPatternTop() < maxY) continue;
                matchingCell = cell;

                int r = 0, c = 0;
                while (cell.getPatternOffsetX(c + 1) < minX - cell.patternLeft) c++;
                while (cell.getPatternOffsetY(r + 1) < minY - cell.patternBottom) r++;
                for (int i = 0; i < drawingPolygon.Points.Count; i++)
                {
                    drawingPolygon.Points[i] = new System.Windows.Point(Coordinates.ToDxfX(drawingPolygon.Points[i].X) - cell.getPatternOffsetX(c), Coordinates.ToDxfY(drawingPolygon.Points[i].Y) - cell.getPatternOffsetY(r));
                }

                break;
            }

            if (matchingCell == null)
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("셀 밖에는 도형을 그릴 수 없습니다.");
            }
            else
            {
                PolygonEntity polygonEntity = new PolygonEntity(matchingCell, drawingPolygon, PolygonEntityType.POLYLINE);
                Mediator.ExecuteUndoableAction(new Mediator.UndoableAction
                (
                    () => {
                        DrawingEntities.Add(polygonEntity);
                    },
                    () => {
                        DrawingEntities.Remove(polygonEntity);
                        polygonEntity.Delete();
                    },
                    () =>
                    {
                        DrawingEntities.Add(polygonEntity);
                        polygonEntity.Restore();
                    },
                    () =>
                    {
                        polygonEntity.Remove();
                    }
                ));

            }
            UpdateLayout();

            Mediator.NotifyColleagues("EntityDetails.ShowCells", null);
        }


        #endregion
    }


}