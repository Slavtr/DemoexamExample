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
        public bool IsAdmin { get; private set; } = false;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
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
                    IsAdmin = true;
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
            MainFrame.Content = Pages.First(x => x.Title.Contains("ServiceList"));
            OnPropertyChanged(nameof(IsAdmin));
            LoadServices();
        }
        
        public ObservableCollection<ServiceClient> ServiceClients { get; set; } = new ObservableCollection<ServiceClient>();
        public ObservableCollection<User> Users { get; set; } = new ObservableCollection<User>();
        public ViewModel1()
        {
        }

        private void FullPathPuttingOrder()
        {
            foreach(Service service in _entities.Service)
            {
                service.MainImagePath = service.MainImagePath.Trim();
            }
            _entities.SaveChanges();
        }

        #region ServicesVM

        public ServicesVM CurrentService { get; set; }
        public ObservableCollection<ServicesVM> Services { get; set; } = new ObservableCollection<ServicesVM>();

        private void Service_Delete()
        {
            _entities.Service.Remove(CurrentService.Service);
            Services.Remove(CurrentService);
            _entities.SaveChanges();
        }

        private void ExecuteServiceCommand(object sender, ExecutedRoutedEventArgs e)
        {
            switch((e.Command as RoutedCommand).Name)
            {
                case "Delete":
                    Service_Delete();
                    break;
                case "Change":
                    break;
                case "Add":
                    CurrentService = new ServicesVM(new Service(), IsAdmin, this.GetType(), ExecuteServiceCommand, CanExecuteServiceCommand);
                    MainFrame.Content = Pages.First(x => x.Title.Contains("ServiceCard"));
                    break;
            }
        }

        private void CanExecuteServiceCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsAdmin;
        }

        private void LoadServices()
        {
            foreach (Service service in _entities.Service)
            {
                Services.Add(new ServicesVM(service, IsAdmin, this.GetType(), ExecuteServiceCommand, CanExecuteServiceCommand));
                ServiceVMCommandBindings.Add(Services.Last().ServiceChangeCommandBinding);
                ServiceVMCommandBindings.Add(Services.Last().ServiceDeleteCommandBinding);
            }
            ServiceAddCommand = new RoutedCommand("Add", this.GetType());
            ServiceAddCommandBinding = new CommandBinding(ServiceAddCommand, ExecuteServiceCommand, CanExecuteServiceCommand);
            ServiceVMCommandBindings.Add(ServiceAddCommandBinding);
        }

        public CommandBindingCollection ServiceVMCommandBindings { get; set; } = new CommandBindingCollection();
        public RoutedCommand ServiceAddCommand { get; private set; }
        private CommandBinding ServiceAddCommandBinding { get; set; }

        #region ServiceCard

        public CommandBindingCollection ServiceCardCommandBindings { get; set; } = new CommandBindingCollection();

        public RoutedCommand ServiceCardSaveCommand { get; private set; }
        private CommandBinding ServiceCardSaveCommandBinding { get; set; }

        public RoutedCommand ServiceCardRejectCommand { get; private set; }
        private CommandBinding ServiceCardRejectCommandBinding { get; set; }

        public RoutedCommand ServiceCardChooseImageCommand { get; private set; }
        private CommandBinding ServiceCardChooseImageCommandBinding { get; set; }

        private void ExecuteServiceCardCommand(object sender, ExecutedRoutedEventArgs e)
        {
            switch ((e.Command as RoutedCommand).Name)
            {
                case "Delete":
                    Service_Delete();
                    break;
                case "Change":
                    break;
                case "Add":
                    CurrentService = new ServicesVM(new Service(), IsAdmin, this.GetType(), ExecuteServiceCommand, CanExecuteServiceCommand);
                    MainFrame.Content = Pages.First(x => x.Title.Contains("ServiceCard"));
                    break;
            }
        }

        private void CanExecuteServiceCardCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            if(CurrentService != null)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
        }

        #endregion

        #endregion
    }

    public class ServicesVM
    {
        public Service Service { get; set; }
        public bool IsAdmin { get; private set; }

        private BitmapImage _mainImage;
        public BitmapImage MainImage
        {
            get
            {
                if (_mainImage == null)
                {
                    _mainImage = new BitmapImage(new Uri(String.Format("\\{0}", Service.MainImagePath), UriKind.Relative));
                }
                return _mainImage;
            }
        }

        public ServicesVM(Service service, bool isAdmin, Type mainVMType, Action<object, ExecutedRoutedEventArgs> executeServiceCommand, Action<object, CanExecuteRoutedEventArgs> canExecute)
        {
            Service = service;
            IsAdmin = isAdmin;

            DeleteCommand = new RoutedCommand("Delete", this.GetType());
            ChangeCommand = new RoutedCommand("Change", this.GetType());

            ServiceDeleteCommandBinding = new CommandBinding(DeleteCommand, executeServiceCommand.Invoke, canExecute.Invoke);
            ServiceChangeCommandBinding = new CommandBinding(ChangeCommand, executeServiceCommand.Invoke, canExecute.Invoke);
        }

        public RoutedCommand DeleteCommand { get; private set; }
        public RoutedCommand ChangeCommand { get; private set; }

        public CommandBinding ServiceDeleteCommandBinding { get; private set; }
        public CommandBinding ServiceChangeCommandBinding { get; private set; }
    }
}
