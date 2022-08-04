using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace SteamLauncher.UI.Utils
{
    public static class MouseObserver
    {
        public static readonly DependencyProperty ObserveProperty = DependencyProperty.RegisterAttached(
            "Observe",
            typeof(bool),
            typeof(MouseObserver),
            new FrameworkPropertyMetadata(OnObserveChanged));

        public static readonly DependencyProperty ObservedMouseOverProperty = DependencyProperty.RegisterAttached(
            "ObservedMouseOver",
            typeof(bool),
            typeof(MouseObserver));

        public static readonly DependencyProperty PropertyNameProperty = DependencyProperty.RegisterAttached(
            "PropertyName", 
            typeof(string), 
            typeof(MouseObserver));

        public static readonly DependencyProperty PropertyNameBindingProperty = DependencyProperty.RegisterAttached(
            "PropertyNameBinding",
            typeof(string),
            typeof(MouseObserver));

        public static readonly DependencyProperty PropertyInfoProperty = DependencyProperty.RegisterAttached(
            "PropertyInfo",
            typeof(PropertyInfo),
            typeof(MouseObserver));

        public static readonly DependencyProperty PropertyInfoBindingProperty = DependencyProperty.RegisterAttached(
            "PropertyInfoBinding",
            typeof(PropertyInfo),
            typeof(MouseObserver));

        public static bool GetObserve(FrameworkElement frameworkElement)
        {
            return (bool)frameworkElement.GetValue(ObserveProperty);
        }

        public static void SetObserve(FrameworkElement frameworkElement, bool observe)
        {
            frameworkElement.SetValue(ObserveProperty, observe);
        }

        public static bool GetObservedMouseOver(FrameworkElement frameworkElement)
        {
            return (bool)frameworkElement.GetValue(ObservedMouseOverProperty);
        }

        public static void SetObservedMouseOver(FrameworkElement frameworkElement, bool observedMouseOver)
        {
            frameworkElement.SetValue(ObservedMouseOverProperty, observedMouseOver);
        }

        public static string GetPropertyName(FrameworkElement frameworkElement)
        {
            return (string)frameworkElement.GetValue(PropertyNameProperty);
        }

        public static void SetPropertyName(FrameworkElement frameworkElement, string propertyName)
        {
            frameworkElement.SetValue(PropertyNameProperty, propertyName);
        }

        public static string GetPropertyNameBinding(FrameworkElement frameworkElement)
        {
            return (string)frameworkElement.GetValue(PropertyNameBindingProperty);
        }

        public static void SetPropertyNameBinding(FrameworkElement frameworkElement, string propertyNameBinding)
        {
            frameworkElement.SetValue(PropertyNameBindingProperty, propertyNameBinding);
        }

        public static PropertyInfo GetPropertyInfo(FrameworkElement frameworkElement)
        {
            return (PropertyInfo)frameworkElement.GetValue(PropertyInfoProperty);
        }

        public static void SetPropertyInfo(FrameworkElement frameworkElement, PropertyInfo propertyInfo)
        {
            frameworkElement.SetValue(PropertyInfoProperty, propertyInfo);
        }

        public static PropertyInfo GetPropertyInfoBinding(FrameworkElement frameworkElement)
        {
            return (PropertyInfo)frameworkElement.GetValue(PropertyInfoBindingProperty);
        }

        public static void SetPropertyInfoBinding(FrameworkElement frameworkElement, PropertyInfo propertyInfoBinding)
        {
            frameworkElement.SetValue(PropertyInfoBindingProperty, propertyInfoBinding);
        }

        private static void OnObserveChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var frameworkElement = (FrameworkElement)dependencyObject;
            if ((bool)e.NewValue)
            {
                frameworkElement.MouseEnter += OnFrameworkElementMouseOverChanged;
                frameworkElement.MouseLeave += OnFrameworkElementMouseOverChanged;
                UpdateObservedMouseOverForFrameworkElement(frameworkElement);
            }
            else
            {
                frameworkElement.MouseEnter -= OnFrameworkElementMouseOverChanged;
                frameworkElement.MouseLeave -= OnFrameworkElementMouseOverChanged;
            }
        }

        private static void OnFrameworkElementMouseOverChanged(object sender, MouseEventArgs e)
        {
            UpdateObservedMouseOverForFrameworkElement((FrameworkElement)sender);
        }

        private static void UpdateObservedMouseOverForFrameworkElement(FrameworkElement frameworkElement)
        {
            frameworkElement.SetCurrentValue(ObservedMouseOverProperty, frameworkElement.IsMouseOver);
            if (PropertyNameBindingProperty != null)
                frameworkElement.SetCurrentValue(PropertyNameBindingProperty,
                                                 frameworkElement.IsMouseOver
                                                     ? frameworkElement.GetValue(PropertyNameProperty) : null);

            if (PropertyInfoBindingProperty != null)
                frameworkElement.SetCurrentValue(PropertyInfoBindingProperty,
                                                 frameworkElement.IsMouseOver
                                                     ? frameworkElement.GetValue(PropertyInfoProperty) : null);
        }
    }
}
