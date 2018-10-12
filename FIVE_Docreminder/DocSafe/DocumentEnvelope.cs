using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace docreminder.DocSafe
{
    /// 
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.34283")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = true)]
    public partial class SafeIDAliasType
    {
        private string aliasScopeField;
        private string valueField;
        /// 
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string AliasScope
        {
            get
            {
                return this.aliasScopeField;
            }
            set
            {
                this.aliasScopeField = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }
    /// 
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.34283")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = true)]
    public partial class DocumentAmountType
    {
        private string currencyField;
        private string designatorField;
        private decimal valueField;
        /// 
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Currency
        {
            get
            {
                return this.currencyField;
            }
            set
            {
                this.currencyField = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Designator
        {
            get
            {
                return this.designatorField;
            }
            set
            {
                this.designatorField = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlTextAttribute()]
        public decimal Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }
    /// 
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.34283")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = true)]
    public partial class PaymentAmountType
    {
        private string currencyField;
        private decimal[] textField;
        /// 
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Currency
        {
            get
            {
                return this.currencyField;
            }
            set
            {
                this.currencyField = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal[] Text
        {
            get
            {
                return this.textField;
            }
            set
            {
                this.textField = value;
            }
        }
    }
    /// 
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.34283")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = true)]
    public partial class IBANType
    {
        private string bICField;
        private string valueField;
        /// 
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string BIC
        {
            get
            {
                return this.bICField;
            }
            set
            {
                this.bICField = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }
    /// 
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.34283")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class DocumentEnvelope
    {
        private Registration registrationField;
        private Properties propertiesField;
        private byte[] documentBytesField;
        private string mIMETypeField;
        private XMLMetaData xMLMetaDataField;
        private string sensitiveXMLMetaDataField;
        /// 
        public Registration Registration
        {
            get
            {
                return this.registrationField;
            }
            set
            {
                this.registrationField = value;
            }
        }
        /// 
        public Properties Properties
        {
            get
            {
                return this.propertiesField;
            }
            set
            {
                this.propertiesField = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "base64Binary")]
        public byte[] DocumentBytes
        {
            get
            {
                return this.documentBytesField;
            }
            set
            {
                this.documentBytesField = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string MIMEType
        {
            get
            {
                return this.mIMETypeField;
            }
            set
            {
                this.mIMETypeField = value;
            }
        }
        /// 
        public XMLMetaData XMLMetaData
        {
            get
            {
                return this.xMLMetaDataField;
            }
            set
            {
                this.xMLMetaDataField = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string SensitiveXMLMetaData
        {
            get
            {
                return this.sensitiveXMLMetaDataField;
            }
            set
            {
                this.sensitiveXMLMetaDataField = value;
            }
        }
    }
    /// 
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.34283")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Registration
    {
        private string senderBUIDField;
        private string sendersObjectIDField;
        private string sendersFlowRefField;
        private SafeIDAliasType safeIDAliasField;
        private string sendersObjectAliasField;
        private string sendersFlowAliasField;
        /// 
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string SenderBUID
        {
            get
            {
                return this.senderBUIDField;
            }
            set
            {
                this.senderBUIDField = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string SendersObjectID
        {
            get
            {
                return this.sendersObjectIDField;
            }
            set
            {
                this.sendersObjectIDField = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string SendersFlowRef
        {
            get
            {
                return this.sendersFlowRefField;
            }
            set
            {
                this.sendersFlowRefField = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public SafeIDAliasType SafeIDAlias
        {
            get
            {
                return this.safeIDAliasField;
            }
            set
            {
                this.safeIDAliasField = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string SendersObjectAlias
        {
            get
            {
                return this.sendersObjectAliasField;
            }
            set
            {
                this.sendersObjectAliasField = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string SendersFlowAlias
        {
            get
            {
                return this.sendersFlowAliasField;
            }
            set
            {
                this.sendersFlowAliasField = value;
            }
        }
    }
    /// 
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.34283")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Properties
    {
        private string sendersDocumentIDField;
        private string titleField;
        private string senderNameField;
        private System.DateTime effectiveDateField;
        private bool effectiveDateFieldSpecified;
        private System.DateTime expiryDateField;
        private bool expiryDateFieldSpecified;
        private System.DateTime suggestedDeletionDateField;
        private bool suggestedDeletionDateFieldSpecified;
        private string suggestedFilenameField;
        private string annotationField;
        private string linkTextField;
        private string linkURLField;
        private System.DateTime creationTSField;
        private bool creationTSFieldSpecified;
        private bool allowsForwardField;
        private bool allowsForwardFieldSpecified;
        private DocumentAmountType amountField;
        /// 
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string SendersDocumentID
        {
            get
            {
                return this.sendersDocumentIDField;
            }
            set
            {
                this.sendersDocumentIDField = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string SenderName
        {
            get
            {
                return this.senderNameField;
            }
            set
            {
                this.senderNameField = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "date")]
        public System.DateTime EffectiveDate
        {
            get
            {
                return this.effectiveDateField;
            }
            set
            {
                this.effectiveDateField = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool EffectiveDateSpecified
        {
            get
            {
                return this.effectiveDateFieldSpecified;
            }
            set
            {
                this.effectiveDateFieldSpecified = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "date")]
        public System.DateTime ExpiryDate
        {
            get
            {
                return this.expiryDateField;
            }
            set
            {
                this.expiryDateField = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExpiryDateSpecified
        {
            get
            {
                return this.expiryDateFieldSpecified;
            }
            set
            {
                this.expiryDateFieldSpecified = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "date")]
        public System.DateTime SuggestedDeletionDate
        {
            get
            {
                return this.suggestedDeletionDateField;
            }
            set
            {
                this.suggestedDeletionDateField = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SuggestedDeletionDateSpecified
        {
            get
            {
                return this.suggestedDeletionDateFieldSpecified;
            }
            set
            {
                this.suggestedDeletionDateFieldSpecified = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string SuggestedFilename
        {
            get
            {
                return this.suggestedFilenameField;
            }
            set
            {
                this.suggestedFilenameField = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Annotation
        {
            get
            {
                return this.annotationField;
            }
            set
            {
                this.annotationField = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string LinkText
        {
            get
            {
                return this.linkTextField;
            }
            set
            {
                this.linkTextField = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string LinkURL
        {
            get
            {
                return this.linkURLField;
            }
            set
            {
                this.linkURLField = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public System.DateTime CreationTS
        {
            get
            {
                return this.creationTSField;
            }
            set
            {
                this.creationTSField = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CreationTSSpecified
        {
            get
            {
                return this.creationTSFieldSpecified;
            }
            set
            {
                this.creationTSFieldSpecified = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool AllowsForward
        {
            get
            {
                return this.allowsForwardField;
            }
            set
            {
                this.allowsForwardField = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AllowsForwardSpecified
        {
            get
            {
                return this.allowsForwardFieldSpecified;
            }
            set
            {
                this.allowsForwardFieldSpecified = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public DocumentAmountType Amount
        {
            get
            {
                return this.amountField;
            }
            set
            {
                this.amountField = value;
            }
        }
    }
    /// 
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.34283")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class XMLMetaData
    {
        private PaymentData paymentDataField;
        /// 
        public PaymentData PaymentData
        {
            get
            {
                return this.paymentDataField;
            }
            set
            {
                this.paymentDataField = value;
            }
        }
    }
    /// 
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.34283")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class PaymentData
    {
        private ESR eSRField;
        private string supportDocSafePaymentField;
        /// 
        public ESR ESR
        {
            get
            {
                return this.eSRField;
            }
            set
            {
                this.eSRField = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string supportDocSafePayment
        {
            get
            {
                return this.supportDocSafePaymentField;
            }
            set
            {
                this.supportDocSafePaymentField = value;
            }
        }
    }
    /// 
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.34283")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class ESR
    {
        private string participantIDField;
        private Participant participantField;
        private Beneficiary beneficiaryField;
        private PaymentAmountType amountField;
        private string referenceField;
        private System.DateTime dueDateField;
        private bool dueDateFieldSpecified;
        /// 
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ParticipantID
        {
            get
            {
                return this.participantIDField;
            }
            set
            {
                this.participantIDField = value;
            }
        }
        /// 
        public Participant Participant
        {
            get
            {
                return this.participantField;
            }
            set
            {
                this.participantField = value;
            }
        }
        /// 
        public Beneficiary Beneficiary
        {
            get
            {
                return this.beneficiaryField;
            }
            set
            {
                this.beneficiaryField = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public PaymentAmountType Amount
        {
            get
            {
                return this.amountField;
            }
            set
            {
                this.amountField = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Reference
        {
            get
            {
                return this.referenceField;
            }
            set
            {
                this.referenceField = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "date")]
        public System.DateTime DueDate
        {
            get
            {
                return this.dueDateField;
            }
            set
            {
                this.dueDateField = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DueDateSpecified
        {
            get
            {
                return this.dueDateFieldSpecified;
            }
            set
            {
                this.dueDateFieldSpecified = value;
            }
        }
    }
    /// 
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.34283")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Participant
    {
        private string addr1Field;
        private string addr2Field;
        private string addr3Field;
        private string addr4Field;
        /// 
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Addr1
        {
            get
            {
                return this.addr1Field;
            }
            set
            {
                this.addr1Field = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Addr2
        {
            get
            {
                return this.addr2Field;
            }
            set
            {
                this.addr2Field = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Addr3
        {
            get
            {
                return this.addr3Field;
            }
            set
            {
                this.addr3Field = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Addr4
        {
            get
            {
                return this.addr4Field;
            }
            set
            {
                this.addr4Field = value;
            }
        }
    }
    /// 
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.34283")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Beneficiary
    {
        private IBANType iBANField;
        private string addr1Field;
        private string addr2Field;
        private string addr3Field;
        private string addr4Field;
        /// 
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public IBANType IBAN
        {
            get
            {
                return this.iBANField;
            }
            set
            {
                this.iBANField = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Addr1
        {
            get
            {
                return this.addr1Field;
            }
            set
            {
                this.addr1Field = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Addr2
        {
            get
            {
                return this.addr2Field;
            }
            set
            {
                this.addr2Field = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Addr3
        {
            get
            {
                return this.addr3Field;
            }
            set
            {
                this.addr3Field = value;
            }
        }
        /// 
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Addr4
        {
            get
            {
                return this.addr4Field;
            }
            set
            {
                this.addr4Field = value;
            }
        }
    }
}