using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Animation;
using WindowsStore.FalafelUtility;

namespace WindowsStore.FalafelUtility
{
    public class SettingsFlyoutInfo
    {
        public double SettingsWidth { get; set; }
        public string SettingsID { get; set; }
        public string SettingsTitle { get; set; }
        public Type SettingsFlyoutType { get; set; }
    }

    public class SettingsFlyoutInfoCollection
    {
        public SettingsFlyoutInfoCollection()
        {
            Infos = new Dictionary<string, SettingsFlyoutInfo>();
        }

        public event EventHandler<bool> PopupChanged;
        public Dictionary<string, SettingsFlyoutInfo> Infos { get; set; }

        internal void RaisePopupChanged(bool value)
        {
            if (this.PopupChanged != null)
            {
                this.PopupChanged(this, value);
            }
        }

        public void AddSettingsFlyoutInfo(Type type, string settingsID, string title, double width)
        {
            Infos.Add(settingsID, new SettingsFlyoutInfo()
            {
                SettingsFlyoutType = type,
                SettingsID = settingsID,
                SettingsTitle = title,
                SettingsWidth = width,
            });
        }
    }

    public class SettingsFlyoutAttachedProperty : WindowsStore.FalafelUtility.AttachedPropertyAssociatedObject<SettingsFlyoutAttachedProperty, Page, SettingsFlyoutInfoCollection>
    {
        // Used to determine the correct height to ensure our custom UI fills the screen.
        private Rect windowBounds;

        // This is the container that will hold our custom content.
        private Popup settingsPopup;

        public override void Initialize()
        {
            windowBounds = Window.Current.Bounds;
            Window.Current.SizeChanged += Current_SizeChanged;

            // Listening for this event lets the app initialize the settings commands and pause its UI until the user closes the pane.
            // To ensure your settings are available at all times in your app, place your CommandsRequested handler in the overridden
            // OnWindowCreated of App.xaml.cs
            SettingsPane.GetForCurrentView().CommandsRequested += onCommandsRequested;
        }

        public override void UnInitialize()
        {
            // Added to make sure the event handler for CommandsRequested is cleaned up before other scenarios.
            SettingsPane.GetForCurrentView().CommandsRequested -= onCommandsRequested;

            Window.Current.SizeChanged -= Current_SizeChanged;
        }

        void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            windowBounds = Window.Current.Bounds;
        }

        /// <summary>
        /// This the event handler for the "Defaults" button added to the settings charm. This method
        /// is responsible for creating the Popup window will use as the container for our settings Flyout.
        /// The reason we use a Popup is that it gives us the "light dismiss" behavior that when a user clicks away 
        /// from our custom UI it just dismisses.  This is a principle in the Settings experience and you see the
        /// same behavior in other experiences like AppBar. 
        /// </summary>
        /// <param name="command"></param>
        void onSettingsCommand(IUICommand command)
        {
            SettingsCommand scmd = command as SettingsCommand;
            var item = this.Value.Infos[scmd.Id.ToString()];

            // Create a Popup window which will contain our flyout.
            settingsPopup = new Popup();
            settingsPopup.Closed += OnPopupClosed;
            Window.Current.Activated += OnWindowActivated;
            settingsPopup.IsLightDismissEnabled = true;
            settingsPopup.Width = item.SettingsWidth;
            settingsPopup.Height = windowBounds.Height;

            // Add the proper animation for the panel.
            settingsPopup.ChildTransitions = new TransitionCollection();
            settingsPopup.ChildTransitions.Add(new PaneThemeTransition()
            {
                Edge = (SettingsPane.Edge == SettingsEdgeLocation.Right) ?
                        EdgeTransitionLocation.Right :
                        EdgeTransitionLocation.Left
            });

            // Create a SettingsFlyout the same dimenssions as the Popup.
            LayoutAwarePage mypane = Activator.CreateInstance(item.SettingsFlyoutType) as LayoutAwarePage;
            mypane.Width = item.SettingsWidth;
            mypane.Height = windowBounds.Height;

            // Place the SettingsFlyout inside our Popup window.
            settingsPopup.Child = mypane;

            // Let's define the location of our Popup.
            settingsPopup.SetValue(Canvas.LeftProperty, SettingsPane.Edge == SettingsEdgeLocation.Right ? (windowBounds.Width - item.SettingsWidth) : 0);
            settingsPopup.SetValue(Canvas.TopProperty, 0);
            settingsPopup.IsOpen = true;
            this.Value.RaisePopupChanged(true);
        }

        /// <summary>
        /// This event is generated when the user opens the settings pane. During this event, append your
        /// SettingsCommand objects to the available ApplicationCommands vector to make them available to the
        /// SettingsPange UI.
        /// </summary>
        /// <param name="settingsPane">Instance that triggered the event.</param>
        /// <param name="eventArgs">Event data describing the conditions that led to the event.</param>
        void onCommandsRequested(SettingsPane settingsPane, SettingsPaneCommandsRequestedEventArgs eventArgs)
        {
            foreach (var item in this.Value.Infos.Values)
            {
                UICommandInvokedHandler handler = new UICommandInvokedHandler(onSettingsCommand);

                SettingsCommand generalCommand = new SettingsCommand(item.SettingsID, item.SettingsTitle, handler);
                eventArgs.Request.ApplicationCommands.Add(generalCommand);
            }
        }

        /// <summary>
        /// We use the window's activated event to force closing the Popup since a user maybe interacted with
        /// something that didn't normally trigger an obvious dismiss.
        /// </summary>
        /// <param name="sender">Instance that triggered the event.</param>
        /// <param name="e">Event data describing the conditions that led to the event.</param>
        private void OnWindowActivated(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.Deactivated)
            {
                settingsPopup.IsOpen = false;
            }
        }

        /// <summary>
        /// When the Popup closes we no longer need to monitor activation changes.
        /// </summary>
        /// <param name="sender">Instance that triggered the event.</param>
        /// <param name="e">Event data describing the conditions that led to the event.</param>
        void OnPopupClosed(object sender, object e)
        {
            Window.Current.Activated -= OnWindowActivated;
            this.Value.RaisePopupChanged(false);
        }
    }
}
