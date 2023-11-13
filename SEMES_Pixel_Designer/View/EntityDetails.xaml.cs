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
            Utils.Mediator.Register("EntityDetails.ShowEntityProperties", ShowEntityProperties);
            Utils.Mediator.Register("EntityDetails.ShowEntityPropertyDetail", ShowEntityPropertyDetail);

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

        public void ShowEntityPropertyDetail(object obj)
        {
        }

        private void SelectEntityProperties(object obj, SelectionChangedEventArgs e)
        {
            if (EntityDetailComboBox.SelectedItem == null)
                return;

            string selectedItem = ((ComboBoxItem)EntityDetailComboBox.SelectedItem).Content.ToString();
            ShowEntityProperties(selectedItem);
        }

        private void MyTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // 텍스트 입력이 완료되었을 때 실행될 코드
            if(sender is TextBox textBox)
            {
                string coordiString = textBox.Text;
                double coordiReal;
                if (double.TryParse(coordiString, out coordiReal))
                {
                    //MessageBox.Show("Text input completed!");

                    MessageBox.Show($"TextBox in row {textBox.Tag.GetType()} ");
                    
                    /*
                    if (dxfCoords != null)
                    {
                        int index = VertexesListView.Items.IndexOf(dxfCoords);
                        // index를 사용하여 몇 번째 항목인지 확인 가능
                        MessageBox.Show($"TextBox in row {index} ");
                    }
                    //propertyEntity.dxfCoords[1]*/
                }
                else
                {
                    MessageBox.Show("WRONG VALUE!!");
                }

       
                
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
    }
}
