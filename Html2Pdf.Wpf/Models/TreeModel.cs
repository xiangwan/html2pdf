using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Html2Pdf.Wpf.Models
{
    public class TreeModel : INotifyPropertyChanged
    {
        #region 私有变量

        /// <summary>
        /// 选中状态
        /// </summary>
        private bool _isChecked;
        /// <summary>
        /// 折叠状态
        /// </summary>
        private bool _isExpanded;

        #endregion

        /// <summary>
        /// 构造
        /// </summary>
        public TreeModel() {
            Children = new List<TreeModel>();
            _isChecked = false;
            IsExpanded = false;
            //Icon = "/Images/16_16/folder_go.png";
        }

        /// <summary>
        /// 键值
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 显示的字符
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// 指针悬停时的显示说明
        /// </summary>
        public string ToolTip {
            get {
                return String.Format("{0}-{1}", Id, Name);
            }
        }

        /// <summary>
        /// 是否选中
        /// </summary>
        public bool IsChecked {
            get {
                return _isChecked;
            }
            set {
                if (value != _isChecked) {
                    _isChecked = value;
                    NotifyPropertyChanged("IsChecked");

                    if (_isChecked) {
                        //如果选中则父项也应该选中
                       /* if (Parent != null) {
                            Parent.IsChecked = true;
                        }*/
                        //如果取消选中子项也应该取消选中
                        foreach (TreeModel child in Children) {
                            child.IsChecked = true;
                        }
                    }
                    else {
                        //如果取消选中子项也应该取消选中
                        foreach (TreeModel child in Children) {
                            child.IsChecked = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 是否展开
        /// </summary>
        public bool IsExpanded {
            get { return _isExpanded; }
            set {
                if (value != _isExpanded) {
                    //折叠状态改变
                    _isExpanded = value;
                    NotifyPropertyChanged("IsExpanded");
                }
            }
        }

        /// <summary>
        /// 父项
        /// </summary>
        public TreeModel Parent { get; set; }

        /// <summary>
        /// 子项
        /// </summary>
        public IList<TreeModel> Children { get; set; }

        /// <summary>
        /// 设置所有子项的选中状态
        /// </summary>
        /// <param name="isChecked"></param>
        public void SetChildrenChecked(bool isChecked) {
            foreach (TreeModel child in Children) {
                child.IsChecked = IsChecked;
                child.SetChildrenChecked(IsChecked);
            }
        }

        /// <summary>
        /// 设置所有子项展开状态
        /// </summary>
        /// <param name="isExpanded"></param>
        public void SetChildrenExpanded(bool isExpanded) {
            foreach (TreeModel child in Children) {
                child.IsExpanded = isExpanded;
                child.SetChildrenExpanded(isExpanded);
            }
        }

        /// <summary>
        /// 属性改变事件
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
