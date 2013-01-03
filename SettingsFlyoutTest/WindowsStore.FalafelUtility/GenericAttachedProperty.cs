using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace WindowsStore.FalafelUtility
{
    public class GenericAttachedProperty<A>
    {
        public static readonly DependencyProperty ValueProperty =
                DependencyProperty.RegisterAttached(
                "Value",
                typeof(A),
                typeof(GenericAttachedProperty<A>),
                null);

        public static A GetValue(DependencyObject d)
        {
            return (A)d.GetValue(ValueProperty);
        }

        public static void SetValue(DependencyObject d, A value)
        {
            d.SetValue(ValueProperty, value);
        }
    }

    public abstract class GenericAttachedPropertyValueChanged<O, A>
    {
        static Type _type = typeof(O);

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.RegisterAttached(
            "Value",
            typeof(A),
            typeof(GenericAttachedPropertyValueChanged<O, A>),
            new PropertyMetadata(null, new PropertyChangedCallback(OnValueChanged)));

        public static A GetValue(DependencyObject d)
        {
            return (A)d.GetValue(ValueProperty);
        }

        public static void SetValue(DependencyObject d, A value)
        {
            d.SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty ValueChangedProperty =
            DependencyProperty.RegisterAttached(
            "ValueChanged",
            typeof(PropertyChangedCallback),
            typeof(GenericAttachedPropertyValueChanged<O, A>),
            null);

        public static PropertyChangedCallback GetValueChanged(DependencyObject d)
        {
            return (PropertyChangedCallback)d.GetValue(ValueChangedProperty);
        }

        public static void SetValueChanged(DependencyObject d, PropertyChangedCallback value)
        {
            d.SetValue(ValueChangedProperty, value);
        }

        public static void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var vc = GetValueChanged(sender);
            if (vc != null)
            {
                vc(sender, e);
            }
        }
    }

}
