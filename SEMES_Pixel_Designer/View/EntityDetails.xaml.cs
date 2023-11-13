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

        public EntityDetails()
        {
            InitializeComponent();

            entityDictionary = new Dictionary<string, PolygonEntity>();
            cellDictionary = new Dictionary<Cell, TreeViewItem>();


            Utils.Mediator.Register("EntityDetails.ShowEntityTypes", ShowEntityTypes);
            Utils.Mediator.Register("EntityDetails.ShowEntityComboBox", ShowEntityComboBox);
            Utils.Mediator.Register("EntityDetails.ShowEntityProperties", ShowEntityProperties);

        }


        public void ShowCells(object obj)
        {
            CellTreeView.Items.Clear();
            for(int i=0;i< Coordinates.CanvasRef.cells.Count;i++)
            {
                Cell c = Coordinates.CanvasRef.cells[i];
                if (!cellDictionary.ContainsKey(c))
                {
                    TreeViewItem item = new TreeViewItem();
                    StackPanel panel;
                    TextBlock title;
                    TextBox content;
                    Binding binding;
                    item.Header = c.name;


                    panel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal
                    };

                    title = new TextBlock
                    {
                        Text = "Left : "
                    };
                    content = new TextBox();
                    binding = new Binding("PatternLeft")
                    {
                        Source = c,
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    };
                    content.SetBinding(TextBox.TextProperty, binding);
                    panel.Children.Add(title);
                    panel.Children.Add(content);
                    item.Items.Add(panel);


                    panel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal
                    };

                    title = new TextBlock
                    {
                        Text = "Bottom : "
                    };
                    content = new TextBox();
                    binding = new Binding("PatternBottom")
                    {
                        Source = c,
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    };
                    content.SetBinding(TextBox.TextProperty, binding);
                    panel.Children.Add(title);
                    panel.Children.Add(content);
                    item.Items.Add(panel);


                    panel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal
                    };

                    title = new TextBlock
                    {
                        Text = "Width : "
                    };
                    content = new TextBox();
                    binding = new Binding("PatternWidth")
                    {
                        Source = c,
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    };
                    content.SetBinding(TextBox.TextProperty, binding);
                    panel.Children.Add(title);
                    panel.Children.Add(content);
                    item.Items.Add(panel);


                    panel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal
                    };

                    title = new TextBlock
                    {
                        Text = "Height : "
                    };
                    content = new TextBox();
                    binding = new Binding("PatternHeight")
                    {
                        Source = c,
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    };
                    content.SetBinding(TextBox.TextProperty, binding);
                    panel.Children.Add(title);
                    panel.Children.Add(content);
                    item.Items.Add(panel);


                    panel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal
                    };

                    title = new TextBlock
                    {
                        Text = "Repetition in X : "
                    };
                    content = new TextBox();
                    binding = new Binding("PatternCols")
                    {
                        Source = c,
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    };
                    content.SetBinding(TextBox.TextProperty, binding);
                    panel.Children.Add(title);
                    panel.Children.Add(content);
                    item.Items.Add(panel);


                    panel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal
                    };

                    title = new TextBlock
                    {
                        Text = "Repetition in Y : "
                    };
                    content = new TextBox();
                    binding = new Binding("PatternRows")
                    {
                        Source = c,
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    };
                    content.SetBinding(TextBox.TextProperty, binding);
                    panel.Children.Add(title);
                    panel.Children.Add(content);
                    item.Items.Add(panel);

                    cellDictionary.Add(c, item);
                }
                TreeViewItem cellViewItem = cellDictionary[c];
                CellTreeView.Items.Add(cellViewItem);
            }
        }


        public void ShowEntityTypes(object obj)
        {
            TreeViewItem entities = new TreeViewItem();

            foreach (PolygonEntity entity in Coordinates.CanvasRef.DrawingEntities)
            {
                CheckBox checkBox = new CheckBox { };

                checkBox.Content = PolygonTypeToString(entity);

                if (checkBox.Content == null)
                    continue;

                Binding binding = new Binding("Selected")
                {
                    Source = entity,
                    Mode = BindingMode.TwoWay
                };


                checkBox.SetBinding(CheckBox.IsCheckedProperty, binding);
                entities.Items.Add(checkBox);
            }

            EntityTreeView.Items.Clear();

            if (entities.Items.Count != 0)
                entities.Header = "Entities";

            EntityTreeView.Items.Add(entities);

        }

        public void ShowEntityComboBox(object obj)
        {
            // ch_test
            ShowCells(null);

            EntityDetailComboBox.Items.Clear();

            foreach (PolygonEntity entity in Coordinates.CanvasRef.DrawingEntities)
            {
                if (entity.Selected == false) continue;


                ComboBoxItem item = new ComboBoxItem(

                    );
                item.Content = entity.GetEntityObject().Handle;


                propertyEntityObject = entity.GetEntityObject();

                EntityDetailComboBox.Items.Add(item);
                EntityDetailComboBox.SelectedItem = item;
            }

            if (EntityDetailComboBox.Items.Count > 0)
            {
                EntityDetailComboBox.SelectedIndex = 0;
            }

        }

        public void ShowEntityProperties(object obj)
        {
            //PropertyStackPanel.Children.Clear();

            if (EntityDetailComboBox.SelectedItem != null)
            {
                string selectedItem = (string)obj;

                
                foreach (PolygonEntity entity in Coordinates.CanvasRef.DrawingEntities)
                {
                    if (entity.GetEntityObject().Handle != selectedItem) continue;

                    propertyEntity = entity;
                    propertyEntityObject = entity.GetEntityObject();

                }

                Color.Text = "R:" + propertyEntityObject.Color.R.ToString() + " G:" + propertyEntityObject.Color.G.ToString() + " B:"
                    + propertyEntityObject.Color.B.ToString();

                Color_type.Text = propertyEntityObject.Color.ToString();

                Handle.Text = propertyEntityObject.Handle;

                Layer.Text = propertyEntityObject.Layer.Name;

                Line_type.Text = propertyEntityObject.Linetype.Name;

                Line_weight.Text = propertyEntityObject.Lineweight.ToString();

                Line_Type_scale.Text = propertyEntityObject.LinetypeScale.ToString();

                Name.Text = propertyEntityObject.CodeName;

                
                List<string> indexdxfCoords = new List<string>();
                for (int i = 0; i < propertyEntity.dxfCoords.Count; i++)
                {
                    indexdxfCoords.Add(i.ToString());
                }

                VertexesIndexListView.ItemsSource = indexdxfCoords;
                VertexesListView.ItemsSource = propertyEntity.dxfCoords;
            }
        }



        public void ClearEntityProperties(object obj)
        {

        }


        private void SelectEntityProperties(object obj, SelectionChangedEventArgs e)
        {
            if (EntityDetailComboBox.SelectedItem == null)
                return;

            string selectedItem = ((ComboBoxItem)EntityDetailComboBox.SelectedItem).Content.ToString();
            ShowEntityProperties(selectedItem);
        }

        private void XTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            EditCoordi(sender, true);
        }

        private void YTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            EditCoordi(sender, false);
        }

        private void XTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                EditCoordi(sender, true);
                e.Handled = true;
            }
        }

        private void YTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                EditCoordi(sender, false);
                e.Handled = true;
            }
        }


        private void EditCoordi(object sender, bool isX)
        {
            if (sender is TextBox textBox && textBox.DataContext is System.Windows.Point indexCoordi)
            {


                ListViewItem listViewItem = FindParent<ListViewItem>(textBox);

                ListView listView = FindParent<ListView>(listViewItem);
                int index = listView.ItemContainerGenerator.IndexFromContainer(listViewItem);

                if (listViewItem != null)
                {
                    string coordiString = textBox.Text;
                    double coordiReal;
                    if (double.TryParse(coordiString, out coordiReal))
                    {
                        if(isX == true)
                            EditCoordiX(index, coordiReal);
                        else
                            EditCoordiY(index, coordiReal);
                    }
                    else
                    {
                        MessageBox.Show("WRONG VALUE!!");
                    }
                }
            }
        }


        private void EditCoordiX(int index, double coordiReal)
        {
            propertyEntity.dxfCoords[index] = new System.Windows.Point(coordiReal, propertyEntity.dxfCoords[index].Y);
            propertyEntity.ReDraw();
        }

        private void EditCoordiY(int index, double coordiReal)
        {
            propertyEntity.dxfCoords[index] = new System.Windows.Point(propertyEntity.dxfCoords[index].X, coordiReal);
            propertyEntity.ReDraw();
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
    }
}
