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
        public EntityDetails()
        {
            InitializeComponent();

            Utils.Mediator.Register("EntityDetails.ShowEntityTypes", ShowEntityTypes);
            Utils.Mediator.Register("EntityDetails.ShowEntityProperties", ShowEntityProperties);
            Utils.Mediator.Register("EntityDetails.ShowEntityPropertyDetail", ShowEntityPropertyDetail);
        }


        public void ShowEntityTypes(object obj)
        {
            TreeViewItem entities = new TreeViewItem();
            entities.Header = "Entities";
                        
            foreach(PolygonEntity val in Coordinates.CanvasRef.DrawingEntities)
            {

            }

            foreach (var entity in MainWindow.doc.Entities.All)
            {
                DockPanel dockPanel = new DockPanel();
                CheckBox checkBox = new CheckBox();
                TextBlock textBlock = new TextBlock();

                textBlock.Text = entity.GetType().Name;

                dockPanel.Children.Add(checkBox);
                dockPanel.Children.Add(textBlock);


                entities.Items.Add(dockPanel);

            }


            entity_tree_view.Items.Clear();

            entity_tree_view.Items.Add(entities);

        }

        public void ShowEntityProperties(object obj)
        {
        }

        public void ShowEntityPropertyDetail(object obj)
        {
        }
    }
}
