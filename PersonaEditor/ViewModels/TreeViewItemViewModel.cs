using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using PersonaEditor.Common;

namespace PersonaEditor.ViewModels
{
    public class SubItemPropertyChangedEventArgs : EventArgs
    {
        public SubItemPropertyChangedEventArgs(int level, TreeViewItemViewModel subItem, string propertyName)
        {
            Level = level;
            SubItem = subItem;
            PropertyName = propertyName;
        }

        public int Level { get; }

        public TreeViewItemViewModel SubItem { get; }

        public string PropertyName { get; }
    }

    public delegate void SubItemPropertyChangedEventHandler(object sender, SubItemPropertyChangedEventArgs e);

    public class TreeViewItemViewModel : BindingObject
    {
        private readonly List<TreeViewItemViewModel> _observedItems = new List<TreeViewItemViewModel>();

        private string _header = string.Empty;
        private bool _isEnabled = true;
        private bool _isExpanded = false;
        private bool _isSelected = false;

        public TreeViewItemViewModel()
        {
            SubItems.CollectionChanged += SubItems_CollectionChanged;
        }

        public ObservableCollection<TreeViewItemViewModel> SubItems { get; } = new ObservableCollection<TreeViewItemViewModel>();

        public ObservableCollection<object> ContextMenu { get; } = new ObservableCollection<object>();

        public string Header
        {
            get => _header;
            set => SetProperty(ref _header, value);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        private void SubItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (TreeViewItemViewModel item in e.NewItems)
                    {
                        _observedItems.Add(item);
                        item.PropertyChanged += Item_PropertyChanged;
                        item.SubItemPropertyChanged += Item_SubItemPropertyChanged;
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (TreeViewItemViewModel item in e.OldItems)
                    {
                        _observedItems.Remove(item);
                        item.PropertyChanged -= Item_PropertyChanged;
                        item.SubItemPropertyChanged -= Item_SubItemPropertyChanged;
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    foreach (var item in _observedItems)
                    {
                        item.PropertyChanged -= Item_PropertyChanged;
                        item.SubItemPropertyChanged -= Item_SubItemPropertyChanged;
                    }
                    _observedItems.Clear();
                    break;

                case NotifyCollectionChangedAction.Move:
                    break;

                case NotifyCollectionChangedAction.Replace:
                    foreach (TreeViewItemViewModel item in e.OldItems)
                    {
                        _observedItems.Remove(item);
                        item.PropertyChanged -= Item_PropertyChanged;
                        item.SubItemPropertyChanged -= Item_SubItemPropertyChanged;
                    }

                    foreach (TreeViewItemViewModel item in e.NewItems)
                    {
                        _observedItems.Add(item);
                        item.PropertyChanged += Item_PropertyChanged;
                        item.SubItemPropertyChanged += Item_SubItemPropertyChanged;
                    }
                    break;

                default:
                    throw new Exception($"SubItems_CollectionChanged - {e.Action}");
            }
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var arg = new SubItemPropertyChangedEventArgs(0, sender as TreeViewItemViewModel, e.PropertyName);
            SubItemPropertyChanged?.Invoke(this, arg);
        }

        private void Item_SubItemPropertyChanged(object sender, SubItemPropertyChangedEventArgs e)
        {
            var arg = new SubItemPropertyChangedEventArgs(e.Level + 1, e.SubItem, e.PropertyName);
            SubItemPropertyChanged?.Invoke(this, arg);
        }

        public event SubItemPropertyChangedEventHandler SubItemPropertyChanged;
    }
}