﻿using System;
using System.Linq;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.Util;
using Newtonsoft.Json;
using NHibernate.Mapping.Attributes;
using softwrench.sw4.problem.classes;
using CompressionUtil = cts.commons.Util.CompressionUtil;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.entities {

    [Class(Table = "BAT_BATCHITEM", Lazy = false)]
    public class BatchItem : IBaseEntity {

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id { get; set; }

        [Property]
        public DateTime? UpdateDate { get; set; }
        
        [Property(TypeType = typeof(BatchStatusType))]
        public BatchStatus Status { get; set; }

        [Property]
        public String Application { get; set; }

        /// <summary>
        /// The id of the item in Maximo
        /// </summary>
        [Property]
        public String ItemId { get; set; }

        [Property(Column = "schema_")]
        public String Schema { get; set; }

        [Property]
        public String Operation { get; set; }

        [Property]
        public String RemoteId { get; set; }

        [Property(Type = "BinaryBlob")]
        [JsonIgnore]
        public virtual byte[] DataMapJson { get; set; }

        [ManyToOne(Column = "problem_id", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False, Cascade = "none")]
        public virtual Problem Problem { get; set; }

        public virtual string DataMapJsonAsString {
            get {
                return DataMapJson == null ? null : StringExtensions.GetString(CompressionUtil.Decompress(DataMapJson));
            }
            set {
                DataMapJson = CompressionUtil.Compress(value.GetBytes());
            }
        }

    }


}
