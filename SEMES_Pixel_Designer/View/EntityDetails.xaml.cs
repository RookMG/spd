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
using static SEMES_Pixel_Designer.Utils.PolygonEntity;

namespace SEMES_Pixel_Designer
{
    /// <summary>
    /// EntityDetails.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class EntityDetails : Page
    {
        Dictionary<string, PolygonEntity> entityDictionary;

        public EntityDetails()
        {
            InitializeComponent();

            entityDictionary = new Dictionary<string, PolygonEntity>();


            Utils.Mediator.Register("EntityDetails.ShowEntityTypes", ShowEntityTypes);
            Utils.Mediator.Register("EntityDetails.ShowEntityComboBox", ShowEntityComboBox);
            Utils.Mediator.Register("EntityDetails.ShowEntityPropertyDetail", ShowEntityPropertyDetail);

        }


        public void ShowEntityTypes(object obj)
        {
            TreeViewItem entities = new TreeViewItem();
            
                        
            foreach(PolygonEntity entity in Coordinates.CanvasRef.DrawingEntities)
            {
                CheckBox checkBox = new CheckBox {};

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

            if(entities.Items.Count != 0)
                entities.Header = "Entities";

            EntityTreeView.Items.Add(entities);

        }

        public void ShowEntityComboBox(object obj)
        {
            EntityDetailComboBox.Items.Clear();

            foreach (PolygonEntity entity in Coordinates.CanvasRef.DrawingEntities)
            {
                if (entity.Selected == false) continue;


                ComboBoxItem item = new ComboBoxItem();
                item.Content = PolygonTypeToString(entity);

                entityDictionary.Add(PolygonTypeToString(entity), entity);

                EntityDetailComboBox.Items.Add(item);
            }

        }

        private void ShowEntityProperties(object obj, SelectionChangedEventArgs e)
        {
            PropertyStackPanel.Children.Clear();

            if (EntityDetailComboBox.SelectedItem != null)
            {
                string selectedItem = ((ComboBoxItem)EntityDetailComboBox.SelectedItem).Content.ToString();

                TextBlock textBlock = new TextBlock();
                textBlock.Text = "Name";
                
                //PropertyStackPanel.Children.Add();
                //textBlock.Text = "Color";
                PropertyStackPanel.Children.Add(textBlock);
                //entityDictionary[selectedItem];
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
