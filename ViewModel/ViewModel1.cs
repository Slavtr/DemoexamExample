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

        public ObservableCollection<Service> Services { get; set; }
        public ObservableCollection<ServiceClient> ServiceClients { get; set; }
        public ObservableCollection<User> Users { get; set; }
        public ViewModel1()
        {
            Services = new ObservableCollection<Service>(_entities.Service);
            ServiceClients = new ObservableCollection<ServiceClient>(_entities.ServiceClient);
            Users = new ObservableCollection<User>(_entities.User);
        }
        private void FullPathPuttingOrder()
        {
            foreach(Service service in _entities.Service)
            {
                service.MainImagePath = service.MainImagePath.Trim();
            }
            _entities.SaveChanges();
        }
        public void ListBoxService_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (ListBoxItem listBoxItem in e.NewItems)
                    {
                        Image img = listBoxItem.FindName("lbiPicture") as Image;
                        if (img != null)
                        {
                            BitmapImage src = new BitmapImage();
                            src.BeginInit();
                            src.UriSource = new Uri((listBoxItem.DataContext as Service).MainImagePath);
                            src.EndInit();
                            img.Source = src;
                        }

                        Button bRedact = listBoxItem.FindName("lbiButtonRedact") as Button;
                        if (bRedact != null)
                        {
                            bRedact.IsEnabled = _isAdmin;
                            bRedact.Click += ServiceList_OnClick;
                        }
                        Button bDelete = listBoxItem.FindName("lbiButtonDelete") as Button;
                        if (bRedact != null)
                        {
                            bDelete.IsEnabled = _isAdmin;
                            bDelete.Click += ServiceList_OnClick;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach(ListBoxItem listBoxItem in e.NewItems)
                    {
                        (sender as ItemCollection).Remove(listBoxItem);
                        Services.Remove(listBoxItem.DataContext as Service);
                    }
                    break;
            }
        }

        private void ServiceList_OnClick(object sender, EventArgs e)
        {
            switch((sender as Button).Name)
            {
                case "AddButton":
                    MainFrame.Content = Pages.FirstOrDefault(x => x.Name == "ServiceCard");
                    break;
                case "lbiButtonDelete":
                    
                    break;
                case "lbiButtonRedact":
                    MainFrame.Content = Pages.FirstOrDefault(x => x.Name == "ServiceCard");
                    break;
            }
        }

    }
}
