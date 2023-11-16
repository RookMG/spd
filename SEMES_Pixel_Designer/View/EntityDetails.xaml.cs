using SEMES_Pixel_Designer.Utils;
using System;
using System.Collections.Generic;
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
using netDxf;
using netDxf.Collections;
using netDxf.Entities;
using static SEMES_Pixel_Designer.Utils.PolygonEntity;
using netDxf.Tables;
using System.Reflection;
using System.Windows.Markup;

namespace SEMES_Pixel_Designer
{
    /// <summary>
    /// EntityDetails.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class EntityDetails : Page
    {
        Dictionary<string, PolygonEntity> entityDictionary;
        Dictionary<Cell, TreeViewItem> cellDictionary;

        private EntityObject propertyEntityObject = null;
        private PolygonEntity propertyEntity = null;

        public class CoordInfo
        {
            public int idx { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
            public CoordInfo(int idx, System.Windows.Point p)
            {
                this.idx = idx;
                X = p.X;
                Y = p.Y;
            }
        }
        public EntityDetails()
        {
            InitializeComponent();

            entityDictionary = new Dictionary<string, PolygonEntity>();
            cellDictionary = new Dictionary<Cell, TreeViewItem>();

            Utils.Mediator.Register("EntityDetails.ShowEntityComboBox", ShowEntityComboBox);
            Utils.Mediator.Register("EntityDetails.ShowEntityProperties", ShowEntityProperties);
            Utils.Mediator.Register("EntityDetails.ShowCells", ShowCells);

            ColorComboBox.Items.Clear();
            ColorComboBox.ItemsSource = typeof(Colors).GetProperties().Where(p => p.PropertyType == typeof(Color) && (p.Name == "Red" || p.Name == "Blue" || p.Name == "Green")).ToList();
            AddCellButton.DataContext = new CommandDataContext();
            //SetCellButton.DataContext = new CommandDataContext();
        }
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                FocusManager.SetFocusedElement(FocusManager.GetFocusScope((TextBox)sender), null);
                Keyboard.ClearFocus();
            }
        }

        public void ShowCells(object obj)
        {
            CellListView.ItemsSource = null;
            CellListView.ItemsSource = Coordinates.CanvasRef.cells;
            
        }


        public void ShowEntityComboBox(object obj)
        {
            // ch_test
            ShowCells(null);
            EntityDetailComboBox.ItemsSource = null;
            EntityDetailComboBox.ItemsSource = selectedEntities;

            EntityDetailComboBox.SelectedIndex = EntityDetailComboBox.Items.Count - 1;
            if (selectedEntities.Count > 0)
            {
                ColorComboBox.SelectedIndex = 2 - int.Parse(selectedEntities[selectedEntities.Count - 1].GetEntityObject().Color.ToString()) / 2;
            }
            ShowEntityProperties(null);
        }

        private void ColorChange(object obj, SelectionChangedEventArgs e)
        {
            if (propertyEntity != null && ColorComboBox.SelectedItem != null)
            {
                if (ColorComboBox.SelectedItem is PropertyInfo selectedColor)
                {
                    string selectedColorName = selectedColor.Name;

                    if(selectedColorName == "Red")
                    {
                        AciColor myColor = new AciColor(255, 0, 0);
                        propertyEntity.Color_type = myColor;
                        propertyEntity.path.Fill = Brushes.Red;
                    }
                    else if (selectedColorName == "Green")
                    {
                        AciColor myColor = new AciColor(0, 255, 0);
                        propertyEntity.Color_type = myColor;
                        propertyEntity.path.Fill = Brushes.Green;
                    }
                    else if (selectedColorName == "Blue")
                    {
                        AciColor myColor = new AciColor(0, 0, 255);
                        propertyEntity.Color_type = myColor;
                        propertyEntity.path.Fill = Brushes.Blue;
                    }
                }
            }
        }
        public void ShowEntityProperties(object obj)
        {
           

            if(propertyEntity == null)
            {
                VertexesListView.ItemsSource = null;
                return;
            }

            List<CoordInfo> dxfCoordsInfo = new List<CoordInfo>();
            for (int i = 0; i < propertyEntity.dxfCoords.Count; i++)
            {
                dxfCoordsInfo.Add(new CoordInfo(i, propertyEntity.dxfCoords[i]));
            }

            VertexesListView.ItemsSource = dxfCoordsInfo;
        }



        public void ClearEntityProperties(object obj)
        {

        }


        private void SelectEntityProperties(object sender, SelectionChangedEventArgs e)
        {
            CommonValueStackPanel.DataContext = propertyEntity = (PolygonEntity)(sender as ComboBox).SelectedItem;


            ShowEntityProperties(null);
        }
        
