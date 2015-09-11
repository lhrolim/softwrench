using System;
using cts.commons.portable.Util;

namespace softWrench.sW4.Exceptions {
    public class MenuMetadataException : InvalidOperationException {


        public MenuMetadataException(string message) : base(message) {
        }

        public static MenuMetadataException MissingReferenceException(string referenceId) {
            throw new MenuMetadataException("menu with id {0} could not be located. please review your menu definition".Fmt(referenceId));
        }

        public static MenuMetadataException MissingIdException() {
            throw new MenuMetadataException("Id field is required for the reference tag ");
        }
    }
}
