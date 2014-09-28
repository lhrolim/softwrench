using System;
using System.Drawing;
using MonoTouch.CoreAnimation;
using MonoTouch.UIKit;
using softWrench.iOS.Utilities;
using softWrench.Mobile.Data;
using softWrench.Mobile.Metadata.Applications;

using softWrench.Mobile.Metadata.Extensions;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.iOS.Views {
    public partial class ListCell : UITableViewCell {
        public ListCell(IntPtr handle)
            : base(handle) {
        }

        public void Construct(ApplicationSchemaDefinition application, DataMap dataMap) {
            Show(application, dataMap);
            BorderAround();
        }

        private void WarnIfBouncing(DataMap dataMap) {
            if (title.Hidden) {
                return;
            }

            var isBouncing = dataMap
                .LocalState
                .IsBouncing;

            // If this item is facing synchronization
            // issues, we should tell the user right away.
            title.TextColor = isBouncing
                ? Theme.ErrorColor
                : Theme.TextColor;

            title.SizeToFit();
            bounce.Hidden = false == isBouncing;

            if (isBouncing) {
                bounce.Image = Theme.ErrorIcon;
                bounce.Frame = bounce
                    .Frame
                    .Resize(x: title.Frame.X + title.Frame.Width + 6);
            }
        }

        private void Show(ApplicationSchemaDefinition application, DataMap dataMap) {
            var previewTitle = application.PreviewTitle();
            if (null != previewTitle) title.Text = dataMap.Value(previewTitle.Attribute);

            var previewSubtitle = application.PreviewSubtitle();
            if (null != previewSubtitle) subtitle.Text = dataMap.Value(previewSubtitle.Attribute);

            var previewFeatured = application.PreviewFeatured();
            if (null != previewFeatured) featured.Text = dataMap.Value(previewFeatured.Attribute);

            var previewExcerpt = application.PreviewExcerpt();
            if (null != previewExcerpt) excerpt.Text = dataMap.Value(previewExcerpt.Attribute);

            title.Hidden = (null == previewTitle);
            subtitle.Hidden = (null == previewSubtitle);
            featured.Hidden = (null == previewFeatured);
            featuredBackground.Hidden = (null == previewFeatured);
            excerpt.Hidden = (null == previewExcerpt);

            if (null != previewFeatured) {
                //                featuredBackground.Image = Theme.NumberBox;
                featuredBackground.BackgroundColor = GetColorFromStatus(featured.Text);
                featuredBackground.TintColor = UIColor.White;
            }

            if (false == string.IsNullOrEmpty(subtitle.Text)) {
                subtitle.Text = subtitle.Text.ToLower();
            }

            if (false == string.IsNullOrEmpty(excerpt.Text)) {
                excerpt.Text = excerpt.Text.ToLower();
            }

            WarnIfBouncing(dataMap);
        }

        //TODO: This is a pog for Pulse Environment redo it later
        private static UIColor GetColorFromStatus(string text) {
            if (text == "APPR" || text == "INPRG") {
                return UIColor.Green;
            }
            if (text == "WPCOND" || text == "WMATL" || text == "WSCH") {
                return UIColor.Orange;
            }
            if (text == "CAN") {
                return UIColor.Red;
            }
            if (text == "CLOSE" || text == "COMP" || text == "HISTEDIT") {
                return UIColor.Gray;
            }

            if (text == "WAPPR") {
                return UIColor.Yellow;
            }

            //            APPR Approved = GREEN
            //WPCOND Waiting on Plant Cond = ORANGE
            //CAN Canceled = RED
            //CLOSE Closed = GRAY
            //COMP Completed = GRAY
            //HISTEDIT Edited in History = GRAY
            //INPRG In Progress = GREEN
            //WAPPR Waiting on Approva = YELLOW
            //WMATL Waiting on Material = ORANGE
            //WSCH Waiting to be Scheduled = ORANGE

            return UIColor.Gray;
        }

        private void BorderAround() {
            var border = new CALayer();
            border.Frame = new RectangleF(0, 87, 321, 1);
            border.BackgroundColor = Theme.BorderColor;
            ContentView.Layer.AddSublayer(border);
        }
    }
}
