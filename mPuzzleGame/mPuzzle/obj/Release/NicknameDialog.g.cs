﻿#pragma checksum "E:\projects\win-phone\mPuzzle-latest\mPuzzle\NicknameDialog.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "B6D8B0C1005355E26ADCDE0BAD04A3E5"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.17929
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

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


namespace mPuzzle {
    
    
    public partial class NicknameDialog : System.Windows.Controls.UserControl {
        
        internal System.Windows.Controls.StackPanel nameInput;
        
        internal System.Windows.Controls.TextBox username;
        
        internal System.Windows.Controls.TextBlock statusText;
        
        internal System.Windows.Media.Animation.Storyboard statusTextAnimation;
        
        internal System.Windows.Controls.Button buttonOk;
        
        internal System.Windows.Controls.Button buttonCancel;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/mPuzzle;component/NicknameDialog.xaml", System.UriKind.Relative));
            this.nameInput = ((System.Windows.Controls.StackPanel)(this.FindName("nameInput")));
            this.username = ((System.Windows.Controls.TextBox)(this.FindName("username")));
            this.statusText = ((System.Windows.Controls.TextBlock)(this.FindName("statusText")));
            this.statusTextAnimation = ((System.Windows.Media.Animation.Storyboard)(this.FindName("statusTextAnimation")));
            this.buttonOk = ((System.Windows.Controls.Button)(this.FindName("buttonOk")));
            this.buttonCancel = ((System.Windows.Controls.Button)(this.FindName("buttonCancel")));
        }
    }
}

