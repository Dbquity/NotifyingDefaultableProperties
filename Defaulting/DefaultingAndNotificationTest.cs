using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Dbquity {
    [TestClass]
    public class DefaultingAndNotificationTest {
        static List<string> CreateItemPropertyNotificationLog(Item i) {
            List<string> log = new List<string>();
            i.PropertyChanging += (s, e) =>
            log.Add($"-{e.PropertyName}: {((IPropertyOwner)s)[e.PropertyName]}");
            i.PropertyChanged += (s, e) =>
            log.Add($"+{e.PropertyName}: {((IPropertyOwner)s)[e.PropertyName]}");
            return log;
        }
        [TestMethod]
        public void DefaultsAndPropertyChangeNotifications() {
            Item i = new Item() { Name = "Testing, testing ..." };

            Assert.IsFalse(i.CanBeDefaulted(nameof(Item.Name)));
            Assert.IsTrue(i.CanBeDefaulted(nameof(Item.Label)));

            Assert.IsTrue(i.CostIsDefaulted);
            Assert.AreEqual(43L, i.Cost);

            List<string> log = CreateItemPropertyNotificationLog(i);
            i.Cost = 89L;
            Assert.IsFalse(i.CostIsDefaulted);
            Assert.AreEqual(89L, i.Cost);
            CollectionAssert.AreEqual(new[] { "-CostIsDefaulted: True", "-Cost: 43", "+Cost: 89", "+CostIsDefaulted: False" }, log);

            log.Clear();
            i["Cost"] = Item.CostDefault;
            Assert.IsFalse(i.CostIsDefaulted);
            Assert.AreEqual(43L, i.Cost);
            CollectionAssert.AreEqual(new[] { "-Cost: 89", "+Cost: 43" }, log);

            log.Clear();
            i["Cost"] = null;
            Assert.IsTrue(i.CostIsDefaulted);
            Assert.AreEqual(43L, i.Cost);
            CollectionAssert.AreEqual(new[] { "-CostIsDefaulted: False", "-Cost: 43", "+Cost: 43", "+CostIsDefaulted: True" }, log);

            log.Clear();
            i.SetToDefault(nameof(Item.Cost));
            Assert.AreEqual(0, log.Count);

            Assert.IsTrue(i.LabelIsDefaulted);
            Assert.AreEqual("<label it>", i.Label);

            log.Clear();
            i.Label = "Well known";
            Assert.IsFalse(i.LabelIsDefaulted);
            Assert.AreEqual("Well known", i.Label);
            CollectionAssert.AreEqual(
                new[] { "-LabelIsDefaulted: True", "-Label: <label it>", "+Label: Well known", "+LabelIsDefaulted: False" }, log);

            log.Clear();
            i.Label = Item.LabelDefault;
            Assert.IsFalse(i.LabelIsDefaulted);
            Assert.AreEqual("<label it>", i.Label);
            CollectionAssert.AreEqual(new[] { "-Label: Well known", "+Label: <label it>" }, log);

            log.Clear();
            i.SetToDefault(nameof(Item.Label));
            Assert.IsTrue(i.LabelIsDefaulted);
            Assert.AreEqual(Item.LabelDefault, i.Label);
            CollectionAssert.AreEqual(
                new[] { "-LabelIsDefaulted: False", "-Label: <label it>", "+Label: <label it>", "+LabelIsDefaulted: True" }, log);

            log.Clear();
            i.Name = null;
            CollectionAssert.AreEqual(new[] { "-Name: Testing, testing ...", "+Name: " }, log);
        }
        [TestMethod]
        public void DelayedPropertyChangeNotifications() {
            Item i = new Item();
            List<string> log = CreateItemPropertyNotificationLog(i);
            i.Name = "Good name"; // ⚡Name Changing, Changed
            log.Add("after Name");
            i.Cost = 1; // ⚡Cost and CostIsDefaulted Changing, Changed
            log.Add("after Cost");
            CollectionAssert.AreEqual(new[] { "-Name: ", "+Name: Good name", "after Name",
                "-CostIsDefaulted: True", "-Cost: 43", "+Cost: 1", "+CostIsDefaulted: False", "after Cost" }, log);

            log.Clear();
            using (i.DelayedPropertyChangeNotification()) {
                i.Name = "No name"; // ⚡Name Changing
                log.Add("after Name");
                i.Cost = 2; // ⚡Cost Changing
                log.Add("after Cost");
                i.Name = "Another name"; // no ⚡
                CollectionAssert.AreEqual(new[] { "-Name: Good name", "after Name", "-Cost: 1", "after Cost" }, log);
                log.Clear();
            }// ⚡Name and Cost Changed
            CollectionAssert.AreEqual(new[] { "+Name: Another name", "+Cost: 2" }, log);
        }
    }
}