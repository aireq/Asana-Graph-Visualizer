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
using System.IO;
using System.Xml;

namespace AsanaGraphVisualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        WorkSpace CurrentWorkspace { get; set; }

        private void readDataButton_Click(object sender, RoutedEventArgs e)
        {

            var workspaceId = System.Convert.ToInt64(workSpaceIdTextBox.Text);
            var apiKeys = apiKeysTextBox.Text.Split(new String[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            WorkSpace workSpace = new WorkSpace(workspaceId, apiKeys);
            workSpace.GenerateGraph();

            CurrentWorkspace = workSpace;

            this.teamCountLabel.Content = workSpace.Teams.Count;
            this.usersCountLabel.Content = workSpace.Users.Count;
            this.projCountLabel.Content = workSpace.Projects.Count;
            this.tasksCountLabel.Content = workSpace.Tasks.Count;

            exportButton.IsEnabled = true;
            exportTypeCombo.IsEnabled = true;
        }

        private void exportButton_Click(object sender, RoutedEventArgs e)
        {


            switch (exportTypeCombo.SelectedIndex)
            {
                case 0:
                    ExportToYedGraphMl(CurrentWorkspace);
                    break;

                case 1:
                    ExportToExcel(CurrentWorkspace);
                    break;

                case 2:
                    ExportToGephiGraphMl(CurrentWorkspace);
                    break;
            }



        }

        private void ExportToGephiGraphMl(WorkSpace CurrentWorkspace)
        {
            throw new NotImplementedException();
        }

        private void ExportToYedGraphMl(WorkSpace CurrentWorkspace)
        {
            Microsoft.Win32.SaveFileDialog sDiag = new Microsoft.Win32.SaveFileDialog();
            sDiag.Filter = "Yed (*.graphml)|*.graphml";

            var res = sDiag.ShowDialog();

            if (!(res.HasValue & res.Value)) return;


            string filePath = sDiag.FileName;

            if (File.Exists(filePath)) File.Delete(filePath);


            using (var xmlWriter = XmlWriter.Create(filePath))
            {
                xmlWriter.WriteStartDocument();

                //var yEdNameSpace = "xmlns="http://graphml.graphdrawing.org/xmlns" xmlns:xsi="" xmlns:y="http://www.yworks.com/xml/graphml" xmlns:yed="http://www.yworks.com/xml/yed/3" xsi:schemaLocation="http://graphml.graphdrawing.org/xmlns http://www.yworks.com/xml/schema/graphml/1.1/ygraphml.xsd"

                xmlWriter.WriteStartElement("graphml", "http://graphml.graphdrawing.org/xmlns");
                xmlWriter.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
                xmlWriter.WriteAttributeString("xmlns", "y", null, "http://www.yworks.com/xml/graphml");
                xmlWriter.WriteAttributeString("xmlns", "yed", null, "http://www.yworks.com/xml/yed/3");
                xmlWriter.WriteAttributeString("xsi", "schemaLocation", null, "http://graphml.graphdrawing.org/xmlns http://www.yworks.com/xml/schema/graphml/1.1/ygraphml.xsd");


                //Write Keys
                WriteKeyElements(xmlWriter);


                foreach (var team in this.CurrentWorkspace.Teams.Values.ToList())
                {
                    long node_Id = team.Id;
                    string node_Name = team.Name;
                    string node_shape = "hexagon";

                    WriteNode(xmlWriter, node_Id, node_Name, node_shape);
                }


                foreach (var proj in this.CurrentWorkspace.Projects.Values.ToList())
                {
                    long node_Id = proj.Id;
                    string node_Name = proj.Name;
                    string node_shape = "rectangle";

                    WriteNode(xmlWriter, node_Id, node_Name, node_shape);
                }


                foreach (var task in this.CurrentWorkspace.Tasks.Values.ToList())
                {
                    if (task.Completed) continue;

                    long node_Id = task.Id;
                    string node_Name = task.Name;
                    string node_shape = "roundrectangle";

                    WriteNode(xmlWriter, node_Id, node_Name, node_shape);
                }



                foreach (var user in this.CurrentWorkspace.Users.Values.ToList())
                {
                    long node_Id = user.Id;
                    string node_Name = user.Name;
                    string node_shape = "circle";

                    WriteNode(xmlWriter, node_Id, node_Name, node_shape);
                }


                int team_proj_edgeidx = 0;
                int team_user_edgeidx = 0;

                foreach (var team in this.CurrentWorkspace.Teams.Values)
                {
                    foreach (var project in team.Projects.Values)
                    {
                        WriteEdge(xmlWriter, "team_proj_" + team_proj_edgeidx.ToString(), team.Id, project.Id, "line", 2);
                        team_proj_edgeidx++;
                    }


                    foreach (var user in team.Users.Values)
                    {
                        WriteEdge(xmlWriter, "team_user_" + team_user_edgeidx.ToString(), team.Id, user.Id, "line", 2);
                        team_user_edgeidx++;
                    }
                }



                int proj_task_edgeidx = 0;

                int task_assignee_edgeidx = 0;
                int task_follower_edgeidx = 0;


                foreach (var proj in this.CurrentWorkspace.Projects.Values)
                {

                    foreach (var task in proj.Tasks.Values)
                    {
                        if (task.Completed) continue;

                        WriteEdge(xmlWriter, "proj_task_" + proj_task_edgeidx.ToString(), proj.Id, task.Id, "line", 2);
                        proj_task_edgeidx++;

                        if (task.Assignee != null)
                        {
                            WriteEdge(xmlWriter, "task_assignee" + task_assignee_edgeidx.ToString(), task.Assignee.Id, task.Id, "line", 2);
                            task_assignee_edgeidx++;
                        }


                        
                        //foreach (var follower in task.Followers.Values)
                        //{

                        //    WriteEdge(xmlWriter, "task_follower" + task_follower_edgeidx.ToString(), follower.Id, task.Id, "dotted", 2);
                        //    task_follower_edgeidx++;
                        //}
                    }

                }





                //xmlWriter.WriteEndElement();//graphml
                xmlWriter.WriteEndDocument();


            }





        }

        private static void WriteEdge(XmlWriter xmlWriter, string edge_Id, long source_id, long target_id, string lineType,double width)
        {

            xmlWriter.WriteStartElement("edge");
            xmlWriter.WriteAttributeString("id", edge_Id);
            xmlWriter.WriteAttributeString("source", source_id.ToString());
            xmlWriter.WriteAttributeString("target", target_id.ToString());


            xmlWriter.WriteStartElement("data");
            xmlWriter.WriteAttributeString("key", "d_edgeDesc");
            xmlWriter.WriteEndElement();


            xmlWriter.WriteStartElement("data");
            xmlWriter.WriteAttributeString("key", "d_edgeGraphics");





                xmlWriter.WriteStartElement("y", "PolyLineEdge",null);

                    xmlWriter.WriteStartElement("y", "Path", null);
                    xmlWriter.WriteAttributeString("sx", "0.0");
                    xmlWriter.WriteAttributeString("sy", "0.0");
                    xmlWriter.WriteAttributeString("tx", "0.0");
                    xmlWriter.WriteAttributeString("ty", "0.0");
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("y", "LineStyle", null);
                    xmlWriter.WriteAttributeString("color", "#000000");
                    xmlWriter.WriteAttributeString("type", lineType);
                    xmlWriter.WriteAttributeString("width", width.ToString());
                    xmlWriter.WriteEndElement();


                    xmlWriter.WriteStartElement("y", "Arrows", null);
                    xmlWriter.WriteAttributeString("source", "none");
                    xmlWriter.WriteAttributeString("target", "standard");
                    xmlWriter.WriteEndElement();


                    xmlWriter.WriteStartElement("y", "BendStyle", null);
                    xmlWriter.WriteAttributeString("smoothed", "false");
                    xmlWriter.WriteEndElement();

                xmlWriter.WriteEndElement();//PolyLineEdge

            xmlWriter.WriteEndElement();//data
            
            

            






            
            xmlWriter.WriteEndElement();//node
        
        }



        private static void WriteNode(XmlWriter xmlWriter, long node_Id, string node_Name, string node_shape)
        {
            xmlWriter.WriteStartElement("node");
            xmlWriter.WriteAttributeString("id", node_Id.ToString());


            xmlWriter.WriteStartElement("data");
            xmlWriter.WriteAttributeString("key", "d_nodeDesc");
            xmlWriter.WriteEndElement();


            xmlWriter.WriteStartElement("data");
            xmlWriter.WriteAttributeString("key", "d_nodeUrl");
            xmlWriter.WriteEndElement();


            xmlWriter.WriteStartElement("data");
            xmlWriter.WriteAttributeString("key", "d_nodeGraphics");


            xmlWriter.WriteStartElement("y", "ShapeNode", null);


            xmlWriter.WriteElementString("y", "NodeLabel", null, node_Name);


            xmlWriter.WriteStartElement("y", "Shape", null);
            xmlWriter.WriteAttributeString("type", node_shape);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();//data
            xmlWriter.WriteEndElement();//node
        }

        private static void WriteKeyElements(XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("key");
            xmlWriter.WriteAttributeString("for", "graphml");
            xmlWriter.WriteAttributeString("id", "d_resources");
            xmlWriter.WriteAttributeString("yfiles.type", "resources");
            xmlWriter.WriteEndElement();


            xmlWriter.WriteStartElement("key");
            xmlWriter.WriteAttributeString("for", "port");
            xmlWriter.WriteAttributeString("id", "d_portGraphics");
            xmlWriter.WriteAttributeString("yfiles.type", "portgraphics");
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("key");
            xmlWriter.WriteAttributeString("for", "port");
            xmlWriter.WriteAttributeString("id", "d_portGeometry");
            xmlWriter.WriteAttributeString("yfiles.type", "portgeometry");
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("key");
            xmlWriter.WriteAttributeString("for", "port");
            xmlWriter.WriteAttributeString("id", "d_portData");
            xmlWriter.WriteAttributeString("yfiles.type", "portuserdata");
            xmlWriter.WriteEndElement();


            xmlWriter.WriteStartElement("key");
            xmlWriter.WriteAttributeString("for", "node");
            xmlWriter.WriteAttributeString("id", "d_nodeUrl");
            xmlWriter.WriteAttributeString("attr.name", "url");
            xmlWriter.WriteAttributeString("attr.type", "string");
            xmlWriter.WriteEndElement();


            xmlWriter.WriteStartElement("key");
            xmlWriter.WriteAttributeString("for", "edge");
            xmlWriter.WriteAttributeString("id", "d_edgeUrl");
            xmlWriter.WriteAttributeString("attr.name", "url");
            xmlWriter.WriteAttributeString("attr.type", "string");
            xmlWriter.WriteEndElement();


            xmlWriter.WriteStartElement("key");
            xmlWriter.WriteAttributeString("for", "edge");
            xmlWriter.WriteAttributeString("id", "d_edgeDesc");
            xmlWriter.WriteAttributeString("attr.name", "description");
            xmlWriter.WriteAttributeString("attr.type", "string");
            xmlWriter.WriteEndElement();



            xmlWriter.WriteStartElement("key");
            xmlWriter.WriteAttributeString("for", "node");
            xmlWriter.WriteAttributeString("id", "d_nodeDesc");
            xmlWriter.WriteAttributeString("attr.name", "description");
            xmlWriter.WriteAttributeString("attr.type", "string");
            xmlWriter.WriteEndElement();


            xmlWriter.WriteStartElement("key");
            xmlWriter.WriteAttributeString("for", "graph");
            xmlWriter.WriteAttributeString("id", "d_graphDesc");
            xmlWriter.WriteAttributeString("attr.name", "Description");
            xmlWriter.WriteAttributeString("attr.type", "string");
            xmlWriter.WriteEndElement();


            xmlWriter.WriteStartElement("key");
            xmlWriter.WriteAttributeString("for", "node");
            xmlWriter.WriteAttributeString("id", "d_nodeGraphics");
            xmlWriter.WriteAttributeString("yfiles.type", "nodegraphics");
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("key");
            xmlWriter.WriteAttributeString("for", "edge");
            xmlWriter.WriteAttributeString("id", "d_edgeGraphics");
            xmlWriter.WriteAttributeString("yfiles.type", "edgegraphics");
            xmlWriter.WriteEndElement();
        }

        private void ExportToExcel(WorkSpace CurrentWorkspace)
        {
            throw new NotImplementedException();
        }
    }
}

