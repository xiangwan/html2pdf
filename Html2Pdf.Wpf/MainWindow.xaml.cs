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
using NLog;
using Ookii.Dialogs.Wpf;
using Path = System.IO.Path;
using ProgressDialog = Ookii.Dialogs.Wpf.ProgressDialog;

namespace Html2Pdf.Wpf
{
    public partial class MainWindow : MetroWindow
    {
        public Hashtable Profiles { get; set; }
        public List<TreeModel> TreeModelList { get; set; }
        public List<TreeModel> TreeModelChecked { get; set; }

        public string PdfBaseFolder { get; set; }
        public string PdfFolderTemp { get; set; }
        public string PdfFileName { get; set; }
        public int PdfFontSize { get; set; }
        public string HtmlBaseFolder { get; set; } 
        public bool TabScrapbookIsReady { get; set; }
        public string[] HtmlFiles { get; set; }

        public MainWindow()
        {
            InitializeComponent(); 
        }
        #region  scrapbook tab
        private void TabScrapbook_OnLoaded(object sender, RoutedEventArgs e) {
            if (!TabScrapbookIsReady) {
                TreeModelList = new List<TreeModel>();
                TreeModelChecked = new List<TreeModel>();
                Profiles = Helper.GetFirefoxProfiles();
                RefreshCombobox();
                TabScrapbookIsReady = true;
            }
        }
        public void RefreshCombobox()
        {
            IEnumerator enumerator = Profiles.Keys.GetEnumerator();
            while (enumerator.MoveNext())
            {
                ComboxScrapbookDataPath.Items.Add(enumerator.Current.ToString());
            }
            ComboxScrapbookDataPath.SelectedIndex = 0;
            FillTree();
        }

        #region  fill treeview

