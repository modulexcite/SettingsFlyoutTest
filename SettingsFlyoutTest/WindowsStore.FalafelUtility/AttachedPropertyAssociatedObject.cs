using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace WindowsStore.FalafelUtility
{
    /// <summary>
    /// Use this class as an attached property that can handle value changed events and maintains the 
    /// associated object that is attached to.
    /// </summary>
    /// <typeparam name="O">The type of the owner class</typeparam>
    /// <typeparam name="T">Type of the associated object that can be attached to</typeparam>
    /// <typeparam name="A">Type of the attached property that is attached to the associated object</typeparam>
    public abstract class AttachedPropertyAssociatedObject<O, T, A> : DependencyObject
        where T : DependencyObject
        where O : AttachedPropertyAssociatedObject<O, T, A>, new()
    {
        static Type _type = typeof(O);

        public T AssociatedObject { get; private set; }

        public AttachedPropertyAssociatedObject()
            : base()
        { }

        public AttachedPropertyAssociatedObject(T associatedObject)
            : this()
        {
            AssociatedObject = associatedObject;
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.RegisterAttached(
                "Value",
                typeof(A),
                typeof(AttachedPropertyAssociatedObject<O, T, A>),
                new PropertyMetadata(null, new PropertyChangedCallback(OnValueChanged)));

        public static A GetValue(DependencyObject d)
        {
            return (A)d.GetValue(ValueProperty);
        }

        public static void SetValue(DependencyObject d, A value)
        {
            d.SetValue(ValueProperty, value);
        }

        public A Value 
        {
            get
            {
                return GetValue(AssociatedObject);
            }
            set
            {
                SetValue(AssociatedObject, value);
            }
        }

        public static readonly DependencyProperty ValueChangedProperty =
            DependencyProperty.RegisterAttached(
                "ValueChanged",
                typeof(PropertyChangedCallback),
                typeof(AttachedPropertyAssociatedObject<O, T, A>),
                null);

        public static PropertyChangedCallback GetValueChanged(DependencyObject d)
        {
            return (PropertyChangedCallback)d.GetValue(ValueChangedProperty);
        }

        public static void SetValueChanged(DependencyObject d, PropertyChangedCallback value)
        {
            d.SetValue(ValueChangedProperty, value);
        }

        public static O GetInstance(DependencyObject d)
        {
            return GenericAttachedProperty<O>.GetValue(d);
        }

        public static void SetInstance(DependencyObject d, O value)
        {
            GenericAttachedProperty<O>.SetValue(d, value);
        }

        public static void InitializeInstance(T associatedObject)
        {
            O attachedPropertyAssociatedObject = GetInstance(associatedObject);
            if (attachedPropertyAssociatedObject == null)
            {
                attachedPropertyAssociatedObject = new O();
                attachedPropertyAssociatedObject.AssociatedObject = associatedObject;

                SetInstance(associatedObject, attachedPropertyAssociatedObject);
                attachedPropertyAssociatedObject.Initialize();
            }
        }

        protected static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            T associatedObject = d as T;
            if (associatedObject == null)
            {
                throw new Exception(String.Format("DependencyObject must be of type {0}", typeof(T)));
            }

            O attachedPropertyAssociatedObject = GetInstance(d);
            if (e.NewValue != null)
            {
                if (attachedPropertyAssociatedObject == null)
                {
                    attachedPropertyAssociatedObject = new O();
                    attachedPropertyAssociatedObject.AssociatedObject = associatedObject;

                    SetInstance(d, attachedPropertyAssociatedObject);
                    attachedPropertyAssociatedObject.Initialize();
                }

                PropertyChangedCallback callback = GetValueChanged(d);
                if (callback != null)
                {
                    callback(associatedObject, e);
                }
            }
            else
            {
                if (attachedPropertyAssociatedObject != null)
                {
                    attachedPropertyAssociatedObject.UnInitialize();
                    SetInstance(d, null);
                }
            }
        }

        public virtual void Initialize()
        {
        }

        public virtual void UnInitialize()
        {
        }
    }

}
