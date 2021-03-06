﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Valour.Client.Channels;
using Valour.Client.Messages;
using Valour.Client.Planets;
using Valour.Client.Shared;
using Valour.Shared.Channels;

namespace Valour.Client
{
    /// <summary>
    /// This tracks and manages the open windows for the client
    /// </summary>
    public class ClientWindowManager
    {
        private List<ClientWindow> OpenWindows = new List<ClientWindow>();

        private ClientWindow SelectedWindow;

        public Func<Task> OnWindowSelect;

        public static ClientWindowManager Instance;

        public ClientWindowManager()
        {
            Instance = this;
        }

        public async Task SetSelectedWindow(int index)
        {
            await SetSelectedWindow(OpenWindows[index]);
        }

        public async Task SetSelectedWindow(ClientWindow window)
        {
            if (window == null || (SelectedWindow == window)) return;

            SelectedWindow = window;

            Console.WriteLine($"Set active window to {window.Index}");

            if (OnWindowSelect != null)
            {
                Console.WriteLine($"Invoking window change event");
                await OnWindowSelect?.Invoke();
            }
        }

        public ClientWindow GetSelectedWindow()
        {
            return SelectedWindow;
        }

        public void AddWindow(ClientWindow window)
        {
            window.Index = OpenWindows.Count;
            OpenWindows.Add(window);
        }

        public int GetWindowCount()
        {
            return OpenWindows.Count;
        }

        public ClientWindow GetWindow(int index)
        {
            if (index > OpenWindows.Count - 1)
            {
                return null;
            }

            return OpenWindows[index];
        }

        public IEnumerable<ClientWindow> GetWindows()
        {
            return OpenWindows;
        }

        public void ClearWindows()
        {
            foreach (ClientWindow window in OpenWindows)
            {
                window.OnClosed();
            }

            OpenWindows.Clear();
        }

        public void SetWindow(int index, ClientWindow window)
        {
            if (OpenWindows[index] == window)
            {
                return;
            }

            window.Index = index;
            OpenWindows[index].OnClosed();
            OpenWindows.RemoveAt(index);
            OpenWindows.Insert(index, window);
        }
    }

    public class ClientWindow
    {
        /// <summary>
        /// The index of this window
        /// </summary>
        public int Index { get; set; }

        public ClientWindow(int index)
        {
            this.Index = index;
        }

        public virtual void OnClosed()
        {

        }
    }

    public class HomeWindow : ClientWindow
    {
        public HomeWindow(int index) : base(index)
        {
            
        }
    }

    public class ChatChannelWindow : ClientWindow
    {
        /// <summary>
        /// The channel this window represents
        /// </summary>
        public ClientPlanetChatChannel Channel { get; set; }

        /// <summary>
        /// The component that belongs to this window
        /// </summary>
        public ChannelWindowComponent Component { get; set; }

        /// <summary>
        /// Storage for messages that should be displayed
        /// </summary>
        public List<ClientPlanetMessage> messages;

        /// <summary>
        /// The index of the last recieved message
        /// </summary>
        public ulong messageIndex;

        /// <summary>
        /// The index of the earliest recieved message
        /// </summary>
        public ulong firstMessageIndex;

        public ChatChannelWindow(int index, ClientPlanetChatChannel channel) : base(index)
        {
            this.Channel = channel;
        }

        public override void OnClosed()
        {
            base.OnClosed();

            Task.Run(async () =>
            {
                await Component.OnWindowClosed();
            });
        }
    }
}
