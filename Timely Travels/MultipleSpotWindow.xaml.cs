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
using System.Windows.Shapes;
using System.Drawing;

namespace Timely_Travels
{
    /// <summary>
    /// Interaction logic for MultipleSpotWindow.xaml
    /// </summary>
    /// 
    public static class Extensions
    {
        public static K FindFirstKeyByValue<K, V>(this Dictionary<K, V> dict, V val)
        {
            return dict.FirstOrDefault(entry =>
                EqualityComparer<V>.Default.Equals(entry.Value, val)).Key;
        }
    }
    public partial class MultipleSpotWindow : Window
    {
        protected class Node
        {
            private string name;
            private List<string> nearbyNodes = new List<string>();                                            //most likely get this from connections when it is current node
            private List<NodeConnection> connections = new List<NodeConnection>();
            private bool visited = false;                                                                     

            public void setName(string n)
            {
                this.name = n;
            }

            public string getName()
            {
                return this.name;
            }

            public void addConnection(NodeConnection c)
            {
                connections.Add(c);
            }

            public List<NodeConnection> getConnections()
            {
                return connections;
            }

            public void removeConnection(Node nodeConnectedToThis)
            {
                for (int i = 0; i < connections.Count; i++)
                {
                    if (connections[i].getNodes()[0].getName() == nodeConnectedToThis.getName()
                        || connections[i].getNodes()[1].getName() == nodeConnectedToThis.getName())
                    {
                        connections.RemoveAt(i);
                    }
                }
            }

        }

        protected class NodeConnection
        {
            private Node[] connection = new Node[2];
            
            public NodeConnection(Node firstNode, Node secondNode)
            {
                connection[0] = firstNode;
                connection[1] = secondNode;
            }

            public Node[] getNodes()
            {
                return connection;
            }
        }

        private List<int> distanceList = new List<int>();                                      //list of distances along the path (created with the algorithm, all start at 0)
        private List<Node> nodeList = new List<Node>();                                        //node list of list of nodes in dictionary (to be used at computation time and for searching)
        private List<int> surroundList = new List<int>();                                      //a copy of nearby nodes
        private List<Node> pathList = new List<Node>();                                        
        private List<int> tempDistancesList = new List<int>();                                 //used when comparing potential paths to take

        private int currentDistance = 0;        
        private Node currentNode;
        private Node needToVisit;                                                                   
        private int numOfNodesNoHome = 0;                                                       //used as a general index for nodes (GENERAL)

        private Dictionary<int, Node> nodeDictionary = new Dictionary<int, Node>();             //links nodes with their index 
        private Dictionary<int, string> nodeNameDictionary = new Dictionary<int, string>();     //links node names with their index 
        private List<NodeConnection> connectionList = new List<NodeConnection>();               //a list to keep general track of all connections (only used for finding most recent/first/2nd/etc connections)

        private List<Button> nodeButtonArray = new List<Button>();                              //UI control lists
        private List<TextBox> nodeNameTextBoxArray = new List<TextBox>();
        private Button[] selectedButtons = new Button[2];
        private List<Line> connectionLines = new List<Line>();
        private List<TextBox> connectionDistanceTextBoxes = new List<TextBox>();


        private int connectionIndex = 0;    

        private Point firstNodeLocation;                                                        //location on the UI
        private Point secondNodeLocation;

        private bool connectionAlreadyExists = false;


        public MultipleSpotWindow()
        {
            InitializeComponent();

            distanceList.Add(0);
            Node firstNode = new Node();
            firstNode.setName("Home");;
            pathList.Add(firstNode);
            nodeDictionary.Add(0, firstNode);
            nodeNameDictionary.Add(0, "Home");
            nodeButtonArray.Add(node0);
            nodeList.Add(firstNode);

            currentNode = firstNode;

            selectedButtons[0] = null;                                          //make them null so comparing is easier; shows that they're both empty at initiation
            selectedButtons[1] = null;

        }

