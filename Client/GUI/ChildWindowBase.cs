using mdilib;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;


namespace Eudora.Net.GUI
{
    /// <summary>
    /// sbMDI is designed such that for the most part, the MdiChild window class
    /// is not interacted with directly by the application. Instead, ChildWindowBase
    /// is the basis class for interacting with UX/window content.
    /// </summary>
    public class ChildWindowBase : UserControl, INotifyPropertyChanged
    {
        ///////////////////////////////////////////////////////////
        #region INotifyPropertyChanged
        /////////////////////////////

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void SetField<TField>(ref TField field, TField value, string propertyName)
        {
            if (EqualityComparer<TField>.Default.Equals(field, value))
            {
                return;
            }

            field = value;
            OnPropertyChanged(propertyName);
        }

        /////////////////////////////
        #endregion INotifyPropertyChanged
        ///////////////////////////////////////////////////////////



        ///////////////////////////////////////////////////////////
        #region Dependency Properties
        /////////////////////////////

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(ChildWindowBase), 
                new PropertyMetadata(string.Empty, OnTitlePropertyChanged));

        private static void OnTitlePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is ChildWindowBase window)
            {
                window.Window.Title = window.Title;
            }
        }

        /////////////////////////////
        #endregion Dependency Properties
        ///////////////////////////////////////////////////////////



        ///////////////////////////////////////////////////////////
        #region Properties
        /////////////////////////////

        private MdiChild _Window;
        public MdiChild Window
        {
            get => _Window;
            set => _Window = value;
        }


        /////////////////////////////
        #endregion Properties
        ///////////////////////////////////////////////////////////


        public ChildWindowBase()
        {
            // Design-time workaround
            if (MainWindow.Instance is null)
            {
                _Window = new MdiChild();
            }
            // Actual application runtime path
            else
            {
                _Window = MainWindow.Instance.MDI.NewChildWindow(this, Title);
                MainWindow.Instance.FocusWindow(_Window);
                //Binding bindTitle = new()
                //{
                //    Source = TitleProperty
                //};
                //_Window.SetBinding(TitleProperty, bindTitle);

                //PropertyChanged += Uc_ViewBase_ProagateTitle;
            }
        }

        //private void Uc_ViewBase_ProagateTitle(object? sender, PropertyChangedEventArgs e)
        //{
        //    if(e.PropertyName == nameof(Title) && Window is not null)
        //    {
        //        Window.Title = Title;
        //    }
        //}

        public void Close()
        {
            if(Parent is MdiChild window)
            {
                window.Close();
            }
        }

        public virtual void MdiActivated()
        {
            ActivateSubviews(this);
        }

        public virtual void MdiDeactivated()
        {
            DeactivateSubviews(this);
        }

        private void ActivateSubviews(DependencyObject current)
        {
            foreach (var child in LogicalTreeHelper.GetChildren(current))
            {
                if (child is SubviewBase subview)
                {
                    subview.Activate();
                }
                if (child is DependencyObject dobj)
                {
                    ActivateSubviews(dobj);
                }
            }
        }

        private void DeactivateSubviews(DependencyObject current)
        {
            foreach (var child in LogicalTreeHelper.GetChildren(current))
            {
                if (child is SubviewBase subview)
                {
                    subview.Deactivate();
                }
                if (child is DependencyObject dobj)
                {
                    DeactivateSubviews(dobj);
                }
            }
        }

        public void BlockUX(bool block)
        {

        }
    }
}