        private void FillTree()
        {
            if (TxtScrapbookDataPath.Text != "")
            {
                string str = string.Concat(TxtScrapbookDataPath.Text, "\\scrapbook.rdf");
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

        private void ComboxScrapbookDataPath_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string str = ComboxScrapbookDataPath.SelectedItem.ToString();
            string str1 = Profiles[str].ToString();
            if (string.IsNullOrEmpty(str1))
            {
                TxtScrapbookDataPath.Text = "";
            }
            else
            {
                TxtScrapbookDataPath.Text = str1;
            }
        }
        private void TxtScrapbookDataPath_OnTextChanged(object sender, TextChangedEventArgs e) {
            FillTree();
        }
        #endregion


        private void BtnPdfSaveFolder_OnClick(object sender, RoutedEventArgs e)
        { 
            var dialog = new SaveFileDialog();
            dialog.CheckPathExists = true;
           
            dialog.CreatePrompt = true;
            dialog.OverwritePrompt = true;
            dialog.DefaultExt = "pdf";
            dialog.Filter = "pdf file(*.pdf)|*.pdf";
         
            var result = dialog.ShowDialog();
            if (result.Value) {
                var file = dialog.FileName;
                TxtPdfSaveFolder.Text =file ;
                PdfBaseFolder = new DirectoryInfo(dialog.FileName).Parent.FullName;
                PdfFileName = new FileInfo(file).Name.Replace(".pdf","");
            }
        }

        private  void BtnConvert_OnClick(object sender, RoutedEventArgs e)
        {
            var strFontSize = TxtPdfFontSize.Text;
            int iFontSize;
            if (int.TryParse(strFontSize,out iFontSize))
            {
                PdfFontSize = iFontSize;
            }
            else
            {
                this.ShowMessageAsync("提示", "PDF字体大小必须是数字");
                return;
            }
            var strPath = TxtPdfSaveFolder.Text;
            if (string.IsNullOrEmpty(strPath)) {
                this.ShowMessageAsync("提示", "请选择PDF文件保存位置"); 
                return;
            }
            PdfBaseFolder = new DirectoryInfo(strPath).Parent.FullName;
            var dialog = new Ookii.Dialogs.Wpf.ProgressDialog(); 
            var tabIndex = MyTabs.SelectedIndex;
            if (tabIndex == 1) {//scrapbook
                TreeModelChecked.Clear();
                GetCheckedNode(TreeModelList);
                if (!TreeModelChecked.Any()) {
                    this.ShowMessageAsync("提示", "请选择需要转换的节点");  
                    return;
                } 
                HtmlBaseFolder = Path.Combine(TxtScrapbookDataPath.Text, "Data");
                dialog.DoWork += DoScrapbookConvert;
            }
            else if (tabIndex==0) {
                HtmlBaseFolder = TxtHtmlFilesFolder.Text;
                if (string.IsNullOrEmpty(HtmlBaseFolder)) {
                    this.ShowMessageAsync("提示", "请选择Html文件所在目录");
                    return;
                }
               var isSearchSub = !(RbtnNoSub.IsChecked.HasValue&&RbtnNoSub.IsChecked.Value);
               var searchOption = isSearchSub ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
               HtmlFiles = "*.html".Split('|').SelectMany(filter =>Directory.GetFiles(HtmlBaseFolder, filter, searchOption)).ToArray();
                     
                if (!HtmlFiles.Any()) {
                    this.ShowMessageAsync("提示", "所选目录没有搜索到任何Html文件："+HtmlBaseFolder);
                    return;
                }
                dialog.DoWork += DoHtmlConvert;
            }
            PdfFolderTemp = Path.Combine(PdfBaseFolder, Guid.NewGuid().ToString());
            if (!Directory.Exists(PdfFolderTemp)) {
                Directory.CreateDirectory(PdfFolderTemp);
            } 
            dialog.ShowCancelButton = false;
            dialog.Text = "正在转换，请稍后"; 
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
        private void DoHtmlConvert(object sender, DoWorkEventArgs e) {

            var dialog = (Ookii.Dialogs.Wpf.ProgressDialog)sender;
            var i = 0;
            var x = HtmlFiles.Count();
            foreach (var file in HtmlFiles) {
                i++;
                if (File.Exists(file)) {
                    var fileName = new FileInfo(file).Name.Replace(".html","").Replace(".htm","");
                    var pdfPath = Path.Combine(PdfFolderTemp, fileName + ".pdf");
                    var result = Converter.Run(pdfPath, file,PdfFontSize);
                    dialog.ReportProgress(i / x * 100, "正在转换，请稍后", result +" ... "+ fileName);
                }
            }
            CombinePdfs(dialog);
        }
        private void DoScrapbookConvert(object sender, DoWorkEventArgs e)
        {    
            var dialog = (Ookii.Dialogs.Wpf.ProgressDialog)sender;
            var i = 0;
             foreach (var node in TreeModelChecked)
             {
                 i++;
                 var htmlFilePath = Path.Combine(HtmlBaseFolder, node.Id, "index.html");
                if (File.Exists(htmlFilePath))
                {
                    var pdfPath = Path.Combine(PdfFolderTemp, node.Name + ".pdf");
                    var result = Converter.Run(pdfPath, htmlFilePath,PdfFontSize);
                    dialog.ReportProgress(i / TreeModelChecked.Count * 100, "正在转换，请稍后", result + " ... " + node.Name);
                }
            }
             CombinePdfs(dialog);
        } 
        private void CombinePdfs(ProgressDialog dialog) {
            dialog.ReportProgress(100, "正在合并，请稍后", "合并中...");
            var pdfs = Directory.GetFiles(PdfFolderTemp, "*.pdf");
            Converter.Combine(pdfs,PdfFileName);
            Directory.Delete(PdfFolderTemp, true);
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

        private void BtnSelectHtmlFilesFolder_OnClick(object sender, RoutedEventArgs e) {
            var dialog = new VistaFolderBrowserDialog();
            dialog.ShowNewFolderButton = false;
           var result= dialog.ShowDialog();
            if (result.HasValue&&result.Value) {
                TxtHtmlFilesFolder.Text = dialog.SelectedPath; 
            }
        }

        private void BtnHelp_OnClick(object sender, RoutedEventArgs e) {
            System.Diagnostics.Process.Start("https://github.com/xiangwan/html2pdf");
        }

        private void BtnAbout_OnClick(object sender, RoutedEventArgs e) {
            var version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
            this.ShowMessageAsync("关于", string.Format("html to pdf verson {0} . \r\ncreate by xiangwan", version));
        }
    }
}