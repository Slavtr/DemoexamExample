using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.ComponentModel;

namespace ViewModel
{
    public class ViewModel1 : INotifyPropertyChanged
    {
        /// <summary>
        /// Главный фрейм
        /// </summary>
        public Frame MainFrame { get; set; }
        /// <summary>
        /// Список страниц
        /// </summary>
        public List<Page> Pages { get; set; }
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
            MainFrame.Content = Pages.Where(x => x.Name.Contains("ServiceList"));
        }

    }
}
