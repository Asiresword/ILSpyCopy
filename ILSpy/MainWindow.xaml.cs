using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ILSpy
{
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

        private TreeViewItem LoadAssemblyDependencies(Assembly InnerAssembly, SortedSet<string> PreviousCallStack)
        {
            if(PreviousCallStack.Contains(InnerAssembly.GetName().Name))
            {
                return null;
            }
            PreviousCallStack.Add(InnerAssembly.GetName().Name);

            TreeViewItem Item = new TreeViewItem() { Header = InnerAssembly.GetName().Name };
            AssemblyName[] Names = InnerAssembly.GetReferencedAssemblies();
            foreach(AssemblyName Name in Names)
            {
                TreeViewItem InnerItem = LoadAssemblyDependencies(Assembly.Load(Name), PreviousCallStack);
                if (InnerItem != null)
                {
                    Item.Items.Add(InnerItem);
                }
            }
            return Item;
        }

        private TreeViewItem ConfigureTreeViewItemType(Type type, Assembly FileAssembly)
        {
            TreeViewItem Item = new TreeViewItem() { Header = type.Name };
            TreeViewItem BasedOn = new TreeViewItem() { Header = "BasedOn" };
            TreeViewItem DerivedFrom = new TreeViewItem() { Header = "Derived classes" };
            TreeViewItem Methods = new TreeViewItem() { Header = "Methods" };
            TreeViewItem Fields = new TreeViewItem() { Header = "Fields" };
            TreeViewItem Properties = new TreeViewItem() { Header = "Properties" };

            if (type.BaseType != null)
            {
                BasedOn.Items.Add(new TreeViewItem() { Header = type.BaseType.FullName });
            }

            Type[] DerivedClasses = (from assemblyType in FileAssembly.GetTypes()
                                     where assemblyType.IsSubclassOf(type)
                                     select assemblyType).ToArray();

            foreach(Type t in DerivedClasses)
            {
                DerivedFrom.Items.Add(new TreeViewItem() { Header = t.FullName });
            }

            foreach (MethodInfo Info in type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            { 
                string Method = Info.IsPrivate ? "-" : "+";
                Method = Info.IsFamilyOrAssembly ? "#" : "+";
                Method += " " + Info.Name;
                Method += "(";
                foreach(ParameterInfo PInfo in Info.GetParameters())
                {
                    Method += PInfo.ParameterType + " " + PInfo.Name + ", ";
                }
                Method += ")";


                Methods.Items.Add(new TreeViewItem() { Header = Method });
            }

            foreach (FieldInfo Info in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                string Field = Info.IsPrivate ? "-" : "+";
                Field = Info.IsFamilyOrAssembly ? "#" : "+";
                Field += " " + Info.Name;

                Fields.Items.Add(new TreeViewItem() { Header = Field });
            }

            foreach (PropertyInfo Info in type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                Properties.Items.Add(new TreeViewItem() { Header = Info.Name });
            }

            Item.Items.Add(BasedOn);
            Item.Items.Add(DerivedFrom);
            Item.Items.Add(Methods);
            Item.Items.Add(Fields);
            Item.Items.Add(Properties);

            return Item;
        }

        private Dictionary<string, List<Type>> GetNamespacesTypes(Assembly FileAssembly)
        {
            Dictionary<string, List<Type>> NamespacesTypes = new Dictionary<string, List<Type>>();

            Type[] Types = FileAssembly.GetTypes();
            foreach(Type type in Types)
            {
                if (!string.IsNullOrEmpty(type.Namespace))
                {
                    if (!NamespacesTypes.ContainsKey(type.Namespace))
                    {
                        NamespacesTypes.Add(type.Namespace, new List<Type>() { type });
                    }
                    else
                    {
                        NamespacesTypes[type.Namespace].Add(type);
                    }
                }
            }

            return NamespacesTypes;
        }

        private void LoadReflection()
        {
            try
            {
                Assembly FileAssembly = Assembly.LoadFrom(this.FileName);

                TreeViewItem MainItem = new TreeViewItem() { Header = FileAssembly.GetName().Name };
                TreeViewItem References = new TreeViewItem() { Header = "References" };
                Dictionary<string, List<Type>> NamespacesTypes = GetNamespacesTypes(FileAssembly);

                foreach (string Namespace in NamespacesTypes.Keys)
                {
                    TreeViewItem NamespaceItem = new TreeViewItem() { Header = Namespace };
                    foreach (Type type in NamespacesTypes[Namespace])
                    {
                        TreeViewItem TypeItem = ConfigureTreeViewItemType(type, FileAssembly);
                        NamespaceItem.Items.Add(TypeItem);
                    }
                    MainItem.Items.Add(NamespaceItem);
                }

                foreach(AssemblyName Name in FileAssembly.GetReferencedAssemblies())
                {
                    try
                    {
                        References.Items.Add(LoadAssemblyDependencies(Assembly.Load(Name), new SortedSet<string>()));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Unable to load dependecies: " + ex.Message);
                    }
                }

                MainItem.Items.Add(References);
                this.AssemblyInfoTree.Items.Add(MainItem);
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
