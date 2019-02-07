using Dbquity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Defaulting {
    [TestClass]
    public class DefaultingAndNotificationTest {
        static List<string> CreateItemPropertyNotificationLog(IPropertyOwner i) {
            List<string> log = new List<string>();
            i.PropertyChanging += (s, e) =>
            log.Add($"-{e.PropertyName}: {((IPropertyOwner)s)[e.PropertyName]}");
            i.PropertyChanged += (s, e) =>
            log.Add($"+{e.PropertyName}: {((IPropertyOwner)s)[e.PropertyName]}");
            return log;
        }
        [TestMethod]
        public void DefaultsAndPropertyChangeNotifications() {
            dynamic i = new TestItems.Item() { Name = "Testing, testing ..." };
            IPropertyOwner ipo = (IPropertyOwner)i;

            Assert.IsTrue(i.CostIsDefaulted);
            Assert.AreEqual(43L, i.Cost);

            List<string> log = CreateItemPropertyNotificationLog(i);
            i.Cost = 89L;
            Assert.IsFalse(i.CostIsDefaulted);
            Assert.AreEqual(89L, i.Cost);
            CollectionAssert.AreEqual(new[] { "-Cost: 43", "-CostIsDefaulted: True", "+Cost: 89", "+CostIsDefaulted: False" }, log);

            log.Clear();
            i["Cost"] = TestItems.ItemBase.CostDefault;
            Assert.IsFalse(i.CostIsDefaulted);
            Assert.AreEqual(43L, i.Cost);
            CollectionAssert.AreEqual(new[] { "-Cost: 89", "+Cost: 43" }, log);

            log.Clear();
            i["Cost"] = null;
            Assert.IsTrue(i.CostIsDefaulted);
            Assert.AreEqual(43L, i.Cost);
            CollectionAssert.AreEqual(new[] { "-Cost: 43", "-CostIsDefaulted: False", "+Cost: 43", "+CostIsDefaulted: True" }, log);

            log.Clear();
            ipo.SetToDefault(nameof(i.Cost));
            Assert.AreEqual(0, log.Count);

            Assert.IsTrue(i.LabelIsDefaulted);
            Assert.AreEqual("<label it>", i.Label);

            log.Clear();
            i.Label = "Well known";
            Assert.IsFalse(i.LabelIsDefaulted);
            Assert.AreEqual("Well known", i.Label);
            CollectionAssert.AreEqual(
                new[] { "-Label: <label it>", "-LabelIsDefaulted: True", "+Label: Well known", "+LabelIsDefaulted: False" }, log);

            log.Clear();
            i.Label = TestItems.ItemBase.LabelDefault;
            Assert.IsFalse(i.LabelIsDefaulted);
            Assert.AreEqual("<label it>", i.Label);
            CollectionAssert.AreEqual(new[] { "-Label: Well known", "+Label: <label it>" }, log);

            log.Clear();
            ipo.SetToDefault(nameof(i.Label));
            Assert.IsTrue(i.LabelIsDefaulted);
            Assert.AreEqual(TestItems.ItemBase.LabelDefault, i.Label);
            CollectionAssert.AreEqual(
                new[] { "-Label: <label it>", "-LabelIsDefaulted: False", "+Label: <label it>", "+LabelIsDefaulted: True" }, log);

            log.Clear();
            i.Name = null;
            CollectionAssert.AreEqual(
                new[] { "-Name: Testing, testing ...", "+Name: " }, log);
        }
        [TestMethod]
        public void DelayedPropertyChangeNotifications() {
            dynamic i = new TestItems.Item();
            IPropertyOwner ipo = (IPropertyOwner)i;
            List<string> log = CreateItemPropertyNotificationLog(i);
            i.Name = "Good name"; // ⚡Name Changing, Changed
            log.Add("after Name");
            i.Cost = 1; // ⚡Cost and CostIsDefaulted Changing, Changed
            log.Add("after Cost");
            CollectionAssert.AreEqual(new[] {
                "-Name: ", "+Name: Good name", "after Name",
                "-Cost: 43", "-CostIsDefaulted: True", "+Cost: 1", "+CostIsDefaulted: False", "after Cost" }, log);

            log.Clear();
            using (ipo.DelayedPropertyChangeNotification()) {
                i.Name = "No name"; // ⚡Name Changing
                log.Add("after Name");
                i.Cost = 2; // ⚡Cost Changing
                log.Add("after Cost");
                i.Name = "Another name"; // no ⚡
                CollectionAssert.AreEqual(new[] { "-Name: Good name", "after Name", "-Cost: 1", "after Cost" }, log);
                log.Clear();
            } // ⚡Name and Cost Changed
            CollectionAssert.AreEqual(new[] { "+Name: Another name", "+Cost: 2" }, log);

            Assert.AreEqual(default(decimal), i.Price);
            i.Price = 144M;
            Assert.AreEqual(144M, i.Price);
            log.Clear();
            i["Price"] = null;
            Assert.AreEqual(0M, i.Price);
            i.Price = 89;
            CollectionAssert.AreEqual(new[] { "-Price: 144", "+Price: 0", "-Price: 0", "+Price: 89" }, log);
        }
        [TestMethod]
        public void DelayedPropertyChangeNotificationsOnItemOnPropertyBag() {
            IPropertyOwner i = new TestItems.ItemOnPropertyBag();
            List<string> log = CreateItemPropertyNotificationLog(i);
            i["Name"] = "Good name"; // ⚡Name Changing, Changed
            log.Add("after Name");
            Assert.IsTrue(i.IsDefaulted("Cost"));
            i["Cost"] = 1; // ⚡Cost and CostIsDefaulted Changing, Changed
            Assert.IsFalse(i.IsDefaulted("Cost"));
            log.Add("after Cost");
            CollectionAssert.AreEqual(new[] {
                "-Name: ", "+Name: Good name", "after Name",
                "-Cost: 43", "-CostIsDefaulted: True", "+Cost: 1", "+CostIsDefaulted: False", "after Cost" }, log);

            log.Clear();
            using (i.DelayedPropertyChangeNotification()) {
                i["Name"] = "No name"; // ⚡Name Changing
                log.Add("after Name");
                i["Cost"] = 2; // ⚡Cost Changing
                log.Add("after Cost");
                i["Name"] = "Another name"; // no ⚡
                CollectionAssert.AreEqual(new[] { "-Name: Good name", "after Name", "-Cost: 1", "after Cost" }, log);
                log.Clear();
            } // ⚡Name and Cost Changed
            CollectionAssert.AreEqual(new[] { "+Name: Another name", "+Cost: 2" }, log);

            Assert.AreEqual(default(decimal), i["Price"]);
            i["Price"] = 144M;
            Assert.AreEqual(144M, i["Price"]);
            log.Clear();
            i["Price"] = null;
            Assert.AreEqual(0M, i["Price"]);
            i["Price"] = 89M;
            CollectionAssert.AreEqual(new[] { "-Price: 144", "+Price: 0", "-Price: 0", "+Price: 89" }, log);
        }
    }
}