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
                Services.Add(new ServicesVM(service, _isAdmin, Service_Action, Service_Delete));
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

        private void Service_Action(string pageName, int mode)
        {
            switch (mode)
            {
                case 1:
                    CurrentService = new ServicesVM();
                    Services.Add(CurrentService);
                    MainFrame.Content = Pages.FirstOrDefault(x => x.Name == pageName);
                    break;
                case 2:
                    MainFrame.Content = Pages.FirstOrDefault(x => x.Name == pageName);
                    break;
            }
        }
        private void Service_Delete()
        {
            Services.Remove(CurrentService);
        }
    }

    public struct ServicesVM
    {
        public Service Service { get; set; }
        public bool _isAdmin;

        public ServicesVM(Service service, bool isAdmin, MainFrameChange method, ServiceDelete delete)
        {
            Service = service;
            _isAdmin = isAdmin;
            MainFrameChangeEvent = method;
            ServiceDeleteEvent = delete;
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

        public delegate void MainFrameChange(string frameName, int mode);
        public event MainFrameChange MainFrameChangeEvent;

        public delegate void ServiceDelete();
        public event ServiceDelete ServiceDeleteEvent;

        //ServiceListItemCommand _itemCommand;
        //public ICommand ItemCommand
        //{
        //    get
        //    {
        //        return _itemCommand;
        //    }
        //    set
        //    {
        //        if (_itemCommand == null)
        //        {
        //            _itemCommand = (ServiceListItemCommand)value;
        //        }
        //    }
        //}
    }
}