        private void nodeManage(object sender, KeyEventArgs e)
        {
            var currentPos = Mouse.GetPosition(Application.Current.MainWindow);

            if (e.Key == Key.Space)
            {
                numOfNodesNoHome++;

                Button newButton = new Button();
                newButton.Name = "node" + numOfNodesNoHome;
                newButton.Content = "Node " + numOfNodesNoHome;
                newButton.Width = 80;
                newButton.Height = 35;
                newButton.Margin = new Thickness(currentPos.X - 10, currentPos.Y - 10, 0, 0);
                newButton.Click += connectionHandle;
                mainCanvas.Children.Add(newButton);
                nodeButtonArray.Add(newButton);

                newButton = null;                                           //make null for garbage collection

                Node newNode = new Node();
                newNode.setName("Node " + numOfNodesNoHome);
                distanceList.Add(0);
                nodeDictionary.Add(numOfNodesNoHome, newNode);
                nodeNameDictionary.Add(numOfNodesNoHome, "Node " + numOfNodesNoHome);
                nodeList.Add(newNode);

                newNode = null;

                TextBox newTextBox = new TextBox();
                newTextBox.Width = 85;
                newTextBox.Height = 20;
                newTextBox.Margin = new Thickness(10, 49, 717, 347 - (numOfNodesNoHome * 55));
                newTextBox.Name = "node" + numOfNodesNoHome + "InputBox";
                newTextBox.Text = "Node " + numOfNodesNoHome;
                mainGrid.Children.Add(newTextBox);
                nodeNameTextBoxArray.Add(newTextBox);

                newTextBox = null;

                GC.Collect();
                GC.WaitForPendingFinalizers();

            }
            else if (e.Key == Key.X)
            {
                if (numOfNodesNoHome > 0)
                {
                    mainCanvas.Children.Remove(nodeButtonArray[numOfNodesNoHome]);                  //Remove from UI and UI lists             
                    mainGrid.Children.Remove(nodeNameTextBoxArray[numOfNodesNoHome-1]);
                    nodeButtonArray.RemoveAt(numOfNodesNoHome);
                    nodeNameTextBoxArray.RemoveAt(numOfNodesNoHome-1);

                    nodeList.RemoveAt(numOfNodesNoHome);                                            //remove from general node list
                    distanceList.RemoveAt(numOfNodesNoHome);                                        //remove from distance list so it's not calculated
                    

                    for (int i = 0; i < nodeList.Count; i++)                                        //remove any other connections with that node
                                                                                                    //(reference in node dict and numOfNodes kept for refernce in connection delete)
                    {                                                                               //i = for each node (remember that deleted node is not included in this search)
                       
                        if (nodeList[i].getConnections().Count != 0)                                //make sure we're not going through empty connections to prevent out of range
                        {
                            for (int j = 0; j < nodeList[i].getConnections().Count; j++)                 //j = for each connection in node
                            {
                                if (nodeList[i].getConnections()[j].getNodes()[0].getName() == nodeDictionary[numOfNodesNoHome].getName()   //if the deleted node is found in a connection
                                    || nodeList[i].getConnections()[j].getNodes()[1].getName() == nodeDictionary[numOfNodesNoHome].getName())
                                {
                                    nodeList[i].removeConnection(nodeDictionary[numOfNodesNoHome]);                                         //get out of the node list

                                    for (int k = 0; k < connectionLines.Count; k++)                                                         //loop to find which lines to delete in general list and UI (not the most recent)
                                    {
                                        if (connectionList[k].getNodes()[0].getName() == nodeDictionary[numOfNodesNoHome].getName()         //if any connection contains the deleted node
                                            || connectionList[k].getNodes()[1].getName() == nodeDictionary[numOfNodesNoHome].getName())
                                        {
                                            mainCanvas.Children.Remove(connectionLines[k]);                                                 //get rid of it
                                            connectionLines.Remove(connectionLines[k]);
                                            break;

                                        }
                                    }
                                   
                                    connectionIndex--;
                                    break;

                                }
                            }
                        }
                    }

                    nodeDictionary.Remove(numOfNodesNoHome);        //since we're done with the reference, delete it entirely now
                    nodeNameDictionary.Remove(numOfNodesNoHome);

                    numOfNodesNoHome--;
                }
                else
                {
                    MessageBox.Show("Cannot delete your home off the trip.", "Deletion Error");
                }

            }
            else if (e.Key == Key.Z)
            {
                if (connectionIndex > 0)
                { 

                    Node firstNodeToRemove = connectionList[connectionIndex-1].getNodes()[0];
                    Node secondNodeToRemove = connectionList[connectionIndex-1].getNodes()[1];

                    mainCanvas.Children.Remove(connectionLines[connectionIndex-1]);
                    connectionLines.Remove(connectionLines[connectionIndex-1]);
                    firstNodeToRemove.removeConnection(secondNodeToRemove);
                    secondNodeToRemove.removeConnection(firstNodeToRemove);

                    connectionIndex--;
                }
            }
        }

