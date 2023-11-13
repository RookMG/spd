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
using System.Globalization;

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

        public EntityDetails()
        {
            InitializeComponent();

            entityDictionary = new Dictionary<string, PolygonEntity>();
            cellDictionary = new Dictionary<Cell, TreeViewItem>();


            Mediator.Register("EntityDetails.ShowCells", ShowCells);

            Mediator.Register("EntityDetails.ShowEntityTypes", ShowEntityTypes);
            Mediator.Register("EntityDetails.ShowEntityComboBox", ShowEntityComboBox);
            Mediator.Register("EntityDetails.ShowEntityPropertyDetail", ShowEntityPropertyDetail);

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
                    content.Text = c.patternLeft.ToString();
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
                    content.Text = c.patternBottom.ToString();
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
                    content.Text = c.patternWidth.ToString();
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
                    content.Text = c.patternHeight.ToString();
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
                    content.Text = c.patternCols.ToString();
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
                    content.Text = c.patternRows.ToString();
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
            EntityDetailComboBox.Items.Clear();

            foreach (PolygonEntity entity in Coordinates.CanvasRef.DrawingEntities)
            {
                if (entity.Selected == false) continue;


                ComboBoxItem item = new ComboBoxItem(

                    );
                item.Content = entity.GetEntityObject().Handle;


                propertyEntityObject = entity.GetEntityObject();
                //entityDictionary.Add(entity., entity);
                //entity.debug_ch();

                EntityDetailComboBox.Items.Add(item);
                EntityDetailComboBox.SelectedItem = item;
            }

        }

        private void ShowEntityProperties(object obj, SelectionChangedEventArgs e)
        {
            //PropertyStackPanel.Children.Clear();

            if (EntityDetailComboBox.SelectedItem != null)
            {
                string selectedItem = ((ComboBoxItem)EntityDetailComboBox.SelectedItem).Content.ToString();

                PolygonEntity propertyEntity = null;
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

                /*
                TextBlock textBlock = new TextBlock();
                textBlock.Text = "Name";
                textBlock.Background = Brushes.White;
                textBlock.Margin = new Thickness(1);

                TextBlock textBlock2  = new TextBlock();
                textBlock2.Text = "Color";
                textBlock2.Background = Brushes.White;
                //PropertyStackPanel.Children.Add();
                //textBlock.Text = "Color";
                //PropertyStackPanel.Children.Add(textBlock);
                //PropertyStackPanel.Children.Add(textBlock2);
                //entityDictionary[selectedItem];*/
            }
        }


        public void ShowEntityPropertyDetail(object obj)
        {
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
