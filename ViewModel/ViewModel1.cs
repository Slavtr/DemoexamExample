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
using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace ViewModel
{
    public class ViewModel1 : INotifyPropertyChanged
    {
        private const string _imagePath = "Услуги школы\\";
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
            LoadUsers();
        }
        
        public ViewModel1()
        {
            ServiceAddCommand = new RoutedCommand("Add", typeof(ServicesVM));
            ServiceAddCommandBinding = new CommandBinding(ServiceAddCommand, ExecuteServiceCommand, CanExecuteServiceCommand);

            FilterClearCommand = new RoutedCommand("ClearFilter", GetType());
            FilterClearCommandBinding = new CommandBinding(FilterClearCommand, FilterClear, CanExecuteServiceCommand);

            ServiceCardSaveCommand = new RoutedCommand("Save", GetType());
            ServiceCardRejectCommand = new RoutedCommand("Reject", GetType());
            ServiceCardChooseImageCommand = new RoutedCommand("ChooseImage", GetType());

            ServiceCardSaveCommandBinding = new CommandBinding(ServiceCardSaveCommand, ExecuteServiceCardCommand, CanExecuteServiceCardCommand);
            ServiceCardRejectCommandBinding = new CommandBinding(ServiceCardRejectCommand, ExecuteServiceCardCommand, CanExecuteServiceCardCommand);
            ServiceCardChooseImageCommandBinding = new CommandBinding(ServiceCardChooseImageCommand, ExecuteServiceCardCommand, CanExecuteServiceCardCommand);

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
        public ObservableCollection<ServicesVM> UnchangedServices { get; set; } = new ObservableCollection<ServicesVM>();

        private void Service_Delete()
        {
            if (_entities.ServiceClient.FirstOrDefault(x => x.Service == CurrentService.Service.Id) == null)
            {
                _entities.Service.Remove(CurrentService.Service);
                Services.Remove(CurrentService);
                _entities.SaveChanges();
            }
        }

        private void ExecuteServiceCommand(object sender, ExecutedRoutedEventArgs e)
        {
            switch((e.Command as RoutedCommand).Name)
            {
                case "Delete":
                    Service_Delete();
                    break;
                case "Change":
                    LoadServiceCard();
                    ChangeService(sender);
                    break;
                case "Add":
                    LoadServiceCard();
                    CurrentService = new ServicesVM(new Service(), IsAdmin, ExecuteServiceCommand, CanExecuteServiceCommand);
                    OnPropertyChanged("CurrentService");
                    MainFrame.Content = Pages.First(x => x.Title.Contains("ServiceCard"));
                    break;
                case "AddUser":
                    AddUserCardLoad(sender);
                    break;
            }
        }

        private void AddUserCardLoad(object sender)
        {
            Type t = sender.GetType();
            System.Reflection.FieldInfo fInfo = t.GetField("lbServices");
            object lb = fInfo.GetValue(sender);
            if (lb != null)
            {
                CurrentService = (lb as ListBox).Items.CurrentItem as ServicesVM;
            }
            OnPropertyChanged("CurrentService");
            MainFrame.Content = Pages.First(x => x.Title.Contains("AddUser"));
        }

        private void ChangeService(object sender)
        {
            Type t = sender.GetType();
            System.Reflection.FieldInfo fInfo = t.GetField("lbServices");
            object lb = fInfo.GetValue(sender);
            if (lb != null)
            {
                CurrentService = (lb as ListBox).Items.CurrentItem as ServicesVM;
            }
            CurrentService.BeginEdit();
            OnPropertyChanged("CurrentService");
            MainFrame.Content = Pages.First(x => x.Title.Contains("ServiceCard"));
        }

        private void CanExecuteServiceCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsAdmin;
        }

        private void LoadServices()
        {
            foreach (Service service in _entities.Service)
            {
                Services.Add(new ServicesVM(service, IsAdmin, ExecuteServiceCommand, CanExecuteServiceCommand));
                ServiceVMCommandBindings.Add(Services.Last().ServiceChangeCommandBinding);
                ServiceVMCommandBindings.Add(Services.Last().ServiceDeleteCommandBinding);
                ServiceVMCommandBindings.Add(Services.Last().AddUserCommandBinding);
                UnchangedServices.Add(new ServicesVM(service, IsAdmin, ExecuteServiceCommand, CanExecuteServiceCommand));
                ServiceVMCommandBindings.Add(UnchangedServices.Last().ServiceChangeCommandBinding);
                ServiceVMCommandBindings.Add(UnchangedServices.Last().ServiceDeleteCommandBinding);
                ServiceVMCommandBindings.Add(UnchangedServices.Last().AddUserCommandBinding);
            }
            ServiceVMCommandBindings.Add(ServiceAddCommandBinding);
            ServiceVMCommandBindings.Add(FilterClearCommandBinding);

        }

        private void LoadUsers()
        {
            foreach (User user in _entities.User)
            {
                Users.Add(user);
            }
            CurrentUser = Users.First();
        }

        public CommandBindingCollection ServiceVMCommandBindings { get; set; } = new CommandBindingCollection();
        public RoutedCommand ServiceAddCommand { get; private set; }
        public CommandBinding ServiceAddCommandBinding { get; set; }

        #region ServiceCard

        private void LoadServiceCard()
        {
            if(ServiceCardCommandBindings.Count == 0)
            {
                ServiceCardCommandBindings.Add(ServiceCardSaveCommandBinding);
                ServiceCardCommandBindings.Add(ServiceCardRejectCommandBinding);
                ServiceCardCommandBindings.Add(ServiceCardChooseImageCommandBinding);
            }
        }

        public CommandBindingCollection ServiceCardCommandBindings { get; set; } = new CommandBindingCollection();

        public RoutedCommand ServiceCardSaveCommand { get; private set; }
        public CommandBinding ServiceCardSaveCommandBinding { get; set; }

        public RoutedCommand ServiceCardRejectCommand { get; private set; }
        public CommandBinding ServiceCardRejectCommandBinding { get; set; }

        public RoutedCommand ServiceCardChooseImageCommand { get; private set; }
        public CommandBinding ServiceCardChooseImageCommandBinding { get; set; }

        private void ExecuteServiceCardCommand(object sender, ExecutedRoutedEventArgs e)
        {
            switch ((e.Command as RoutedCommand).Name)
            {
                case "ChooseImage":
                    OpenFileDialog dlg = new OpenFileDialog
                    {
                        Filter = "Images (.png, .jpg)|*.png;*.jpg"
                    };
                    bool? result = dlg.ShowDialog();

                    if (result == true)
                    {
                        string name = dlg.FileName.Replace("\\", "/").Split('/').Last();

                        File.Copy(dlg.FileName, @"D:\Slava\Программы\Проекты\DemoexamExample\ViewModel\Услуги школы\" + name, true);
                        File.Copy(dlg.FileName, @"D:\Slava\Программы\Проекты\DemoexamExample\Услуги школы\" + name, true);

                        CurrentService.BeginEdit();

                        CurrentService.Service.MainImagePath = name;

                        CurrentService.MainImage = new BitmapImage(new Uri(String.Format("\\{0}", CurrentService.Service.MainImagePath), UriKind.Relative));
                        int index = Services.IndexOf(Services.First(x=> x.Service.Id == CurrentService.Service.Id));
                        Services[index] = CurrentService;
                        OnPropertyChanged("Services");
                    }
                    break;
                case "Reject":
                    CurrentService.CancelEdit();
                    OnPropertyChanged("CurrentService");
                    break;
                case "Save":
                    CurrentService.EndEdit();
                    _entities.SaveChanges();
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

        public ObservableCollection<string> DiscountSort { get; private set; } = new ObservableCollection<string> { "none", "0-5%", "5%-15%" };

        private string _selectedDiscount;
        public string SelectedDiscount
        {
            get { return _selectedDiscount; }
            set
            {
                switch (value)
                {
                    case "none":
                        Services = UnchangedServices;
                        OnPropertyChanged("Services");
                        break;
                    case "0-5%":
                        Services = new ObservableCollection<ServicesVM>(Services.Where(x => x.Service.Discount >= 0 && x.Service.Discount < 5));
                        OnPropertyChanged("Services");
                        break;
                    case "5%-15%":
                        Services = new ObservableCollection<ServicesVM>(Services.Where(x => x.Service.Discount >= 5 && x.Service.Discount < 15));
                        OnPropertyChanged("Services");
                        break;
                }
                _selectedDiscount = value;
                OnPropertyChanged("SelectedDiscount");
            }
        }
        public ObservableCollection<string> CostSort { get; private set; } = new ObservableCollection<string> { "none", "Убыв", "Возр" };

        private string _selectedCost;
        public string SelectedCost
        {
            get { return _selectedCost; }
            set
            {
                switch (value)
                {
                    case "none":
                        Services = UnchangedServices;
                        OnPropertyChanged("Services");
                        break;
                    case "Убыв":
                        Services = new ObservableCollection<ServicesVM>(Services.OrderBy(x=> x.Service.Cost));
                        OnPropertyChanged("Services");
                        break;
                    case "Возр":
                        Services = new ObservableCollection<ServicesVM>(Services.OrderByDescending(x=> x.Service.Cost));
                        OnPropertyChanged("Services");
                        break;
                }
                _selectedCost = value;
                OnPropertyChanged("SelectedCost");
            }
        }

        public RoutedCommand FilterClearCommand { get; private set; }
        public CommandBinding FilterClearCommandBinding { get; set; }

        public void FilterClear(object sender, ExecutedRoutedEventArgs e)
        {
            SelectedCost = "none";
            SelectedDiscount = "none";
        }

        private string _filterName;
        public string FilterName
        {
            get { return _filterName; }
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                {
                    Services = UnchangedServices;
                }
                else
                {
                    Services = new ObservableCollection<ServicesVM>(Services.Where(x => x.Service.Title.Contains(value)));
                    OnPropertyChanged("Services");
                }
                _filterName = value;
                OnPropertyChanged("FilterName");
            }
        }
        private string _filterDescr;
        public string FilterDesct
        {
            get { return _filterDescr; }
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                {
                    Services = UnchangedServices;
                }
                else
                {
                    Services = new ObservableCollection<ServicesVM>(Services.Where(x => x.Service.Description.Contains(value)));
                    OnPropertyChanged("Services");
                }
                _filterDescr = value;
                OnPropertyChanged("FilterName");
            }
        }

        #region AddUser

        public RoutedCommand ServiceClientAddCommand { get; private set; }
        public CommandBinding ServiceClientAddCommandBinding { get; set; }

        public User CurrentUser { get; set; }

        public ObservableCollection<ServiceClient> ServiceClients { get; set; } = new ObservableCollection<ServiceClient>();
        public ObservableCollection<User> Users { get; set; } = new ObservableCollection<User>();

        public void ServiceClientAdd(object sender, ExecutedRoutedEventArgs e)
        {
            ServiceClient serviceClient = new ServiceClient
            {
                Service = CurrentService.Service.Id,
                Service1 = CurrentService.Service,
                User = CurrentUser
            };

            ServiceClients.Add(serviceClient);
            _entities.ServiceClient.Add(serviceClient);
            _entities.SaveChanges();
        }

        #endregion

        #endregion
    }

    public class ServicesVM : INotifyPropertyChanged, IEditableObject
    {
        private ServicesVM _tempValue;
        public Service Service { get; set; }
        public bool IsAdmin { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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
            set
            {
                _mainImage = value;
                OnPropertyChanged("MainImage");
            }
        }

        public ServicesVM(Service service, bool isAdmin, Action<object, ExecutedRoutedEventArgs> executeServiceCommand, Action<object, CanExecuteRoutedEventArgs> canExecute)
        {
            Service = service;
            IsAdmin = isAdmin;

            DeleteCommand = new RoutedCommand("Delete", this.GetType());
            ChangeCommand = new RoutedCommand("Change", this.GetType());

            AddUserCommand = new RoutedCommand("AddUser", this.GetType());

            ServiceDeleteCommandBinding = new CommandBinding(DeleteCommand, ChangeCurrentItemLbServices, canExecute.Invoke);
            ServiceChangeCommandBinding = new CommandBinding(ChangeCommand, ChangeCurrentItemLbServices, canExecute.Invoke);
            AddUserCommandBinding = new CommandBinding(AddUserCommand, ChangeCurrentItemLbServices, canExecute.Invoke);

            ServiceChangeCommandBinding.Executed += executeServiceCommand.Invoke;
            ServiceDeleteCommandBinding.Executed += executeServiceCommand.Invoke;
            AddUserCommandBinding.Executed += executeServiceCommand.Invoke;
        }
        public ServicesVM()
        {

        }

        private void ChangeCurrentItemLbServices(object sender, ExecutedRoutedEventArgs e)
        {
            Type t = sender.GetType();
            System.Reflection.FieldInfo fInfo = t.GetField("lbServices");
            object lb = fInfo.GetValue(sender);
            if(lb != null)
            {
                bool isCur = (lb as ListBox).Items.MoveCurrentTo(this);
            }
        }

        public void BeginEdit()
        {
            _tempValue = new ServicesVM
            {
                Service = new Service
                {
                    Cost = this.Service.Cost,
                    Description = this.Service.Description,
                    Discount = this.Service.Discount,
                    Duration = this.Service.Duration,  
                    Id = this.Service.Id,
                    MainImagePath = this.Service.MainImagePath,
                    ServiceClient = this.Service.ServiceClient,
                    Title = this.Service.Title
                },
                IsAdmin = this.IsAdmin,
                DeleteCommand = this.DeleteCommand,
                ChangeCommand = this.ChangeCommand,
                ServiceChangeCommandBinding = this.ServiceChangeCommandBinding,
                ServiceDeleteCommandBinding = this.ServiceDeleteCommandBinding
            };
        }

        public void EndEdit()
        {
            _tempValue = null;
        }

        public void CancelEdit()
        {
            if (_tempValue == null) return;

            Service = _tempValue.Service;
            IsAdmin = _tempValue.IsAdmin;
            DeleteCommand = _tempValue.DeleteCommand;
            ChangeCommand = _tempValue.ChangeCommand;
            ServiceChangeCommandBinding = _tempValue.ServiceChangeCommandBinding;
            ServiceDeleteCommandBinding = _tempValue.ServiceDeleteCommandBinding;
        }

        public RoutedCommand DeleteCommand { get; private set; }
        public RoutedCommand ChangeCommand { get; private set; }

        public CommandBinding ServiceDeleteCommandBinding { get; private set; }
        public CommandBinding ServiceChangeCommandBinding { get; private set; }

        public RoutedCommand AddUserCommand { get; private set; }
        public CommandBinding AddUserCommandBinding { get; set; }
    }

    public class ServiceClientsVM
    {

    }
}