        private void changeNames(object sender, RoutedEventArgs e)
        {
            if (numOfNodesNoHome == 0)                                                              
            {
                nodeDictionary[0].setName(homeNameInputBox.Text);
                node0.Content = homeNameInputBox.Text;
            }
            else if (numOfNodesNoHome > 0)                                                          //set all of their names
            {
                for (int i = 1; i <= numOfNodesNoHome; i++)
                {
                    nodeDictionary[0].setName(homeNameInputBox.Text);
                    node0.Content = homeNameInputBox.Text;
                    nodeDictionary[i].setName(nodeNameTextBoxArray[i-1].Text);
                    nodeNameDictionary.Remove(i);                                                   //delete and re-add into dictionaries
                    nodeNameDictionary.Add(i, nodeNameTextBoxArray[i-1].Text);
                    nodeButtonArray[i].Content = nodeNameTextBoxArray[i-1].Text;
                }
            }
        }

        private void connectionHandle(object sender, RoutedEventArgs e)
        {
            var currentPos = Mouse.GetPosition(Application.Current.MainWindow);

            if (selectedButtons[0] == null)                 //represents adding the first button to be compared 
            {
                firstNodeLocation = currentPos;
                selectedButtons[0] = (Button)sender;        //sender = currently clicked button
                Console.WriteLine("selected Button 0 + " + selectedButtons[0].Name);
            }
            else
            {
                selectedButtons[1] = (Button)sender;
                secondNodeLocation = currentPos;

                if (selectedButtons[1] == selectedButtons[0])           //if same button is clicked twice
                {
                    MessageBox.Show("The target location on the route cannot be the same as the original location.", "Location Error");

                }
                else if (selectedButtons[1] != selectedButtons[0])      //if different buttons are clicked
                {
                    int selectedIndex0, selectedIndex1;
                    selectedIndex0 = nodeNameDictionary.FindFirstKeyByValue(selectedButtons[0].Content.ToString());     //find index of the node from it's name on the button
                    selectedIndex1 = nodeNameDictionary.FindFirstKeyByValue(selectedButtons[1].Content.ToString());
                    Node firstNode = nodeDictionary[selectedIndex0];                        //get actual node from node dictionary with the index
                    Node secondNode = nodeDictionary[selectedIndex1];

                    NodeConnection newConnection = new NodeConnection(firstNode, secondNode);

                    if (firstNode.getConnections().Count != 0 || secondNode.getConnections().Count != 0)            //if one of the nodes has connections
                    {

                        for (int i = 0; i < firstNode.getConnections().Count; i++)                                  //check every connection in the first node
                        {
                            if (firstNode.getConnections()[i].getNodes()[0].getName() == secondNode.getName()
                                || firstNode.getConnections()[i].getNodes()[1].getName() == secondNode.getName())
                            //statement is finding out if any sort of connection between node 1 and 2 has already been recorded
                            {
                                newConnection = null;
                                MessageBox.Show("These locations are already connected to each other.", "Location Error");
                                connectionAlreadyExists = true;
                            }

                        }
                        

                    }

                    if (!connectionAlreadyExists)
                           //no record of the connection exists between the node
                    {
                        firstNode.addConnection(newConnection);
                        secondNode.addConnection(newConnection);

                        nodeDictionary.Remove(selectedIndex0);                                  //remove and re-add updated nodes
                        nodeDictionary.Add(selectedIndex0, firstNode);
                        nodeDictionary.Remove(selectedIndex1);
                        nodeDictionary.Add(selectedIndex1, secondNode);

                        connectionList.Add(newConnection);                                      //add it to the general/searching list

                        Line newConnectionLine = new Line();                                    //make a new UI line
                        newConnectionLine.X1 = firstNodeLocation.X;
                        newConnectionLine.Y1 = firstNodeLocation.Y;
                        newConnectionLine.X2 = secondNodeLocation.X;
                        newConnectionLine.Y2 = secondNodeLocation.Y - 10;
                        newConnectionLine.Stroke = new SolidColorBrush(Colors.DarkSlateGray);
                        newConnectionLine.StrokeThickness = 7;
                        newConnectionLine.Name = "connection" + connectionIndex;
                        mainCanvas.Children.Add(newConnectionLine);
                        connectionLines.Add(newConnectionLine);

                       // TextBox newDistanceBox = new TextBox();
                        //newDistanceBox.Margin = new Thickness(secondNodeLocation.X-firstNodeLocation.X, 0, secondNodeLocation.Y - firstNodeLocation.Y, 0);
                        mainCanvas.Children.Add(newDistanceBox);

                        connectionIndex++;


                    }

                    connectionAlreadyExists = false;
                    firstNode = null;
                    secondNode = null;

                }

                GC.Collect();
                GC.WaitForPendingFinalizers();

                selectedButtons[0] = null;              //clear them
                selectedButtons[1] = null;

                firstNodeLocation = new Point(0, 0);
                secondNodeLocation = new Point(0, 0);

            }
        }
    }
}
