﻿namespace softWrench.sW4.Data.Persistence.Dataset.Commons.Maximo
{
    public sealed class AttachmentDTO {

        public string Title { get; set; }

        public string Description { get; set; }

        public string Path { get; set; }

        public string Data { get; set; }

        public byte[] BinaryData { get; set; }

        /// <summary>
        /// this is a hash used to uniquely indentify an attachment file on an offline device so that it is not downloaded  afterwards 
        /// if it has been created out of that particular device
        /// </summary>
        public string OffLineHash { get; set; }
    }
}
