using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
using System.Xml;
using Html2Pdf.Core; 
using Html2Pdf.Wpf.Models;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using Path = System.IO.Path;

namespace Html2Pdf.Wpf
{
    public partial class MainWindow : MetroWindow
    {
        public Hashtable Profiles { get; set; }
        public List<TreeModel> TreeModelList { get; set; }
        public List<TreeModel> TreeModelChecked { get; set; }

        public string PdfBaseFolder { get; set; }
        public string HtmlBaseFolder { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            TreeModelList=new List<TreeModel>();
            TreeModelChecked=new List<TreeModel>();
            Profiles = Helper.GetFirefoxProfiles();
            RefreshCombobox();
        }

        public void RefreshCombobox()
        {
            IEnumerator enumerator = Profiles.Keys.GetEnumerator();
            while (enumerator.MoveNext())
            {
                CboxDataPath.Items.Add(enumerator.Current.ToString());
            }
            CboxDataPath.SelectedIndex = 0;
            FillTree();
        }

        #region  fill treeview

        private void FillTree()
        {
            if (TxtDataPath.Text != "")
            {
                string str = string.Concat(TxtDataPath.Text, "\\scrapbook.rdf");
                var xmlDocument = new XmlDocument();
                xmlDocument.Load(str); 
                TreeModelList.Clear();
                AddItemToList(null, xmlDocument.DocumentElement, "urn:scrapbook:root");
                MyTree.ItemsSourceData = TreeModelList;
            }

        }

        private void AddItemToList(TreeModel tn, XmlNode parent, string about)
        {
            XmlNode xmlNode = SeqNode(parent, about);
            if (xmlNode == null)
            {
                return;
            }
            foreach (XmlNode xmlNode2 in xmlNode.ChildNodes)
            {
                if (xmlNode2.Name == "RDF:li")
                {
                    TreeModel treeModel;
                    if (tn == null)
                    {
                        XmlNode node = DefNode(parent, xmlNode2.Attributes["RDF:resource"].Value);
                        string attributeValue = GetAttributeValue(node, "title");
                        treeModel = new TreeModel();
                        treeModel.Name = attributeValue;
                        treeModel.Id = GetAttributeValue(node, "id");
                        TreeModelList.Add(treeModel);
                    }
                    else
                    {
                        XmlNode node = DefNode(parent, xmlNode2.Attributes["RDF:resource"].Value);
                        string attributeValue2 = GetAttributeValue(node, "title");
                        treeModel = new TreeModel();
                        treeModel.Name = attributeValue2;
                        treeModel.Id = GetAttributeValue(node, "id");
                        treeModel.Parent = tn;
                        tn.Children.Add(treeModel);
                    }
                    AddItemToList(treeModel, parent, xmlNode2.Attributes["RDF:resource"].Value);
                }
            }
        }

        private XmlNode SeqNode(XmlNode parent, string about)
        {
            foreach (XmlNode xmlNode in parent.ChildNodes)
            {
                if (xmlNode.Name == "RDF:Seq" && xmlNode.Attributes["RDF:about"].Value == about)
                {
                    return xmlNode;
                }
            }
            return null;
        }

        private XmlNode DefNode(XmlNode parent, string about)
        {
            foreach (XmlNode xmlNode in parent.ChildNodes)
            {
                if (xmlNode.Name == "RDF:Description" && xmlNode.Attributes["RDF:about"].Value == about)
                {
                    return xmlNode;
                }
            }
            return null;
        }

        private string GetAttributeValue(XmlNode node, string attributeName)
        {
            string @value;
            IEnumerator enumerator = node.Attributes.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    XmlAttribute current = (XmlAttribute) enumerator.Current;
                    if (current.Name.IndexOf(attributeName) < 0)
                    {
                        continue;
                    }
                    @value = current.Value;
                    return @value;
                }
                return null;
            }
            finally
            {
                IDisposable disposable = enumerator as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
            return @value;
        }

        #endregion

        private void CboxDataPath_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string str = CboxDataPath.SelectedItem.ToString();
            string str1 = Profiles[str].ToString();
            if (string.IsNullOrEmpty(str1))
            {
                TxtDataPath.Text = "";
            }
            else
            {
                TxtDataPath.Text = str1;
            }
        }

        private void BtnFolderBrowser_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog();
            dialog.ShowNewFolderButton = true;
            var result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                TxtPdfSaveFolder.Text = dialog.SelectedPath;
            }
        }

        private void TxtDataPath_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            FillTree();
        }

        private void BtnConvert_OnClick(object sender, RoutedEventArgs e)
        {
            TreeModelChecked .Clear();
              GetCheckedNode(TreeModelList);
            if (!TreeModelChecked.Any())
            {
                MessageBox.Show(" please select a node ");
                return;
            }
            PdfBaseFolder =   TxtPdfSaveFolder.Text;
            if (string.IsNullOrEmpty(PdfBaseFolder))
            {
                MessageBox.Show(" please select a folder ");
                  return;
            }
            HtmlBaseFolder = Path.Combine(TxtDataPath.Text, "Data");
            //var dialog = this.ShowProgressAsync("正在将Html转换为Pdf", "请稍后...");
            var dialog = new Ookii.Dialogs.Wpf.ProgressDialog();
            dialog.ShowCancelButton = false;
            dialog.Text = "正在转换，请稍后";
            dialog.DoWork += DoConvert;
            dialog.RunWorkerCompleted += DoConvertCompleted;
            dialog.ShowDialog();
        }

        private void GetCheckedNode(IEnumerable<TreeModel> list )
        {
            var checkedNodes = list.Where(x => x.IsChecked);
            foreach (var node in checkedNodes)
            {
                if (node.Children.Count>0)
                {
                    GetCheckedNode(node.Children);
                }
                else
                {
                    TreeModelChecked.Add(node);
                }
            }
        }

        private void DoConvert(object sender, DoWorkEventArgs e)
        {   
            var pdfFolderTemp = Path.Combine(PdfBaseFolder, Guid.NewGuid().ToString());
            if (!Directory.Exists(pdfFolderTemp))
            {
                Directory.CreateDirectory(pdfFolderTemp);
            }
            var dialog = (Ookii.Dialogs.Wpf.ProgressDialog)sender;
            var i = 0;
             foreach (var node in TreeModelChecked)
             {
                 i++;
                 var htmlFilePath = Path.Combine(HtmlBaseFolder, node.Id, "index.html");
                if (File.Exists(htmlFilePath))
                {
                    var pdfPath = Path.Combine(pdfFolderTemp, node.Name + ".pdf");
                    var result = Converter.Run(pdfPath, htmlFilePath);
                 
                    dialog.ReportProgress(i / TreeModelChecked.Count * 100,"正在转换，请稍后","完成..."+ node.Name);
                }
            }
             dialog.ReportProgress(i / TreeModelChecked.Count * 100, "正在合并，请稍后", "合并中..." );
            var pdfs = Directory.GetFiles(pdfFolderTemp, "*.pdf");
            Converter.Combine(pdfs);
        }

        private void DoConvertCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error!=null)
            {
                MessageBox.Show("发生错误"+e.Error.Message);
            }
            else
            {
                MessageBox.Show("转换完成");
                System.Diagnostics.Process.Start("explorer.exe", PdfBaseFolder);
            } 
        }
    }
}