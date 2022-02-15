using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Collections.Specialized;
using System.Windows.Input;
using System.Collections;
using System.Collections.ObjectModel;
using ViewModel.Commands;

namespace ViewModel
{
    public class ViewModel1 : INotifyPropertyChanged
    {
        private Entities _entities = new Entities();
        /// <summary>
        /// Главный фрейм
        /// </summary>
        public Frame MainFrame { get; set; } = new Frame();
        /// <summary>
        /// Список страниц
        /// </summary>
        public List<Page> Pages { get; set; } = new List<Page>();
        private bool _isAdmin = false;

        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Код авторизации. Если равен "0000" - авторизируется администратор
        /// </summary>
        public string Code 
        { 
            set
            {
                if(value == "0000")
                {
                    _isAdmin = true;
                }
            } 
        }
        /// <summary>
        /// Переведёт на страницу с ServiceList в названии
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Authorisation(object sender, EventArgs e)
        {
            MainFrame.Content = Pages.First(x => x.Name.Contains("ServiceList"));
        }

        public ServicesVM CurrentService { get; set; }
        public ObservableCollection<ServicesVM> Services { get; set; } = new ObservableCollection<ServicesVM>();
        private void LoadServices()
        {
            foreach(Service service in _entities.Service)
            {
                Services.Add(new ServicesVM(service, _isAdmin, this.GetType(), ExecuteServiceCommand, CanExecuteCommand));
            }
        }
        public ObservableCollection<ServiceClient> ServiceClients { get; set; } = new ObservableCollection<ServiceClient>();
        public ObservableCollection<User> Users { get; set; } = new ObservableCollection<User>();
        public ViewModel1()
        {
            LoadServices();
        }

        private void FullPathPuttingOrder()
        {
            foreach(Service service in _entities.Service)
            {
                service.MainImagePath = service.MainImagePath.Trim();
            }
            _entities.SaveChanges();
        }

        private void Service_Delete()
        {
            Services.Remove(CurrentService);
        }

        public void ExecuteServiceCommand(object sender, ExecutedRoutedEventArgs e)
        {
            switch((e.Command as RoutedCommand).Name)
            {
                case "Delete":
                    Service_Delete();
                    break;
                case "Change":
                    break;
            }
        }

        private void CanExecuteCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _isAdmin;
        }
    }

    public struct ServicesVM
    {
        public Service Service { get; set; }
        public bool IsAdmin;

        public ServicesVM(Service service, bool isAdmin, Type mainViewType, Action<object, ExecutedRoutedEventArgs> executeServiceCommand, Action<object, CanExecuteRoutedEventArgs> canExecute)
        {
            Service = service;
            IsAdmin = isAdmin;
            Delete = new RoutedCommand("Delete", mainViewType.GetType());
            Change = new RoutedCommand("Change", mainViewType.GetType());
            
            _serviceDeleteCommandBinding = new CommandBinding(Delete, executeServiceCommand.Invoke, canExecute.Invoke);
            _serviceChangeCommandBinding = new CommandBinding(Change, executeServiceCommand.Invoke, canExecute.Invoke);
        }

        //private void ServiceItem_OnLoad(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    ListBoxItem listBoxItem = sender as ListBoxItem;
        //    Image img = listBoxItem.FindName("lbiPicture") as Image;
        //    if (img != null)
        //    {
        //        BitmapImage src = new BitmapImage();
        //        src.BeginInit();
        //        src.UriSource = new Uri((listBoxItem.DataContext as Service).MainImagePath);
        //        src.EndInit();
        //        img.Source = src;
        //    }

        //    Button bRedact = listBoxItem.FindName("lbiButtonRedact") as Button;
        //    if (bRedact != null)
        //    {
        //        bRedact.IsEnabled = _isAdmin;
        //        bRedact.Click += ServiceList_OnClick;
        //    }
        //    Button bDelete = listBoxItem.FindName("lbiButtonDelete") as Button;
        //    if (bRedact != null)
        //    {
        //        bDelete.IsEnabled = _isAdmin;
        //        bDelete.Click += ServiceList_OnClick;
        //    }
        //}
        
        public RoutedCommand Delete { get; set; }
        public RoutedCommand Change { get; set; }

        private CommandBinding _serviceDeleteCommandBinding;
        private CommandBinding _serviceChangeCommandBinding;
    }
}
