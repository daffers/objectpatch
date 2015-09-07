namespace Tests
{
    using System.Collections.Generic;
    using NUnit.Framework;

    [TestFixture]
    public class PatchRequestBaseTests
    {
        [Test]
        public void UpdatesToCollectionTest()
        {
            var original = new Data();
            original.Custom = new List<string>() { "hello" };

            var editRequest = new EditDataRequest();
            editRequest.Custom = new List<string>() { "hello", "world" };

            bool wasUpdated = editRequest.TryUpdateFromMe(original);
            Assert.That(wasUpdated, Is.True);
        }

        [Test]
        public void UpdatingChildPropertyButNoChildValues()
        {
            var original = new Data();
            original.Child = new ChildData();

            var editRequest = new EditDataRequest();
            editRequest.Child = new EditChildDataRequest();

            bool wasUpdated = editRequest.TryUpdateFromMe(original);
            Assert.That(wasUpdated, Is.False);
        }

        [Test]
        public void UpdatingChildPropertyAndChildValues()
        {
            var original = new Data();
            original.Child = new ChildData();


            var editRequest = new EditDataRequest();
            var editChildDataRequest = new EditChildDataRequest();
            editChildDataRequest.Name = "new name";
            editRequest.Child = editChildDataRequest;

            bool wasUpdated = editRequest.TryUpdateFromMe(original);
            Assert.That(wasUpdated, Is.True);

            Assert.That(original.Child.Name, Is.EqualTo("new name"));
        }

        [Test]
        public void UpdatingASingleProperty()
        {
            var original = new Data();
            original.Header = "hello";
            var editRequest = new EditDataRequest();
            editRequest.Header = "new val";
            bool thing = editRequest.TryUpdateFromMe(original);

            Assert.That(thing, Is.True);

            Assert.That(original.Header, Is.EqualTo("new val"));
        }

        [Test]
        public void UpdatingTwoPropertiesTest()
        {

            var original = new Data();
            original.Header = "hello";
            original.Rating = 5;
            var editRequest = new EditDataRequest();
            editRequest.Header = "new val";
            editRequest.Rating = 3;

            bool thing = editRequest.TryUpdateFromMe(original);

            Assert.That(thing, Is.True);

            Assert.That(original.Header, Is.EqualTo("new val"));
            Assert.That(original.Rating, Is.EqualTo(3));
        }

        [Test]
        public void NoChangesDoesNotTriggerUpdate()
        {
            var original = new Data();
            var editRequest = new EditDataRequest();

            Assert.False(editRequest.TryUpdateFromMe(original));
        }

        [Test]
        public void NoChangesWhenValueDoesNotChange()
        {
            var original = new Data();
            original.Header = "hello";
            var editRequest = new EditDataRequest();
            editRequest.Header = "hello";

            Assert.False(editRequest.TryUpdateFromMe(original));
        }
    }
}