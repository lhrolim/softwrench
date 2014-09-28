using System;
using softWrench.iOS.Behaviors;
using softWrench.iOS.Controllers;
using softWrench.iOS.Utilities;
using softWrench.Mobile.Behaviors;
using softWrench.Mobile.Data;
using softWrench.Mobile.Metadata.Parsing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Threading.Tasks;

namespace softWrench.iOS.Applications.WorkOrder {
    internal class AddPhotoCommand : IApplicationCommand {
        // TODO: How we should reference the attachments
        // application? By name, entity, guid... ?
        private const string AttachmentApplicationName = "attachment";

        /// <summary>
        ///     The string used as key in the <seealso cref="DataMap.CustomFields"/>
        ///     dictionary for storing the actual image, encoded as a base-64 string.
        /// </summary>
        /// <remarks>
        ///     The key is serialized as Json, so make sure
        ///     it follows the casing convention according
        ///     to <seealso cref="JsonParser.SerializerSettings"/>.
        /// </remarks>
        private const string CustomFieldKey = "image";

        /// <summary>
        ///     Creates a new attachment data map using
        ///     the specified image.
        /// </summary>
        /// <param name="arguments">The application command arguments.</param>
        /// <param name="image">The image.</param>
        private static async Task<DataMap> CreateAttachment(IApplicationCommandArguments arguments, UIImage image) {
            var imageAsBase64 = image
               .AsPNG()
               .GetBase64EncodedString(NSDataBase64EncodingOptions.None);

            var attachmentMetadata = await arguments
                .MetadataRepository
                .LoadApplicationAsync(AttachmentApplicationName);

            var attachment = await arguments
                .DataRepository
                .NewAsync(attachmentMetadata, arguments.Composite);

            attachment.Value("document", "NewImage");
            attachment.Value("createdate", DateTime.Now.ToString("MM-dd-yyyy hh:mm:ss"));
            attachment.Value("newattachment", imageAsBase64);

            //attachment.CustomFields[CustomFieldKey] = imageAsBase64;

            return attachment;
        }


        /// <summary>
        ///     Invoked when an image is taken or selected by the user.
        ///     The image is then dumped to a new attachment data map. 
        /// </summary>
        /// <param name="imageData">The arguments returned by the UIImagePickerControllerDelegate.</param>
        /// <param name="commandArguments">The arguments of the original application command.</param>
        private async void OnImagePicked(NSDictionary imageInfo, ApplicationCommandArguments commandArguments) {
            // Did the user cropped the image or performed
            // any other edition on the original image? If
            // so, we'll use this version.
            var image = imageInfo[UIImagePickerController.EditedImage] as UIImage;

            // If he didn't, let's use the original image.
            if (null == image) {
                image = imageInfo[UIImagePickerController.OriginalImage] as UIImage;
            }

            if (null == image) {
                return;
            }

            var attachment = await CreateAttachment(commandArguments, image);

            // Let's instruct the controller to activate and
            // display the new attachment on the screen so the
            // user can complement it and commit the operation.
            ((DetailController)commandArguments
                .Controller)
                .ShowSegment(AttachmentApplicationName, attachment);
        }

        /// <seealso cref="IApplicationCommand.Execute"/>
        public void Execute(IApplicationCommandArguments arguments) {
            var args = (ApplicationCommandArguments)arguments;

            // Do we have a camera, boss?
            if (false == Camera.CanTakePicture()) {
                Alert.Show("Oops...", "We couldn't find an available camera on this device. Is it available?");
                return;
            }
            //            OnImagePicked(new NSDictionary(), args);
            Camera.TakePicture(args.Controller, info => OnImagePicked(info, args));
        }

        /// <seealso cref="IApplicationCommand.Register"/>
        public void Register(OnBeforeShowContext context, DataMap dataMap) {
            context.RegisterCommand(this);
        }

        /// <seealso cref="IApplicationCommand.IsAvailable"/>
        public bool IsAvailable(DataMap dataMap) {
            return true;
        }

        public string Name {
            get { return GetType().FullName; }
        }

        public string Label {
            get { return "Take a Photo"; }
        }

        public string Subtitle {
            get { return "Take a photo using your camera and attach it to this WO."; }
        }

        public string Title {
            get { return "Take a Photo"; }
        }

        public bool IsAvailableOnNew {
            get { return false; }
        }

        public bool IsAvailableOnLocal {
            get { return false; }
        }

        public void Dispose() {
        }
    }
}