        private void XTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            EditCoordi(sender, true);
            FocusManager.SetFocusedElement(FocusManager.GetFocusScope((TextBox)sender), null);
            Keyboard.ClearFocus();
        }

        private void YTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            EditCoordi(sender, false);
            FocusManager.SetFocusedElement(FocusManager.GetFocusScope((TextBox)sender), null);
            Keyboard.ClearFocus();
        }

        private void XTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                EditCoordi(sender, true);
                e.Handled = true;
                FocusManager.SetFocusedElement(FocusManager.GetFocusScope((TextBox)sender), null);
                Keyboard.ClearFocus();
            }
        }

        private void YTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                EditCoordi(sender, false);
                e.Handled = true;
                FocusManager.SetFocusedElement(FocusManager.GetFocusScope((TextBox)sender), null);
                Keyboard.ClearFocus();
            }
        }


        private void EditCoordi(object sender, bool isX)
        {
            if (sender is TextBox textBox && textBox.DataContext is CoordInfo coord)
            {

                ListViewItem listViewItem = FindParent<ListViewItem>(textBox);

                ListView listView = FindParent<ListView>(listViewItem);
                string coordiString = textBox.Text;
                double coordiReal;
                if (double.TryParse(coordiString, out coordiReal))
                {
                    if (isX == true)
                        EditCoordiX(coord.idx, coordiReal);
                    else
                        EditCoordiY(coord.idx, coordiReal);
                }
                else
                {
                    MessageBox.Show("WRONG VALUE!!");
                }

            }
        }


        private void EditCoordiX(int index, double coordiReal)
        {
            double from = propertyEntity.dxfCoords[index].X, to = coordiReal;
            if (from == to) return;
            Mediator.ExecuteUndoableAction(new Mediator.UndoableAction
            (
                "도형 모양 변경",
                () => {
                    propertyEntity.UpdatePoint(from, propertyEntity.dxfCoords[index].Y, index, true);
                    propertyEntity.ReDraw();
                },
                () => {
                    propertyEntity.UpdatePoint(to, propertyEntity.dxfCoords[index].Y, index, true);
                    propertyEntity.ReDraw();
                },
                () =>
                {
                }
            ));
        }

        private void EditCoordiY(int index, double coordiReal)
        {
            double from = propertyEntity.dxfCoords[index].Y, to = coordiReal;
            if (from == to) return;
            Mediator.ExecuteUndoableAction(new Mediator.UndoableAction
            (
                "도형 모양 변경",
                () => {
                    propertyEntity.UpdatePoint(propertyEntity.dxfCoords[index].X, from, index, true);
                    propertyEntity.ReDraw();
                },
                () => {
                    propertyEntity.UpdatePoint(propertyEntity.dxfCoords[index].X, to, index, true);
                    propertyEntity.ReDraw();
                },
                () =>
                {
                }
            ));
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null)
            {
                return null;
            }

            T parent = parentObject as T;
            return parent ?? FindParent<T>(parentObject);
        }
        
        private void CellTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox textBox = (TextBox)sender;

                textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

                e.Handled = true;
            }
        }
        private string PolygonTypeToString(PolygonEntity entity)
        {
            if (entity.GetPolygonType() == PolygonEntityType.DOT)
            {
                return "Dot";
            }
            else if (entity.GetPolygonType() == PolygonEntityType.LINE)
            {
                return "Line";
            }
            else if (entity.GetPolygonType() == PolygonEntityType.POLYLINE)
            {
                return "PolyLine";
            }
            else
                return null;
        }

        private void SetCellClick(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            StackPanel stackPanel = (StackPanel)button.Parent;
            Cell dataContext = (Cell)stackPanel.DataContext;
            Mediator.NotifyColleagues("MainWindow.SetCell", dataContext);
        }

        private void DeleteCellClick(object sender, RoutedEventArgs e)
        {

            Button button = (Button)sender;
            StackPanel stackPanel = (StackPanel)button.Parent;
            Cell cell = (Cell)stackPanel.DataContext;
            int idx = Coordinates.CanvasRef.cells.IndexOf(cell);
            Layer layer = MainWindow.doc.Layers[cell.Name];
            List<PolygonEntity> childrenEntities = new List<PolygonEntity>();
            foreach (PolygonEntity entity in Coordinates.CanvasRef.DrawingEntities)
            {
                if (entity.cell == cell) childrenEntities.Add(entity);
            }
            if (childrenEntities.Count > 0)
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("셀 삭제시 셀 안의 도형이 같이 삭제됩니다. 정말 삭제하시겠습니까?", "셀 삭제", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No) return;
            }
            Mediator.ExecuteUndoableAction(new Mediator.UndoableAction
            (
                "셀 삭제",
                () => {
                    MainWindow.doc.Layers.Add(layer);
                    cell.Restore();
                    foreach (PolygonEntity entity in childrenEntities)
                    {
                        entity.Restore();
                        Coordinates.CanvasRef.DrawingEntities.Add(entity);
                    }
                    Coordinates.CanvasRef.cells.Insert(idx, cell);
                    Mediator.NotifyColleagues("EntityDetails.ShowEntityComboBox", null);
                    Coordinates.CanvasRef.UpdateCanvas();
                },
                () => {
                    Console.WriteLine(MainWindow.doc.Layers.Count);
                    cell.Delete();
                    foreach (PolygonEntity entity in childrenEntities)
                    {
                        entity.Delete();
                        Coordinates.CanvasRef.DrawingEntities.Remove(entity);
                    }
                    Coordinates.CanvasRef.cells.Remove(cell);
                    MainWindow.doc.Layers.Remove(layer);
                    Mediator.NotifyColleagues("EntityDetails.ShowEntityComboBox", null);
                    Coordinates.CanvasRef.UpdateCanvas();
                    Console.WriteLine(MainWindow.doc.Layers.Count);
                },
                () =>
                {
                    cell.Remove();
                }
            ));
        }

    }
}
