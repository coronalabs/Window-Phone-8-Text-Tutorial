using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using TextTutorial.Resources;


namespace TextTutorial
{
    public partial class MainPage : PhoneApplicationPage
    {
        /// <summary>Reference to the Corona runtime environment that is currently running.</summary>
        private CoronaLabs.Corona.WinRT.CoronaRuntimeEnvironment fCoronaRuntimeEnvironment = null;
        private InputForm control;

        public MainPage()
        {
            // Initialize this page's components that were set up via the UI designer.
            InitializeComponent();

            // Set up Corona to automatically start up when the control's Loaded event has been raised.
            // Note: By default, Corona will run the "main.lua" file in the "Assets\Corona" directory.
            //       You can change the defaults via the CoronaPanel's AutoLaunchSettings property.
            fCoronaPanel.AutoLaunchEnabled = true;

            // Set up the CoronaPanel control to render fullscreen via the DrawingSurfaceBackgroundGrid control.
            // This significantly improves the framerate and is the only means of achieving 60 FPS.
            fCoronaPanel.BackgroundRenderingEnabled = true;
            fDrawingSurfaceBackgroundGrid.SetBackgroundContentProvider(fCoronaPanel.BackgroundContentProvider);
            fDrawingSurfaceBackgroundGrid.SetBackgroundManipulationHandler(fCoronaPanel.BackgroundManipulationHandler);

            // Add event handlers to the Corona runtime.
            fCoronaPanel.Runtime.Loaded += OnCoronaRuntimeLoaded;
            fCoronaPanel.Runtime.Terminating += OnCoronaRuntimeExiting;

            // Add application event handlers.
            App.RootFrame.Obscured += OnAppObscured;
            App.RootFrame.Unobscured += OnAppUnobscured;
        }

        /// <summary>
        ///  Called when a new CoronaRuntimeEnvironment has been created/loaded,
        ///  but before the "main.lua" has been executed.
        /// </summary>
        /// <param name="sender">The CoronaRuntime object that raised this event.</param>
        /// <param name="e">Event arguments providing the CoronaRuntimeEnvironment that has been created/loaded.</param>
        private void OnCoronaRuntimeLoaded(
            object sender, CoronaLabs.Corona.WinRT.CoronaRuntimeEventArgs e)
        {
            // Keep a reference to the Corona runtime environment.
            // It's needed so that your login window's results can be dispatched to Corona.
            fCoronaRuntimeEnvironment = e.CoronaRuntimeEnvironment;
            fCoronaRuntimeEnvironment.AddEventListener("requestingLogin", OnRequestingLogin);
        }

        /// <summary>
        ///  Called when the CoronaRuntimeEnvironment is terminating
        ///  during a Lua "applicationExit" system event.
        /// </summary>
        /// <param name="sender">The CoronaRuntime object that raised this event.</param>
        /// <param name="e">Event arguments providing the CoronaRuntimeEnvironment that is being terminated.</param>
        private void OnCoronaRuntimeExiting(
            object sender, CoronaLabs.Corona.WinRT.CoronaRuntimeEventArgs e)
        {
            // Give up our reference to the Corona runtime since it is not running anymore.
            // This also allows it to be garbage collected.
            fCoronaRuntimeEnvironment = null;
        }

        /// <summary>Called by Lua when it is requesting a login popup to be shown.</summary>
        /// <param name="sender">The CoronaRuntimeEnvironment that dispatched this event.</param>
        /// <param name="e">Provides the Lua vent table's fields/properties.</param>
        /// <returns>Returns a boxed object to Lua.</returns>
        private CoronaLabs.Corona.WinRT.ICoronaBoxedData OnRequestingLogin(
            CoronaLabs.Corona.WinRT.CoronaRuntimeEnvironment sender,
            CoronaLabs.Corona.WinRT.CoronaLuaEventArgs e)
        {
            // Fetch the "event.loginName" property, if provided.
            string username = string.Empty;

            var boxedUsername = e.Properties.Get("username") as CoronaLabs.Corona.WinRT.CoronaBoxedString;
            if (boxedUsername != null)
            {
                username = boxedUsername.ToString();
            }
            Console.WriteLine("Got data from Lua... ");
            Console.WriteLine(username);
            // Display your login popup here using the "loginName" fetched up above.
            control = new InputForm( username );
            control.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            control.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            control.OnCompleteCallback = new OnCompleteDelegate(this.OnInputFormClosed);
            fCoronaPanel.Children.Add(control);
            control.Username.Text = username;

            // This returns nil to Lua.
            return null;
        }

        /// <summary>*** You should call this method when the login popup closes.***</summary>
        public void OnInputFormClosed(object sender, EventArgs e, string username, string password)
        {
            // Do not continue if the Corona runtime is no longer running.
            if (fCoronaRuntimeEnvironment == null)
            {
                return;
            }

            // Create a custom Corona event named "userLoggedIn" with the following properties.
            // This will be converted into a Lua "event" table once dispatched by Corona.
            var eventProperties = CoronaLabs.Corona.WinRT.CoronaLuaEventProperties.CreateWithName("onLoginInfo");
            eventProperties.Set("username", username);
            eventProperties.Set("password", password);

            // Dispatch the event to Lua.
            Console.WriteLine("closing form, sending data back to lua\n");
            var eventArgs = new CoronaLabs.Corona.WinRT.CoronaLuaEventArgs(eventProperties);
            fCoronaRuntimeEnvironment.DispatchEvent(eventArgs);
            fCoronaPanel.Children.Remove(control);
        }
        private void OnAppObscured(object sender, ObscuredEventArgs e)
        {
            if (e.IsLocked == false)
            {
                fCoronaPanel.Runtime.Suspend();
            }
        }

        private void OnAppUnobscured(object sender, EventArgs e)
        {
            fCoronaPanel.Runtime.Resume();
        }
    }
}
