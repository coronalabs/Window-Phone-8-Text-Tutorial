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
            var inputForm = new InputForm();
			inputForm.Username = username;
			inputForm.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
			inputForm.VerticalAlignment = System.Windows.VerticalAlignment.Center;
			inputForm.Submitted += OnFormInputSubmitted;
			inputForm.Canceled += OnFormInputCanceled;
			fCoronaPanel.Children.Add(inputForm);

            // This returns nil to Lua.
            return null;
        }

		/// <summary>Called by the InputForm when the OK button has been clicked on.</summary>
		/// <param name="sender">Reference to the InputForm that raised this event.</param>
		/// <param name="e">Empty event arguments.</param>
		private void OnFormInputSubmitted(object sender, EventArgs e)
		{
			HandleFormInput(sender as InputForm, true);
		}

		/// <summary>Called by the InputForm when the Cancel button has been clicked on.</summary>
		/// <param name="sender">Reference to the InputForm that raised this event.</param>
		/// <param name="e">Empty event arguments.</param>
		private void OnFormInputCanceled(object sender, EventArgs e)
		{
			HandleFormInput(sender as InputForm, false);
		}

		/// <summary>Dispatches InputForm data received by after a "Submitted" or "Canceled" event.</summary>
		/// <param name="inputForm">Reference to the input form.</param>
		/// <param name="wasSubmitted">Set true if the OK button was clicked. Set false if canceled.</param>
		private void HandleFormInput(InputForm inputForm, bool wasSubmitted)
		{
			// Validate argument.
			if (inputForm == null)
			{
				return;
			}

			// Close the input form and remove our event handlers from it.
			// This will remove all references to this form, allowing it to be garbage collected.
			fCoronaPanel.Children.Remove(inputForm);
			inputForm.Submitted -= OnFormInputSubmitted;
			inputForm.Canceled -= OnFormInputCanceled;

			// Dispatch an event to Corona about the received input form data.
			if (fCoronaRuntimeEnvironment != null)
			{
				// Create a custom Corona event named "userLoggedIn" with the following properties.
				// This will be converted into a Lua "event" table once dispatched by Corona.
				var eventProperties = CoronaLabs.Corona.WinRT.CoronaLuaEventProperties.CreateWithName("onLoginInfo");
				eventProperties.Set("submitted", wasSubmitted);
				if (wasSubmitted)
				{
					eventProperties.Set("username", inputForm.Username);
					eventProperties.Set("password", inputForm.Password);
				}

				// Dispatch the event to Lua.
				var eventArgs = new CoronaLabs.Corona.WinRT.CoronaLuaEventArgs(eventProperties);
				fCoronaRuntimeEnvironment.DispatchEvent(eventArgs);
			}
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
