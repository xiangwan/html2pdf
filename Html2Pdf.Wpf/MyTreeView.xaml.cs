using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media; 
using Html2Pdf.Wpf.Models;

namespace Html2Pdf.Wpf
{
    public partial class MyTreeView : UserControl
    {
        #region 私有变量属性

        /// <summary>
        ///     控件数据
        /// </summary>
        private IList<TreeModel> _itemsSourceData;

        #endregion

        /// <summary>
        ///     构造
        /// </summary>
        public MyTreeView()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     控件数据
        /// </summary>
        public IList<TreeModel> ItemsSourceData
        {
            get { return _itemsSourceData; }
            set
            {
                _itemsSourceData = value;
                TvZsmTree.ItemsSource = _itemsSourceData;
            }
        }

        /// <summary>
        ///     设置对应Id的项为选中状态
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int SetCheckedById(string id, IList<TreeModel> treeList)
        {
            foreach (TreeModel tree in treeList)
            {
                if (tree.Id.Equals(id))
                {
                    tree.IsChecked = true;
                    return 1;
                }
                if (SetCheckedById(id, tree.Children) == 1)
                {
                    return 1;
                }
            }

            return 0;
        }

        /// <summary>
        ///     设置对应Id的项为选中状态
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int SetCheckedById(string id)
        {
            foreach (TreeModel tree in ItemsSourceData)
            {
                if (tree.Id.Equals(id))
                {
                    tree.IsChecked = true;
                    return 1;
                }
                if (SetCheckedById(id, tree.Children) == 1)
                {
                    return 1;
                }
            }

            return 0;
        }

        /// <summary>
        ///     获取选中项
        /// </summary>
        /// <returns></returns>
        public IList<TreeModel> CheckedItemsIgnoreRelation()
        {
            return GetCheckedItemsIgnoreRelation(_itemsSourceData);
        }

        /// <summary>
        ///     私有方法，忽略层次关系的情况下，获取选中项
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private IList<TreeModel> GetCheckedItemsIgnoreRelation(IList<TreeModel> list)
        {
            IList<TreeModel> treeList = new List<TreeModel>();
            foreach (TreeModel tree in list)
            {
                if (tree.IsChecked)
                {
                    treeList.Add(tree);
                }
                foreach (TreeModel child in GetCheckedItemsIgnoreRelation(tree.Children))
                {
                    treeList.Add(child);
                }
            }
            return treeList;
        }

        /// <summary>
        ///     选中所有子项菜单事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuSelectAllChild_Click(object sender, RoutedEventArgs e)
        {
            if (TvZsmTree.SelectedItem != null)
            {
                var tree = (TreeModel) TvZsmTree.SelectedItem;
                tree.IsChecked = true;
                tree.SetChildrenChecked(true);
            }
        }

        /// <summary>
        ///     全部展开菜单事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuExpandAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (TreeModel tree in TvZsmTree.ItemsSource)
            {
                tree.IsExpanded = true;
                tree.SetChildrenExpanded(true);
            }
        }

        /// <summary>
        ///     全部折叠菜单事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuUnExpandAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (TreeModel tree in TvZsmTree.ItemsSource)
            {
                tree.IsExpanded = false;
                tree.SetChildrenExpanded(false);
            }
        }

        /// <summary>
        ///     全部选中事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuSelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (TreeModel tree in TvZsmTree.ItemsSource)
            {
                tree.IsChecked = true;
                tree.SetChildrenChecked(true);
            }
        }

        /// <summary>
        ///     全部取消选中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuUnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (TreeModel tree in TvZsmTree.ItemsSource)
            {
                tree.IsChecked = false;
                tree.SetChildrenChecked(false);
            }
        }

        /// <summary>
        ///     鼠标右键事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeViewItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;
            if (item != null)
            {
                item.Focus();
                e.Handled = true;
            }
        }

        private static DependencyObject VisualUpwardSearch<T>(DependencyObject source)
        {
            while (source != null && source.GetType() != typeof (T))
                source = VisualTreeHelper.GetParent(source);

            return source;
        }
    }
}