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
    public partial class MultipleSpotWindow : Window
    {
        protected class Node
        {
            protected string name;
            protected List<string> nearbyNodes = new List<string>();
            protected List<NodeConnection> connections = new List<NodeConnection>();
            protected bool visited = false;

            public void setName(string n)
            {
                this.name = n;
            }

            public string getName()
            {
                return this.name;
            }
        }

        protected class NodeConnection
        {
            protected Node[] connection = new Node[2];
        }

        private List<int> distanceList = new List<int>();
        private List<Node> nodeList = new List<Node>();
        private List<int> surroundList = new List<int>();
        private List<Node> pathList = new List<Node>();
        private List<int> tempDistancesList = new List<int>();

        private int currentDistance = 0;
        private Node currentNode;
        private Node needToVisit;
        private int numOfNodesNoHome = 0;

        private Dictionary<int, Node> nodeDictionary = new Dictionary<int, Node>();
        

        public MultipleSpotWindow()
        {
            InitializeComponent();

            distanceList.Add(0);
            Node node0 = new Node();
            node0.setName("Home");
            nodeList.Add(node0);
            pathList.Add(node0);
            nodeDictionary.Add(0, node0);

            currentNode = node0;

        }

        private void addNode(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                var currentPos = Mouse.GetPosition(Application.Current.MainWindow);
                numOfNodesNoHome++;

                Button newButton = new Button();
                newButton.Name = "node" + numOfNodesNoHome;
                newButton.Content = "Node " + numOfNodesNoHome;
                newButton.Width = 60;
                newButton.Height = 35;
                newButton.Margin = new Thickness(currentPos.X-300, currentPos.Y-300, 0, 0);
                mainGrid.Children.Add(newButton);

                Node newNode = new Node();
                newNode.setName("Node "+ numOfNodesNoHome);
                nodeList.Add(newNode);
                nodeDictionary.Add(numOfNodesNoHome, newNode);

                TextBox newTextBox = new TextBox();
                newTextBox.Width = 85;
                newTextBox.Height = 20;
                newTextBox.Margin = new Thickness(147, 44, 580, 347 - (numOfNodesNoHome * 35));
                newTextBox.Name = "node" + numOfNodesNoHome + "InputBox";
                newTextBox.Text = "Node " + numOfNodesNoHome;
                mainGrid.Children.Add(newTextBox);
                
            }
        }
    }
}
