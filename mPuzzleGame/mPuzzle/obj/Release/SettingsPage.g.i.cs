﻿#pragma checksum "E:\projects\win-phone\mPuzzle-latest\mPuzzle\SettingsPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "85448E23878FD97AB9F6DB9AECB394C2"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.17929
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Phone.Controls;
using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;
using mPuzzle;


namespace mPuzzle {
    
    
    public partial class SettingsPage : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Canvas LayoutRoot;
        
        internal System.Windows.Controls.TextBlock username;
        
        internal System.Windows.Controls.Button buttonChangeUsername;
        
        internal System.Windows.Controls.Image header;
        
        internal mPuzzle.NicknameDialog nicknameDialog;
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Windows.Application.LoadComponent(this, new System.Uri("/mPuzzle;component/SettingsPage.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Canvas)(this.FindName("LayoutRoot")));
            this.username = ((System.Windows.Controls.TextBlock)(this.FindName("username")));
            this.buttonChangeUsername = ((System.Windows.Controls.Button)(this.FindName("buttonChangeUsername")));
            this.header = ((System.Windows.Controls.Image)(this.FindName("header")));
            this.nicknameDialog = ((mPuzzle.NicknameDialog)(this.FindName("nicknameDialog")));
        }
    }
}

