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
            //CellTreeView.Items.Clear();
            //for(int i=0;i< Coordinates.CanvasRef.cells.Count;i++)
            //{
            //    Cell c = Coordinates.CanvasRef.cells[i];
            //    if (!cellDictionary.ContainsKey(c))
            //    {
            //        TreeViewItem item = new TreeViewItem();
            //        StackPanel panel;
            //        TextBlock title;
            //        TextBox content;
            //        Binding binding;
            //        item.Header = c.name;


            //        panel = new StackPanel
            //        {
            //            Orientation = Orientation.Horizontal
            //        };

            //        title = new TextBlock
            //        {
            //            Text = "Left : "
            //        };
            //        content = new TextBox
            //        {
            //            UndoLimit = 0
            //        };
            //        content.KeyDown += TextBox_KeyDown;
            //        binding = new Binding("PatternLeft")
            //        {
            //            Source = c,
            //            Mode = BindingMode.TwoWay,
            //            UpdateSourceTrigger = UpdateSourceTrigger.LostFocus,
            //        };
            //        content.SetBinding(TextBox.TextProperty, binding);
            //        content.KeyDown += CellTextBoxKeyDown;
            //        panel.Children.Add(title);
            //        panel.Children.Add(content);
            //        item.Items.Add(panel);


            //        panel = new StackPanel
            //        {
            //            Orientation = Orientation.Horizontal
            //        };

            //        title = new TextBlock
            //        {
            //            Text = "Bottom : "
            //        };
            //        content = new TextBox
            //        {
            //            UndoLimit = 0
            //        };
            //        content.KeyDown += TextBox_KeyDown;
            //        binding = new Binding("PatternBottom")
            //        {
            //            Source = c,
            //            Mode = BindingMode.TwoWay,
            //            UpdateSourceTrigger = UpdateSourceTrigger.LostFocus,
            //        };
            //        content.SetBinding(TextBox.TextProperty, binding);
            //        content.KeyDown += CellTextBoxKeyDown;
            //        panel.Children.Add(title);
            //        panel.Children.Add(content);
            //        item.Items.Add(panel);


            //        panel = new StackPanel
            //        {
            //            Orientation = Orientation.Horizontal
            //        };

            //        title = new TextBlock
            //        {
            //            Text = "Width : "
            //        };
            //        content = new TextBox
            //        {
            //            UndoLimit = 0
            //        };
            //        content.KeyDown += TextBox_KeyDown;
            //        binding = new Binding("PatternWidth")
            //        {
            //            Source = c,
            //            Mode = BindingMode.TwoWay,
            //            UpdateSourceTrigger = UpdateSourceTrigger.LostFocus,
            //        };
            //        content.SetBinding(TextBox.TextProperty, binding);
            //        content.KeyDown += CellTextBoxKeyDown;
            //        panel.Children.Add(title);
            //        panel.Children.Add(content);
            //        item.Items.Add(panel);


            //        panel = new StackPanel
            //        {
            //            Orientation = Orientation.Horizontal
            //        };

            //        title = new TextBlock
            //        {
            //            Text = "Height : "
            //        };
            //        content = new TextBox
            //        {
            //            UndoLimit = 0
            //        };
            //        content.KeyDown += TextBox_KeyDown;
            //        binding = new Binding("PatternHeight")
            //        {
            //            Source = c,
            //            Mode = BindingMode.TwoWay,
            //            UpdateSourceTrigger = UpdateSourceTrigger.LostFocus,
            //        };
            //        content.SetBinding(TextBox.TextProperty, binding);
            //        content.KeyDown += CellTextBoxKeyDown;
            //        panel.Children.Add(title);
            //        panel.Children.Add(content);
            //        item.Items.Add(panel);


            //        panel = new StackPanel
            //        {
            //            Orientation = Orientation.Horizontal
            //        };

            //        title = new TextBlock
            //        {
            //            Text = "Repetition in X : "
            //        };
            //        content = new TextBox
            //        {
            //            UndoLimit = 0
            //        };
            //        content.KeyDown += TextBox_KeyDown;
            //        binding = new Binding("PatternCols")
            //        {
            //            Source = c,
            //            Mode = BindingMode.TwoWay,
            //            UpdateSourceTrigger = UpdateSourceTrigger.LostFocus,
            //        };
            //        content.SetBinding(TextBox.TextProperty, binding);
            //        content.KeyDown += CellTextBoxKeyDown;
            //        panel.Children.Add(title);
            //        panel.Children.Add(content);
            //        item.Items.Add(panel);


            //        panel = new StackPanel
            //        {
            //            Orientation = Orientation.Horizontal
            //        };

            //        title = new TextBlock
            //        {
            //            Text = "Repetition in Y : "
            //        };
            //        content = new TextBox
            //        {
            //            UndoLimit = 0
            //        };
            //        content.KeyDown += TextBox_KeyDown;
            //        binding = new Binding("PatternRows")
            //        {
            //            Source = c,
            //            Mode = BindingMode.TwoWay,
            //            UpdateSourceTrigger = UpdateSourceTrigger.LostFocus,
            //        };
            //        content.SetBinding(TextBox.TextProperty, binding);
            //        content.KeyDown += CellTextBoxKeyDown;
            //        panel.Children.Add(title);
            //        panel.Children.Add(content);
            //        item.Items.Add(panel);

            //        cellDictionary.Add(c, item);
            //    }
            //    TreeViewItem cellViewItem = cellDictionary[c];
            //    CellTreeView.Items.Add(cellViewItem);
            //}
        }


        public void ShowEntityComboBox(object obj)
        {
            // ch_test
            ShowCells(null);
            EntityDetailComboBox.ItemsSource = null;
            EntityDetailComboBox.ItemsSource = selectedEntities;

            //EntityDetailComboBox.Items.Clear();

            //foreach (PolygonEntity entity in Coordinates.CanvasRef.DrawingEntities)
            //{
            //    if (entity.Selected == false) continue;


            //    ComboBoxItem item = new ComboBoxItem(

            //    );
            //    item.Content = entity.GetEntityObject().Handle;


            //    propertyEntityObject = entity.GetEntityObject();

            //    EntityDetailComboBox.Items.Add(item);
            //    EntityDetailComboBox.SelectedItem = item;
            //}

            EntityDetailComboBox.SelectedIndex = EntityDetailComboBox.Items.Count - 1;

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
            ////PropertyStackPanel.Children.Clear();

            //if (EntityDetailComboBox.SelectedItem != null)
            //{
            //    string selectedItem = (string)obj;


            //    foreach (PolygonEntity entity in Coordinates.CanvasRef.DrawingEntities)
            //    {
            //        if (entity.GetEntityObject().Handle != selectedItem) continue;

            //        propertyEntity = entity;
            //        propertyEntityObject = entity.GetEntityObject();

            //    }

            //    //Color.Text = "R:" + propertyEntityObject.Color.R.ToString() + " G:" + propertyEntityObject.Color.G.ToString() + " B:"
            //    //    + propertyEntityObject.Color.B.ToString();

            //    if (propertyEntityObject.Color.R == 255)
            //    {
            //        ColorComboBox.SelectedItem = typeof(Colors).GetProperty("Red");
            //        propertyEntity.path.Fill = Brushes.Red;
            //    }
            //    else if (propertyEntityObject.Color.G == 255)
            //    {
            //        ColorComboBox.SelectedItem = typeof(Colors).GetProperty("Green");
            //        propertyEntity.path.Fill = Brushes.Green;
            //    }
            //    else if (propertyEntityObject.Color.B == 255)
            //    {
            //        ColorComboBox.SelectedItem = typeof(Colors).GetProperty("Blue");
            //        propertyEntity.path.Fill = Brushes.Blue;
            //    }


            //    Color_type.Text = propertyEntityObject.Color.ToString();

            //    Handle.Text = propertyEntityObject.Handle;

            //    Layer.Text = propertyEntityObject.Layer.Name;

            //    Line_type.Text = propertyEntityObject.Linetype.Name;

            //    Line_weight.Text = propertyEntityObject.Lineweight.ToString();

            //    Line_Type_scale.Text = propertyEntityObject.LinetypeScale.ToString();

            //    Name.Text = propertyEntityObject.CodeName;

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
            //    //VertexesIndexListView.ItemsSource = indexdxfCoords;
            //    // VertexesListView.ItemsSource = propertyEntity.dxfCoords;

            //    /*
            //    TextBlock textBlock = new TextBlock();
            //    textBlock.Text = "Name";
            //    textBlock.Background = Brushes.White;
            //    textBlock.Margin = new Thickness(1);

            //    TextBlock textBlock2  = new TextBlock();
            //    textBlock2.Text = "Color";
            //    textBlock2.Background = Brushes.White;
            //    //PropertyStackPanel.Children.Add();
            //    //textBlock.Text = "Color";
            //    //PropertyStackPanel.Children.Add(textBlock);
            //    //PropertyStackPanel.Children.Add(textBlock2);
            //    //entityDictionary[selectedItem];*/
            //}
        }



        public void ClearEntityProperties(object obj)
        {

        }


        private void SelectEntityProperties(object sender, SelectionChangedEventArgs e)
        {
            //if (EntityDetailComboBox.SelectedItem == null)
            //    return;
            CommonValueStackPanel.DataContext = propertyEntity = (PolygonEntity)(sender as ComboBox).SelectedItem;


            //string selectedItem = ((ComboBoxItem)EntityDetailComboBox.SelectedItem).Content.ToString();
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

        private void Color_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
