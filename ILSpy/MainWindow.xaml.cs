using System;
using System.Reflection;
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

namespace ILSpy
{
    internal enum TreeViewItemType
    {
        Assembly,
        References,
        Reference
    }

    public partial class MainWindow : Window
    {
        private string FileName { get; set; }
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ExitMenu_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

        private void OpenMenu_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog Dialog = new Microsoft.Win32.OpenFileDialog();
            Dialog.DefaultExt = ".exe";
            Dialog.Filter = "Execution Files (*.exe)|*.exe|Dynamic Link Libraries (*.dll)|*.dll";

            Nullable<bool> result = Dialog.ShowDialog();

            if (result == true)
            {
                this.FileName = Dialog.FileName;
                LoadReflection();
            }
        }

        private TreeViewItem ConfigureTreeViewItemSource(string Name, TreeViewItemType Type)
        {
            TreeViewItem Item = new TreeViewItem();
            StackPanel Panel = new StackPanel();
            Image Image = new Image();
            TextBlock Block = new TextBlock();

            if(Type == TreeViewItemType.Assembly)
            {
                Image.Source = new BitmapImage(new Uri("../../Images/CSAssemblyInfoFile_16x.png", UriKind.Relative));
            }
            else
            {
                Image.Source = new BitmapImage(new Uri("../../Images/Reference_16x.png", UriKind.Relative));
            }
            Image.Margin = new Thickness(5, 0, 5, 0);
            Block.Text = Name;

            Panel.Children.Add(Image);
            Panel.Children.Add(Block);

            Item.Header = Panel;

            return Item;
        }

        private void LoadReflection()
        {
            try
            {
                /*Assembly FileAssembly = Assembly.LoadFrom(this.FileName);
                TreeViewItem MainItem = ConfigureTreeViewItemSource(FileAssembly.GetName().Name, TreeViewItemType.Assembly);
                TreeViewItem References = ConfigureTreeViewItemSource("References", TreeViewItemType.References);

                MainItem.Items.Add(References);

                foreach(AssemblyName Name in FileAssembly.GetReferencedAssemblies())
                {
                    //Assembly InnerAssembly = Assembly.Load(Name);
                    References.Items.Add(ConfigureTreeViewItemSource(Name.Name, TreeViewItemType.Reference));
                }

                this.AssemblyInfoTree.Items.Add(MainItem);*/
            }
            catch(System.IO.IOException ex)
            {
                MessageBox.Show($"An IO error occurred during opening this file: {ex.Message}");
                return;
            }
            catch(Exception ex)
            {
                MessageBox.Show($"An unkown error occurred during opening this file: {ex.Message}");
                return;
            }
        }
    }
}
