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

namespace SEMES_Pixel_Designer
{
    /// <summary>
    /// EntityDetails.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class EntityDetails : Page
    {
        Dictionary<string, PolygonEntity> entityDictionary;

        private EntityObject propertyEntityObject = null;
        private PolygonEntity propertyEntity = null;

        public EntityDetails()
        {
            InitializeComponent();

            entityDictionary = new Dictionary<string, PolygonEntity>();


            Utils.Mediator.Register("EntityDetails.ShowEntityTypes", ShowEntityTypes);
            Utils.Mediator.Register("EntityDetails.ShowEntityComboBox", ShowEntityComboBox);
            Utils.Mediator.Register("EntityDetails.ShowEntityPropertyDetail", ShowEntityPropertyDetail);

            ColorComboBox.Items.Clear();
            ColorComboBox.ItemsSource = typeof(Colors).GetProperties().Where(p => p.PropertyType == typeof(Color) && (p.Name == "Red" || p.Name == "Blue" || p.Name == "Green")).ToList();

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

        private void ColorChange(object obj, SelectionChangedEventArgs e)
        {
            if (propertyEntityObject != null && propertyEntity != null && ColorComboBox.SelectedItem != null)
            {
                if (ColorComboBox.SelectedItem is PropertyInfo selectedColor)
                {
                    string selectedColorName = selectedColor.Name;

                    if(selectedColorName == "Red")
                    {
                        AciColor myColor = new AciColor(255, 0, 0);
                        propertyEntityObject.Color = myColor;
                        propertyEntity.path.Fill = Brushes.Red;
                    }
                    else if (selectedColorName == "Green")
                    {
                        AciColor myColor = new AciColor(0, 255, 0);
                        propertyEntityObject.Color = myColor;
                        propertyEntity.path.Fill = Brushes.Green;
                    }
                    else if (selectedColorName == "Blue")
                    {
                        AciColor myColor = new AciColor(0, 0, 255);
                        propertyEntityObject.Color = myColor;
                        propertyEntity.path.Fill = Brushes.Blue;
                    }
                }
            }
        }
        private void ShowEntityProperties(object obj, SelectionChangedEventArgs e)
        {
            //PropertyStackPanel.Children.Clear();

            if (EntityDetailComboBox.SelectedItem != null)
            {
                string selectedItem = ((ComboBoxItem)EntityDetailComboBox.SelectedItem).Content.ToString();

                
                foreach (PolygonEntity entity in Coordinates.CanvasRef.DrawingEntities)
                {
                    if (entity.GetEntityObject().Handle != selectedItem) continue;

                    propertyEntity = entity;
                    propertyEntityObject = entity.GetEntityObject();

                }

                //Color.Text = "R:" + propertyEntityObject.Color.R.ToString() + " G:" + propertyEntityObject.Color.G.ToString() + " B:"
                //    + propertyEntityObject.Color.B.ToString();

                if (propertyEntityObject.Color.R == 255)
                {
                    ColorComboBox.SelectedItem = typeof(Colors).GetProperty("Red");
                    propertyEntity.path.Fill = Brushes.Red;
                }
                else if (propertyEntityObject.Color.G == 255)
                {
                    ColorComboBox.SelectedItem = typeof(Colors).GetProperty("Green");
                    propertyEntity.path.Fill = Brushes.Green;
                }
                else if (propertyEntityObject.Color.B == 255)
                {
                    ColorComboBox.SelectedItem = typeof(Colors).GetProperty("Blue");
                    propertyEntity.path.Fill = Brushes.Blue;
                }


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

                //VertexesIndexListView.ItemsSource = indexdxfCoords;
                //VertexesListView.ItemsSource = propertyEntity.dxfCoords;

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

        private void Color_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
