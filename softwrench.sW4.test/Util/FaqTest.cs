using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Applications.DataSet.Faq;

namespace softwrench.sW4.test.Util {
    [TestClass]
    public class FaqTest {
        [TestMethod]
        public void SimulateFaqRealData() {
            var faqDescriptionList = GetFaqDescriptions();
            LanguagesTest(faqDescriptionList);
            CategoriesTest(faqDescriptionList);
            DescriptionTest(faqDescriptionList);
            IdsTest(faqDescriptionList);
        }

        private static void LanguagesTest(IReadOnlyList<FaqDescription> faqDescriptionList) {
            if (faqDescriptionList == null) throw new ArgumentNullException("faqDescriptionList");
            Assert.IsTrue(faqDescriptionList[0].Language.Equals("E")); // english
            Assert.IsFalse(faqDescriptionList[6].Language.Equals("E")); // correct is german, not english
            Assert.IsTrue(faqDescriptionList[6].Language.Equals("G")); // german
            Assert.IsTrue(faqDescriptionList[8].Language.Equals("S")); // spanish
        }

        private static void CategoriesTest(IReadOnlyList<FaqDescription> faqDescriptionList) {
            if (faqDescriptionList == null) throw new ArgumentNullException("faqDescriptionList");
            Assert.IsTrue(faqDescriptionList[0].Categories[0].Contains("Windows 7"));
            Assert.IsTrue(faqDescriptionList[4].Categories[1].Contains("Tips"));
            Assert.IsFalse(faqDescriptionList[4].Categories[1].StartsWith("General"));
            Assert.IsTrue(faqDescriptionList[5].Categories[0].Contains("Outlook"));
            Assert.IsFalse(faqDescriptionList[5].Categories[0].Contains("General"));
            Assert.IsTrue(faqDescriptionList[5].Categories[1].Contains("General"));
        }

        private static void DescriptionTest(IReadOnlyList<FaqDescription> faqDescriptionList) {
            if (faqDescriptionList == null) throw new ArgumentNullException("faqDescriptionList");
            Assert.IsTrue(faqDescriptionList[1].RealDescription.EndsWith("Login problems"));
            Assert.IsTrue(faqDescriptionList[7].RealDescription.EndsWith("New Contact/User/Vessel not in GAL"));
        }

        private static void IdsTest(IReadOnlyList<FaqDescription> faqDescriptionList) {
            if (faqDescriptionList == null) throw new ArgumentNullException("faqDescriptionList");
            Assert.IsTrue(Int32.Parse(faqDescriptionList[1].Id).Equals(18));
            Assert.IsNotNull(faqDescriptionList[10].Id);
        }

        private static List<FaqDescription> GetFaqDescriptions() {
            return new List<FaqDescription>()
            {
                //0
                new FaqDescription("E0006_Windows 7 | Set A4 as default setting"),
                //1
                new FaqDescription("E0018_Webmail | Login problems"),
                //2
                new FaqDescription("E0007_Webmail | Recover deleted items"),
                //3
                new FaqDescription("E0008_Webmail | Change mailbox folder's language"),
                //4
                new FaqDescription("E0005_Outlook / Tips | Calendar tips"),
                //5
                new FaqDescription("E0004_Outlook / General | Deleted BB Mails not synchronized"),
                //6
                new FaqDescription("G0016_Outlook / General | Einladung für Besprechung funktioniert nicht"),
                //7
                new FaqDescription("E0015_Outlook / General | New Contact/User/Vessel not in GAL"),
                //8
                new FaqDescription("S0008_Webmail | Cambio del idioma de la carpeta de correo"),
                //9
                new FaqDescription("S0011_Outlook / General | 'Enviar en nombre' de un buzón de grupo"),
                //10
                new FaqDescription("S0012_Outlook / General | Correos con archivos adjuntos archivados")
            };
        }
    }
}
