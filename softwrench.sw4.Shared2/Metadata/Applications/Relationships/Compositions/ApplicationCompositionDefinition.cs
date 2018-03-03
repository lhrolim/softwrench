﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using softwrench.sw4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations;
using softwrench.sw4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Util;
using softwrench.sw4.Shared2.Metadata.Applications.Schema.Interfaces;

namespace softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions {

    public class ApplicationCompositionDefinition : ApplicationRelationshipDefinition, IDependableField, IPCLCloneable {


        private string _relationship;
        public bool isHidden {
            get; set;
        }
        [DefaultValue(true)]
        public bool isPrintEnabled {
            get; set;
        }

        public ApplicationHeader Header {
            get; set;
        }

        /// <summary>
        /// if this is true, then the composition is not really a relationship to another application, but rather one 
        /// whose data should come from the datamap itself.
        /// 
        /// If the attribute has no such value, than the table will be a null table
        /// 
        /// The list and detail schemas shall point to the same application in that case.
        /// 
        /// </summary>
        public bool IsSelfRelationship => Relationship.StartsWith("#");

        public ApplicationCompositionDefinition() {

        }

        public ApplicationCompositionDefinition(string @from, string relationship, string label, ApplicationCompositionSchema schema, string showExpression, string toolTip, bool hidden, bool printEnabled, ApplicationHeader header)
            : base(from, label, showExpression, toolTip, null) {
            if (relationship == null)
                throw new ArgumentNullException("relationship");
            _relationship = relationship;
            Schema = schema;
            isHidden = hidden;
            isPrintEnabled = printEnabled;
            Header = header;
            if (isHidden) {
                //if hidden then the detail schema can be marked as empty
                Schema.DetailSchema = "";
            }
            Collection = schema is ApplicationCompositionCollectionSchema;
        }


        public string Relationship {
            get {
                return EntityUtil.GetRelationshipName(_relationship);
            }
            set {
                _relationship = value;
            }
        }

        public override string RendererType => Schema.Renderer.RendererType;

        public override string Role => From + "." + Relationship;

        public override string Attribute {
            get {
                return _relationship;
            }
            set {
            }
        }


        public ApplicationCompositionSchema Schema { get; set; }

        public bool Inline => Schema.INLINE;

        public override string ToString() {
            return $"From: {From}, To: {EntityAssociation} , Collection: {Collection}";
        }

        public string TabId => Relationship;

        public ISet<string> DependantFields => Schema.DependantFields;

        public override bool Collection { get; }

        public string AssociationKey => Relationship;


        public object Clone() {
            var cloned = new ApplicationCompositionDefinition(From, Relationship, Label, Schema, ShowExpression, ToolTip, isHidden, isPrintEnabled, Header);
            cloned.Schema = (ApplicationCompositionSchema) cloned.Schema.Clone();
            cloned.SetLazyResolver(LazyEntityAssociation);
            return cloned;

        }


    }
}
