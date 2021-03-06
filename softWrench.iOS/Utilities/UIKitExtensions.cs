//
//  Copyright 2012  Xamarin Inc.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace softWrench.iOS.Utilities
{
	/// <summary>
	/// Helper class with extension methods
	/// </summary>
	public static class UIKitExtensions
	{
		/// <summary>
		/// Sets a callback for registering text changed notifications on a UITextField
		/// </summary>
		public static void SetDidChangeNotification (this UITextField textField, Action<UITextField> callback)
		{
			if (callback == null)
				throw new ArgumentNullException ("callback");

			NSNotificationCenter.DefaultCenter.AddObserver (UITextField.TextFieldTextDidChangeNotification, _ => callback (textField), textField);
		}

		/// <summary>
		/// Sets a callback for registering text changed notifications on a UITextField
		/// </summary>
		public static void SetDidChangeNotification (this UITextView textView, Action<UITextView> callback)
		{
			if (callback == null)
				throw new ArgumentNullException ("callback");
			
			NSNotificationCenter.DefaultCenter.AddObserver (UITextView.TextDidChangeNotification, _ => callback (textView), textView);
		}

		/// <summary>
		/// Returns true if is landscape
		/// </summary>
		public static bool IsLandscape (this UIInterfaceOrientation orientation)
		{
			return orientation == UIInterfaceOrientation.LandscapeLeft || orientation == UIInterfaceOrientation.LandscapeRight;
		}

		/// <summary>
		/// Returns true if is portrait
		/// </summary>
		public static bool IsPortrait (this UIInterfaceOrientation orientation)
		{
			return orientation == UIInterfaceOrientation.Portrait || orientation == UIInterfaceOrientation.PortraitUpsideDown;
		}

		/// <summary>
		/// Loads a UIImage from a byte array
		/// </summary>
		public static UIImage ToUIImage (this byte[] bytes)
		{
			if (bytes == null)
				return null;

			using (var data = NSData.FromArray (bytes)) {
				return UIImage.LoadFromData (data);
			}
		}

		/// <summary>
		/// Loads a UIImage from a Stream
		/// </summary>
		public static UIImage ToUIImage (this System.IO.Stream stream)
		{
			if (stream == null)
				return null;

			using (var data = NSData.FromStream (stream)) {
				return UIImage.LoadFromData (data);
			}
		}

		/// <summary>
		/// Converts a UIImage to a byte array
		/// </summary>
		public static byte[] ToByteArray (this UIImage image)
		{
			if (image == null)
				return null;

			using (image) {
				using (var data = image.AsJPEG ()) {
					byte[] bytes = new byte[data.Length];
					Marshal.Copy (data.Bytes, bytes, 0, (int)data.Length);
					return bytes;
				}
			}
		}

		/// <summary>
		/// Awesome helper method to instantiate a view controller and use the type name for the id
		/// </summary>
		public static T InstantiateViewController<T>(this UIStoryboard storyboard)
			where T : UIViewController
		{
			return (T)storyboard.InstantiateViewController (typeof(T).Name);
		}

        public static RectangleF Resize(this RectangleF rectangle, float? x = null, float? y = null, float? width = null, float? height = null)
        {
            var copy = new RectangleF(
                           x ?? rectangle.X,
                           y ?? rectangle.Y,
                           width ?? rectangle.Width,
                           height ?? rectangle.Height);

            return copy;
        }
	}
}

