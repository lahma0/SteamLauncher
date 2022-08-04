using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace SteamLauncher.UI.Converters
{
    public class PropertyInfoOfExtension : MarkupExtension
    {
        private readonly PropertyPath _propertyPath;

        public PropertyInfoOfExtension(Binding binding)
        {
            _propertyPath = binding.Path;
        }
        
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var indexOfLastVariableName = _propertyPath.Path.LastIndexOf('.');
            var propertyName = _propertyPath.Path.Substring(indexOfLastVariableName + 1);
            var success = TryGetTargetItems(serviceProvider,
                                            out DependencyObject targetObject,
                                            out DependencyProperty targetProperty);
            if (!success)
                return null;

            var targetElement = targetObject as FrameworkElement;
            

            return targetElement?.DataContext?.GetType().GetProperty(propertyName);
        }

        protected virtual bool TryGetTargetItems(IServiceProvider provider, out DependencyObject targetObject, out DependencyProperty targetProperty)
        {
            targetObject = null;
            targetProperty = null;

            var service = (IProvideValueTarget) provider?.GetService(typeof(IProvideValueTarget));
            if (service == null)
                return false;

            targetObject = service.TargetObject as DependencyObject;
            targetProperty = service.TargetProperty as DependencyProperty;
            return targetProperty != null && targetObject != null;
        }
    }
}
