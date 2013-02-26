// MvxTabBarViewController.cs
// (c) Copyright Cirrious Ltd. http://www.cirrious.com
// MvvmCross is licensed using Microsoft Public License (Ms-PL)
// Contributions and inspirations noted in readme.md and license.txt
// 
// Project Lead - Stuart Lodge, @slodge, me@slodge.com

using System.Collections.Generic;
using Cirrious.CrossCore.Touch;
using Cirrious.CrossCore.Touch.Views;
using Cirrious.MvvmCross.Binding.Interfaces;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.Touch.Interfaces;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Binding.Interfaces.BindingContext;
using MonoTouch.UIKit;

namespace Cirrious.MvvmCross.Touch.Views
{
    public class MvxTabBarViewController
        : MvxEventSourceTabBarController
          , IMvxBindingTouchView
    {
        protected MvxTabBarViewController()
        {
            this.AdaptForBinding();
        }

		public object DataContext { 
			get
			{
				// special code needed in TabBar because View is initialised during construction
				if (BindingContext == null) return null;
				return BindingContext.DataContext; 
			}
			set { BindingContext.DataContext = value; }
		}

        public IMvxViewModel ViewModel
        {
            get { return (IMvxViewModel) DataContext; }
            set { DataContext = value; }
        }

        public bool IsVisible
        {
            get { return this.IsVisible(); }
        }

        public MvxShowViewModelRequest ShowRequest { get; set; }

		public IMvxBaseBindingContext<UIView> BindingContext { get; set; }
	}
}